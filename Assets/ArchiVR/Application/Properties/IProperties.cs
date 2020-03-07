using ArchiVR.Application.Editable;
using UnityEngine;

namespace ArchiVR.Application.Properties
{
    public interface IProperties
    {
        IProperty[] Properties { get;  }
    }

    public interface IProperty
    {
        string Name { get; }
    }

    abstract public class Property<T>
        : IProperty
    {
        string Name => throw new System.NotImplementedException();

        string IProperty.Name => throw new System.NotImplementedException();

        abstract public T Value { get; set; }

        public T[] DefaultValues { get; set; }
    }

    /// <summary>
    /// 'GameObject.name' property.
    /// </summary>
    public class ObjectNameProperty
        : Property<string>
    {
        public ObjectNameProperty(
            GameObject gameObject,
            string[] defaultValues)
        {
            _gameObject = gameObject;
            DefaultValues = defaultValues;
        }

        /// <summary>
        /// <see cref="IProperty.Name"/> implementation.
        /// </summary>
        public string Name => "Name";

        /// <summary>
        /// <see cref="Property{String}"/> implementation.
        /// </summary>
        override public string Value
        {
            get { return _gameObject.name; }
            set { _gameObject.name = value; }
        }

        private GameObject _gameObject;
    }

    /// <summary>
    /// 'LayerContent.LayerName' property.
    /// </summary>
    public class LayerProperty
        : Property<string>
    {
        public LayerProperty(
            ILayerContent obj,
            string[] defaultValues)
        {
            _obj = obj;
            DefaultValues = defaultValues;
        }

        /// <summary>
        /// <see cref="IProperty.Name"/> implementation.
        /// </summary>
        public string Name => "Name";

        /// <summary>
        /// <see cref="Property{String}"/> implementation.
        /// </summary>
        override public string Value
        {
            get { return _obj.LayerName; }
            set { _obj.LayerName = value; }
        }

        private ILayerContent _obj;
    }

    /// <summary>
    /// 'Light.Color' property.
    /// </summary>
    public class LightColorProperty
        : Property<Color>
    {
        public LightColorProperty(ArchiVRLight light)
        {
            _light = light;
        }

        /// <summary>
        /// <see cref="IProperty.Name"/> implementation.
        /// </summary>
        public string Name => "Color";

        /// <summary>
        /// <see cref="Property{Color}"/> implementation.
        /// </summary>
        override public Color Value
        {
            get { return _light.Color; }
            set { _light.Color = value; }
        }

        private ArchiVRLight _light;
    }
}

