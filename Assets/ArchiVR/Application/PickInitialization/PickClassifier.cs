using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickClassifier
    {
        public enum PickClassification
        {
            Floor,
            Wall,
            Ceiling,
            Other
        }

        /// <summary>
        /// Classifies the given picks as either Floor, Ceiling or Wall pick.
        /// </summary>
        /// <param name="picks"></param>
        /// <returns></returns>
        static public List<PickClassification> Classify(List<RaycastHit> picks)
        {
            List<PickClassification> c = new List<PickClassification>();

            foreach (var pick in picks)
            {
                var dot = Vector3.Dot(pick.normal, Vector3.up);

                if (dot > 0.9)
                {
                    c.Add(PickClassification.Floor);
                }
                else if (dot < -0.9)
                {
                    c.Add(PickClassification.Ceiling);
                }
                else
                {
                    c.Add(PickClassification.Wall);
                }
            }

            return c;
        }
    }
}
