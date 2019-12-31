namespace WM.Net
{
    /// <summary>
    /// The possible network modes for an application.
    /// - Standalone:   No networking.
    /// - Server:       Running a Server and a Client. Client is connected to own local Server.
    /// - Client:       Running a Client only, connected to remote Server.
    /// 
    /// FIXME: TODO? Split up 'Server into the following 2 states?
    /// - Server:       Running a Server only.
    /// - Host:         Running a Server and a Client. Client is connected to own local Server.
    /// </summary>
    public enum NetworkMode
    {
        Standalone = 0,
        Server,
        Client
    };
}
