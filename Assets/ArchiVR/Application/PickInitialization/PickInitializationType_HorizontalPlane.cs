using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializationType_HorizontalPlane
        : IPickInitializationType
    {
        PickClassifier.PickClassification _plane;

        public PickInitializationType_HorizontalPlane(PickClassifier.PickClassification plane)
        {
            _plane = plane;
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
                case 1:
                    {
                        return pickClassifications[0] == _plane ? 0.5f : 0;
                    }
                default: // 2 or more picks: just use the first 2
                    {
                        var firstPickIsFloor = pickClassifications[0] == _plane;
                        var secondPickIsFloor = pickClassifications[1] == _plane;
                        return (firstPickIsFloor && secondPickIsFloor) ? 1 : 0;
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
                    break;
                case 1:
                    {
                        var position = picks[0].point;
                        var lookat = position + picks[0].normal;
                        var up = Vector3.right;

                        transform.position = position;
                        transform.LookAt(lookat, up);
                    }
                    break;
                default: // 2 or more picks: just use the first 2
                    {
                        var offsetPoint0Point1 = (picks[1].point - picks[0].point);
                        var offsetPoint0Point1Horizontal = new Vector3(offsetPoint0Point1.x, 0, offsetPoint0Point1.z);
                        var forwardDirection = offsetPoint0Point1Horizontal.normalized;

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
