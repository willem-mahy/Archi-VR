using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializationType_Plane
        : IPickInitializationType
    {
        private PickClassifier.PickClassification _planeType;

        public PickInitializationType_Plane(PickClassifier.PickClassification planeType)
        {
            _planeType = planeType;
        }

        public float GetQuality(List<RaycastHit> picks)
        {
            var pickClassifications = PickClassifier.Classify(picks);
            
            switch (picks.Count)
            {
                case 0:
                    {
                        return 0;
                    }
                default:
                    {
                        return pickClassifications[0] == _planeType ? 1 : 0;
                    }
            }
        }

        public Vector3 AnchoringAxis_Local => Vector3.forward;

        public Vector3 UpAxis_Local => Vector3.up;

        public void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks,
            float rotation)
        {
            var transform = gameObject.transform;

            switch (picks.Count)
            {
                case 0:
                    {
                    }
                    break;
                default: 
                    {
                        // Use first pick position as anchoring position.
                        // Use first pick surface normal as anchoring axis.
                        // Use rotation as the rotation around anchoring axis.
                        var position = picks[0].point;

                        transform.position = position;

                        var anchorDirection = picks[0].normal;

                        var lookat = position + anchorDirection;

                        transform.rotation = Quaternion.identity;
                        transform.LookAt(lookat);

                        transform.Rotate(Vector3.forward, rotation, Space.Self);
                    }
                    break;
            }
        }
    }
}
