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
        public GraphicsSettings GraphicsSettings = new GraphicsSettings();

        public bool LoggingEnabled = false;

        public List<string> PlayerNames = new List<string>();

        public PlayerSettings PlayerSettings = new PlayerSettings();

        public SerializableVector3 SharedReferenceFramePosition = Vector3.zero;

        public SerializableQuaternion SharedReferenceFrameRotation = Quaternion.identity;
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
    }
}
