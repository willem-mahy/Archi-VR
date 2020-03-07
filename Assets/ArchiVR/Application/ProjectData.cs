using Assets.WM.Unity;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace ArchiVR.Application
{
    /// <summary>
    /// Data structure used to serialize project content to XML.
    /// </summary>
    [Serializable]
    [XmlRoot("ArchiVRProjectData")]
    public class ProjectData
    {
        /// <summary>
        /// POI data.
        /// </summary>
        public /*readonly*/ POIData POIData = new POIData();

        /// <summary>
        /// Lighting data.
        /// </summary>
        public /*readonly*/ LightingData LightingData = new LightingData();

        /// <summary>
        /// Prop data.
        /// </summary>
        public /*readonly*/ PropData PropData = new PropData();

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProjectData() { }

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        public ProjectData(
            List<LightDefinition> lightDefinitions,
            List<PropDefinition> propDefinitions,
            List<POIDefinition> poiDefinitions)
        {
            LightingData.lightDefinitions = lightDefinitions;
            PropData.propDefinitions = propDefinitions;
            POIData.poiDefinitions = poiDefinitions;
        }

        #endregion Constructors
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
