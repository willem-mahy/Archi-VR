using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.Properties
{
    public class ArchiVRLight
        : PropertiesBase
    {
        public List<Light> LightSources = new List<Light>();

        // Start is called before the first frame update
        void Start()
        {
            _properties = new IProperty[]
            {
                new ObjectNameProperty(gameObject, new string [] { "Light1", "Light2" }),
                new LightColorProperty(this)
            };
        }

        // Update is called once per frame
        void Update()
        {

        }

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
    }
}


