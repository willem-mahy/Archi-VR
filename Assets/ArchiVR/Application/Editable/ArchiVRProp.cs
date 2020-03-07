using ArchiVR.Application.Properties;

namespace ArchiVR.Application.Editable
{
    public class ArchiVRProp
        : PropertiesBase
        , ILayerContent
    {
        // Start is called before the first frame update
        void Start()
        {
            _properties = new IProperty[]
            {
                //new ObjectNameProperty(gameObject, null),
                new LayerProperty(this, ArchiVRLayer.DefaultNames),
            };
        }

        /// <summary>
        /// <see cref="ILayerContent.LayerName"/> implementation.
        /// </summary>
        public string LayerName { get; set; }
    }
}


