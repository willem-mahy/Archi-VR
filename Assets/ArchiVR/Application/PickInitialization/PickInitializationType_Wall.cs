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
            var transform = gameObject.transform;

            switch (picks.Count)
            {
                case 0:
                    {
                    }
                    break;
                case 1:
                    {
                        var position = picks[0].point;
                        var lookat = position + picks[0].normal;
                        var up = Vector3.up;

                        transform.position = position;
                        transform.LookAt(lookat, up);
                    }
                    break;
                default: // 2 or more picks: just use the first 2
                    {
                        var offsetPoint0Point1 = (picks[1].point - picks[0].point);
                        var forwardDirection = offsetPoint0Point1.normalized;

                        var position = picks[0].point;
                        var lookat = position + picks[0].normal;
                        var up = forwardDirection;

                        transform.position = position;
                        transform.LookAt(lookat, up);
                    }
                    break;
            }
        }
    }
}
