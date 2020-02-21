using UnityEngine;

namespace WM.Unity
{
    /// <summary>
    /// Simulates environmental lighting.  Attach to a directional light.
    /// </summary>
    public class EnvironmentalLighting : MonoBehaviour
    {
        #region Variables

        public float AnimationSpeed = 0.1f;

        #endregion

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            #region Animate sun

            var sunSpeed = AnimationSpeed;

            if (sunSpeed != 0.0f)
            {
                gameObject.transform.Rotate(Vector3.up, Time.deltaTime * sunSpeed);
            }

            #endregion
        }
    }
}
