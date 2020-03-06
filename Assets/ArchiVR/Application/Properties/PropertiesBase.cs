using UnityEngine;

namespace ArchiVR.Application.Properties
{
    public class PropertiesBase
        : MonoBehaviour
        , IProperties
    {
        protected IProperty[] _properties;

        public IProperty[] Properties => _properties;
    }
}