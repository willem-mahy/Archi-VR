﻿using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializationType_POI
        : IPickInitializationType
    {
        public PickInitializationType_POI()
        {
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
                        return pickClassifications[0] == PickClassifier.PickClassification.Floor ? 1.0f : 0;
                    }
            }
        }

        public void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks,
            float rotation)
        {
            var transform = gameObject.transform;

            switch (picks.Count)
            {
                case 0:
                    break;
                default:
                    {
                        transform.position = picks[0].point;
                        transform.rotation = Quaternion.identity;
                        transform.Rotate(Vector3.up, rotation, Space.Self);
                    }
                    break;
            }
        }

        public Vector3 AnchoringAxis_Local => Vector3.up;

        public Vector3 UpAxis_Local => Vector3.forward;
    }
}