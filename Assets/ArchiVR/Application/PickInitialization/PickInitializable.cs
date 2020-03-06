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
            Wall,
            POI,
            Default
        };

        private static IPickInitializationType GetPickInitializationTypeInstance(PickInitializationType t)
        {
            switch (t)
            {
                case PickInitializationType.Floor:
                    return new PickInitializationType_Plane(PickClassifier.PickClassification.Floor);
                case PickInitializationType.Ceiling:
                    return new PickInitializationType_Plane(PickClassifier.PickClassification.Ceiling);
                case PickInitializationType.Wall:
                    return new PickInitializationType_Plane(PickClassifier.PickClassification.Wall);
                case PickInitializationType.POI:
                    return new PickInitializationType_POI();
                case PickInitializationType.Default:
                    return new PickInitializationType_Default();
                default:
                    return null;
            }

        }

        public void Awake()
        {
            InitPickInitializationTypes();
        }
        
        private void InitPickInitializationTypes()
        {
            _pickInitializationTypes.Clear();

            foreach (var t in PickInitializationTypes)
            {
                _pickInitializationTypes.Add(GetPickInitializationTypeInstance(t));
            }
        }

        public void AddPickInitializationType(PickInitializationType type)
        {
            if (PickInitializationTypes.Contains(type))
            {
                return;
            }
            
            PickInitializationTypes.Add(type);
            _pickInitializationTypes.Add(GetPickInitializationTypeInstance(type));
        }

        public Vector3 GetAnchoringAxis_Local(List<RaycastHit> picks)
        {
            return GetBestPickInitializationType(picks)?.AnchoringAxis_Local ?? Vector3.forward;
        }

        public Vector3 GetUpAxis_Local(List<RaycastHit> picks)
        {
            return GetBestPickInitializationType(picks)?.UpAxis_Local ?? Vector3.up;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picks"></param>
        /// <returns></returns>
        private IPickInitializationType GetBestPickInitializationType(
            List<RaycastHit> picks)
        {
            float maxQuality = 0;

            IPickInitializationType best = null;

            for (int i = 0; i < _pickInitializationTypes.Count; ++i)
            {
                var pit = _pickInitializationTypes[i];
                var quality = pit.GetQuality(picks);
                if (quality > maxQuality)
                {
                    maxQuality = quality;
                    best = pit;
                }
            }

            return best;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picks"></param>
        /// <returns></returns>
        public bool Initialize(
            List<RaycastHit> picks,
            float rotation)
        {
            var bestPit = GetBestPickInitializationType(picks);

            if (null == bestPit)
            {
                return false;
            }

            bestPit.Initialize(gameObject, picks, rotation);
            return true;
        }

        public List<PickInitializationType> PickInitializationTypes = new List<PickInitializationType>();

        private List<IPickInitializationType> _pickInitializationTypes = new List<IPickInitializationType>();
    }
}
