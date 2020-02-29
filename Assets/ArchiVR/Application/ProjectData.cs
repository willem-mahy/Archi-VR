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
        public POIData POIData = new POIData(); 
        
        public LightingData LightingData = new LightingData();

        public PropData PropData = new PropData();
    }

    [Serializable]
    [XmlRoot("POIData")]
    public class POIData
    {
        public List<POIDefinition> poiDefinitions = new List<POIDefinition>();
    }

    [Serializable]
    [XmlRoot("LightingData")]
    public class LightingData
    {
        public List<LightDefinition> lightDefinitions = new List<LightDefinition>();
    }

    [Serializable]
    [XmlRoot("PropData")]
    public class PropData
    {
        public List<PropDefinition> propDefinitions = new List<PropDefinition>();
    }

    [Serializable]
    [XmlRoot("ObjectDefinition")]
    public class ObjectDefinition
    {
        public string PrefabPath;

        public SerializableVector3 Position;

        public SerializableQuaternion Rotation;

        public string Name;

        [XmlIgnoreAttribute]
        public GameObject GameObject;
    }

    [Serializable]
    [XmlRoot("POIDefinition")]
    public class POIDefinition : ObjectDefinition
    {   
    }

    [Serializable]
    [XmlRoot("LightDefinition")]
    public class LightDefinition : ObjectDefinition
    {
        public string LayerName;

        public Color LightColor;

        public Color BodyColor1;

        public Color BodyColor2;
    }

    [Serializable]
    [XmlRoot("PropDefinition")]
    public class PropDefinition : ObjectDefinition
    {
        public string LayerName;
    }
}
