using Assets.WM.Unity;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace ArchiVR.Application
{
    [Serializable]
    [XmlRoot("ArchiVRProjectData")]
    public class ProjectData
    {
        public LightingData LightingData = new LightingData();

        //public FurnitureData FurnitureData = new FurnitureData();

        //public POIData POIData = new POIData();
    }

    [Serializable]
    [XmlRoot("LightingData")]
    public class LightingData
    {
        public List<LightDefinition> lightDefinitions = new List<LightDefinition>();
    }

    [Serializable]
    [XmlRoot("LightingData")]
    public class LightDefinition
    {
        public string PrefabPath;
        
        public SerializableVector3 Position;

        public SerializableQuaternion Rotation;

        [XmlIgnoreAttribute]
        public GameObject GameObject;
    }
}
