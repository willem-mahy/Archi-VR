using UnityEngine;

namespace Assets.ArchiVR.Application.Editable
{
    /// <summary>
    /// Represents a layer (eg. "Basement", "Ground floor", "Floor 1", "Attic")
    /// </summary>
    public class ArchiVRLayer
    {
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
