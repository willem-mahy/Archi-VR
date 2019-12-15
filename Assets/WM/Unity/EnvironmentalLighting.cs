using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WM.Unity
{
    /// <summary>
    /// Animates sun.
    /// </summary>
    public class EnvironmentalLighting : MonoBehaviour
    {
        #region Variables

        public float AnimationSpeed = 0.1f;

        public GameObject Sun { get; set; }

        #endregion

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            if (Sun == null)
            {
                Sun = UtilUnity.TryFindGameObject("Sun");
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            #region Animate sun

            var sunSpeed = AnimationSpeed;

            if (sunSpeed != 0.0f)
            {
                Sun.transform.Rotate(Vector3.up, Time.deltaTime * sunSpeed);
            }

            #endregion
        }
    }
}
