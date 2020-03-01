using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializable
        : MonoBehaviour
    {
        public enum PickInitializationType
        {
            Ceiling,
            Floor,
            Wall
        };

        private static IPickInitializationType GetPickInitializationTypeInstance(PickInitializationType t)
        {
            switch (t)
            {
                case PickInitializationType.Floor:
                    return new PickInitializationType_HorizontalPlane(PickClassifier.PickClassification.Floor);
                case PickInitializationType.Ceiling:
                    return new PickInitializationType_HorizontalPlane(PickClassifier.PickClassification.Ceiling);
                case PickInitializationType.Wall:
                    return new PickInitializationType_Wall();
                default:
                    return null;
            }

        }

        public void Awake()
        {
            foreach (var t in PickInitializationTypes)
            {
                _pickInitializationTypes.Add(GetPickInitializationTypeInstance(t));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picks"></param>
        /// <returns></returns>
        public bool Initialize(List<RaycastHit> picks)
        {
            float maxQuality = 0;
            int bestPitIndex = -1;

            for (int i = 0; i < this._pickInitializationTypes.Count; ++i)
            {
                var pit = _pickInitializationTypes[i];
                var quality = pit.GetQuality(picks);
                if (quality > maxQuality)
                {
                    maxQuality = quality;
                    bestPitIndex = i;
                }
            }

            if (bestPitIndex == -1)
            {
                return false;
            }
            
            var bestPit = _pickInitializationTypes[bestPitIndex];
            bestPit.Initialize(gameObject, picks);
            return true;
        }

        public List<PickInitializationType> PickInitializationTypes = new List<PickInitializationType>();

        private List<IPickInitializationType> _pickInitializationTypes = new List<IPickInitializationType>();
    }
}
