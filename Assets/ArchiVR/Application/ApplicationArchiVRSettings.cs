using Assets.WM.Unity;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace ArchiVR.Application
{
    /*
     * TODO: Add:
     * - GFX quality
     * - whether or not to show player names above avatars
     * - ActiveProject
     * - ActivePOI
     * - SharedReferenceFrame
     * - ...
     */
    [Serializable]
    [XmlRoot("ApplicationArchiVRSettings")]
    public class ApplicationArchiVRSettings
    {
        public NetworkSettings NetworkSettings = new NetworkSettings();

        public PlayerSettings PlayerSettings = new PlayerSettings();

        public GraphicsSettings GraphicsSettings = new GraphicsSettings();

        public DebugLogSettings DebugLogSettings = new DebugLogSettings();

        public List<string> PlayerNames = new List<string>();

        
    }

    [Serializable]
    [XmlRoot("PlayerSettings")]
    public class PlayerSettings
    {
        public string name = "Unnamed";
        public Guid avatarID = Guid.Empty;
    }

    [Serializable]
    [XmlRoot("GraphicsSettings")]
    public class GraphicsSettings
    {
        public int QualityLevel = 0;
        public bool ShowFPS = false;
        public bool ShowReferenceFrames = false;
    }

    [Serializable]
    [XmlRoot("NetworkSettings")]
    public class NetworkSettings
    {
        public bool ColocationEnabled = false;
        public Vector3 SharedReferencePosition = Vector3.zero;
        public Quaternion SharedReferenceRotation = Quaternion.identity;
    }

    [Serializable]
    [XmlRoot("DebugLogSettings")]
    public class DebugLogSettings
    {
        public bool LoggingEnabled = false;
        public bool FilterWarnings = false;
        public bool FilterErrors = false;
        public bool FilterDebug = false;
    }
}
