using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace WM.Net
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerDiscovery
    {
        #region Variables

        /// <summary>
        /// The log.
        /// </summary>
        protected Logger _log;

        /// <summary>
        /// Gets the log.
        /// </summary>
        public Logger Log
        {
            set
            {
                _log = value;
            }
            protected get
            {
                return _log;
            }
        }

        /// <summary>
        /// The map containing as keys the ServerInfos describing discovered servers.
        /// </summary>
        private Dictionary<ServerInfo, DateTime> _serverInfos = new Dictionary<ServerInfo, DateTime>();

        /// <summary>
        /// The server discovery's worker thread.
        /// </summary>
        private Thread _thread;

        #region Internal state

        /// <summary>
        /// The possible Client states.
        /// </summary>
        public enum ServerDiscoveryState
        {
            Idle,
            Running,
            Stopping
        }

        /// <summary>
        /// Locking object for the Client state.
        /// </summary>
        private System.Object stateLock = new System.Object();

        /// <summary>
        /// The server discovery state.
        /// </summary>
        public ServerDiscoveryState State
        {
            get;
            private set;
        } = ServerDiscoveryState.Idle;

        /// <summary>
        /// The server discovery state, as a string.
        /// </summary>
        public string StateString
        {
            get
            {
                switch (State)
                {
                    case ServerDiscoveryState.Idle:
                        return "Idle";
                    case ServerDiscoveryState.Stopping:
                        return "Stopping";
                    case ServerDiscoveryState.Running:
                        return "Running";
                    default:
                        return "Unknown server discovery state '" + State.ToString() + "'";
                }
            }
        }

        #endregion Internal state

        #endregion Variables

        #region Public API

        /// <summary>
        /// Sets the log.
        /// </summary>
        public void SetLog(Logger log)
        {
            _log = log;
        }

        /// <summary>
        /// Starts the server discovery.
        /// 
        /// \pre The ServerDiscovery must be in state 'Idle' for this method to succeed.
        /// </summary>
        public void Start()
        {
            _log.Debug("ServerDiscovery.Start()");

            lock (stateLock)
            {
                switch (State)
                {
                    case ServerDiscoveryState.Idle:
                        InternalStart();
                        break;
                    case ServerDiscoveryState.Stopping:
                        throw new Exception("Start() called on a stopping server discovery.");
                    case ServerDiscoveryState.Running:
                        throw new Exception("Start() called on a running server discovery.");
                }
            }
        }

        /// <summary>
        /// Stops the server discovery.
        /// 
        /// \pre The ServerDiscovery must be in state 'Running' for this method to succeed.
        /// </summary>
        public void Stop()
        {
            _log.Debug("ServeDiscovery.Stop()");

            lock (stateLock)
            {
                switch (State)
                {
                    case ServerDiscoveryState.Running:
                        InternalStop();
                        break;
                    case ServerDiscoveryState.Idle:
                        break; // throw new Exception("Stop() called on an idle server discovery.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ServerInfo> GetServerInfos()
        { 
            lock (_serverInfos)
            {
                return new List<ServerInfo>(_serverInfos.Keys);
            }
        }

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// 
        /// </summary>
        private void InternalStart()
        {
            _thread = new Thread(new ThreadStart(ThreadFunction));
            _thread.IsBackground = true;
            _thread.Start();
            State = ServerDiscoveryState.Running;
        }

        /// <summary>
        /// Perform internal household chores part of shutting down the client.
        /// </summary>
        private void InternalStop()
        {
            _log.Debug("ServerDiscovery.InternalStop()");

            // Stop the worker thread.
            if (_thread != null)
            {
                _thread.Join();

                _thread = null;
            }

            State = ServerDiscoveryState.Idle;
        }

        /// <summary>
        /// Thread function executed by the server discovery's worker thread.
        /// </summary>
        private void ThreadFunction()
        {
            var callLogTag = "ServerDiscovery:ThreadFunction()";

            Debug.Assert(State == ServerDiscoveryState.Running);

            var discoveryUdpClient = new UdpClient(Server.UdpBroadcastRemotePort);

            // The address from which to receive data.
            // In this case we are interested in data from any IP and any port.
            var discoveryUdpRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            while (State != ServerDiscoveryState.Stopping)
            {
                lock (stateLock)
                {
                    for (int i = 0; i < 10; ++i)
                    {
                        try
                        {
                            if (discoveryUdpClient.Available == 0)
                            {
                                break;
                            }

                            // Receive bytes from anyone on local port 'Server.UdpBroadcastRemotePort'.
                            byte[] data = discoveryUdpClient.Receive(ref discoveryUdpRemoteEndPoint);

                            // Encode received bytes to UTF8- encoding.
                            string receivedText = Encoding.UTF8.GetString(data);

                            var obj = Message.GetObjectFromMessageXML(receivedText);

                            var serverInfo = obj as ServerInfo;

                            if (serverInfo != null)
                            {
                                ProcessServerInfo(serverInfo);
                            }
                            else
                            {
                                _log.Warning(string.Format(callLogTag + ": Received unexpected message '{0}'!", receivedText));
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Warning(callLogTag + ": Exception: " + e.ToString());
                        }
                    }

                    RemoveStaleServerInfos();
                }
            }

            discoveryUdpClient.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverInfo"></param>
        private void ProcessServerInfo(ServerInfo serverInfo)
        {
            lock (_serverInfos)
            {
                _serverInfos[serverInfo] = DateTime.Now;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RemoveStaleServerInfos()
        {
            lock (_serverInfos)
            {
                var keys = new List<ServerInfo>(_serverInfos.Keys);
                var timeNow = DateTime.Now;

                foreach (var serverInfo in keys)
                {
                    var timeLastMessage = _serverInfos[serverInfo];
                    var age = timeNow - timeLastMessage;

                    if (age > TimeSpan.FromSeconds(1))
                    {
                        _serverInfos.Remove(serverInfo);
                    }
                }
            }
        }

        #endregion Non-public API
    }
}
