using UnityEngine;

namespace ArchiVR.Application.Editable
{
    /// <summary>
    /// Objects that are associated wit a specific layer (eg. lights and props) must implement this interface.
    /// </summary>
    public interface ILayerContent
    {
        /// <summary>
        /// Gets/Sets the layer name.
        /// The layer name defines the layer where the object is part of.
        /// </summary>
        string LayerName { get; set; }
    }

    /// <summary>
    /// Represents a layer (eg. "Basement", "Ground floor", "Floor 1", "Attic")
    /// </summary>
    public class ArchiVRLayer
    {
        static public readonly string[] DefaultNames =
        {
            "Omgeving",
            "Kelder",
            "Gelijkvloers",
            "Verdieping",
            "Zolder"
        };

        /// <summary>
        /// The gameobject containing the basic model geometry (as imported from sketchup).
        /// </summary>
        public GameObject Model { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelGO"><see cref="ArchiVRModelLayer.Model"/></param>
        public ArchiVRLayer(GameObject modelGO)
        {
            Model = modelGO;
        }

        /// <summary>
        /// Sets the layer active/inactive.
        /// </summary>
        public void SetActive(bool active)
        {
            Model.SetActive(active);
        }

        /// <summary>
        /// Gets the layer name.
        /// </summary>
        public string Name => Model.name;
    };
}
