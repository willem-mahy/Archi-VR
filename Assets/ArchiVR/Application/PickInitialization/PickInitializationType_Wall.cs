using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializationType_Wall
        : IPickInitializationType
    {
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
                        return pickClassifications[0] == PickClassifier.PickClassification.Wall ? 1 : 0;
                    }
            }
        }

        public void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks)
        {
            switch (picks.Count)
            {
                case 0:
                    {
                    }
                    break;
                default:
                    {
                        var transform = gameObject.transform;

                        var position = picks[0].point;
                        var lookat = position + picks[0].normal;
                        var up = Vector3.up;

                        transform.position = position;
                        transform.LookAt(lookat, up);
                    }
                    break;
            }
        }
    }
}
