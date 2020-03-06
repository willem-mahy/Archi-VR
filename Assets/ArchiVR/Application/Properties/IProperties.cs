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

    public class ObjectNameProperty
        : Property<string>
    {
        public ObjectNameProperty(
            GameObject gameObject,
            string[] defaultValues)
        {
            GameObject = gameObject;
            DefaultValues = defaultValues;
        }

        public GameObject GameObject;

        public string Name => "Name";

        override public string Value
        {
            get { return GameObject.name; }
            set { GameObject.name = value; }
        }
    }

    public class LightColorProperty
        : Property<Color>
    {
        public LightColorProperty(ArchiVRLight light)
        {
            Light = light;
        }

        private ArchiVRLight Light;

        public string Name => "Color";

        override public Color Value
        {
            get { return Light.Color; }
            set { Light.Color = value; }
        }
    }
}

