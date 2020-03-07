using ArchiVR.Application.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.Editable
{
    public class ArchiVRLight
        : PropertiesBase
        , ILayerContent
        , IObjectDefinitionSupplier<LightDefinition>
        , IPrefabInstantiation
    {
        public List<Light> LightSources = new List<Light>();

        // Start is called before the first frame update
        void Start()
        {
            _properties = new IProperty[]
            {
                new ObjectNameProperty(gameObject, new string [] { "Light1", "Light2" }),
                new LayerProperty(this, new string [] { "Omgeving", "Kelder", "Gelijkvloers", "Verdieping", "Zolder" }),
                new LightColorProperty(this)
            };
        }

        /// <summary>
        /// <see cref="ILayerContent.LayerName"/> implementation.
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Color Color
        {
            get
            {
                return LightSources[0].color;
            }
            set
            {
                foreach (var light in LightSources)
                {
                    light.color = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public LightDefinition GetObjectDefinition()
        {
            var definition = new LightDefinition();
            
            definition.Name = gameObject.name;
            definition.PrefabPath = PrefabPath;
            definition.Position = gameObject.transform.position;
            definition.Rotation = gameObject.transform.rotation;

            definition.LayerName = LayerName;
            definition.LightColor = Color;

            return definition;
        }

        /// <summary>
        /// 
        /// </summary>
        public string PrefabPath { get; set; }
    }
}


