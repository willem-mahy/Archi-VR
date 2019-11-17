using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.WM.Unity
{
    [System.Serializable]
    public class SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static implicit operator Quaternion(SerializableQuaternion sq)
        {
            return new Quaternion(sq.x, sq.y, sq.z, sq.w);
        }

        public static implicit operator SerializableQuaternion(Quaternion q)
        {
            return new SerializableQuaternion()
            {
                x = q.x,
                y = q.y,
                z = q.z,
                w = q.w
            };
        }
    }

    [System.Serializable]
    public class SerializableVector3
    {   
        public float x;
        public float y;
        public float z;

        // Vector3
        public static implicit operator Vector3(SerializableVector3 sv)
        {
            return new Vector3(sv.x, sv.y, sv.z);
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3()
            {
                x = v.x,
                y = v.y,
                z = v.z
            };
        }
    }
}
