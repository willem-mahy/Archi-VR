using ArchiVR.Application.Properties;

namespace ArchiVR.Application.Editable
{
    public class ArchiVRProp
        : PropertiesBase
        , ILayerContent
        , IObjectDefinitionSupplier<PropDefinition>
        , IPrefabInstantiation
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PropDefinition GetObjectDefinition()
        {
            var definition = new PropDefinition();

            definition.Name = gameObject.name;
            definition.PrefabPath = PrefabPath;
            definition.Position = gameObject.transform.position;
            definition.Rotation = gameObject.transform.rotation;

            definition.LayerName = LayerName;

            return definition;
        }

        /// <summary>
        /// 
        /// </summary>
        public string PrefabPath { get; set; }
    }
}


