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

        public SerializableQuaternion()
        {
            x = y = z = 0;
            w = 1;
        }

        public bool Equals(SerializableQuaternion s)
        {
            return
                x == s.x &&
                y == s.y &&
                z == s.z &&
                w == s.w;
        }
    }

    [System.Serializable]
    public class SerializableVector3
    {   
        public float x = 0;
        public float y = 0;
        public float z = 0;

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

        public bool Equals(SerializableVector3 s)
        {
            return
                x == s.x &&
                y == s.y &&
                z == s.z;
        }
    }
}
