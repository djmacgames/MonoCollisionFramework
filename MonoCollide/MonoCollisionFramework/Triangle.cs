using System;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public struct Triangle
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 Normal;
        public float Dist;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
            Normal = Vector3.Normalize(Vector3.Cross(c - b, b - a));
            Dist = -Vector3.Dot(A, Normal);
        }

        public Vector3 this[int i]
        {
            get
            {
                if (i == 0)
                {
                    return A;
                }
                else if (i == 1)
                {
                    return B;
                }
                else
                {
                    return C;
                }
            }
        }

        public void CalcPlane()
        {
            Normal = Vector3.Normalize(Vector3.Cross(C - B, B - A));
            Dist = -Vector3.Dot(A, Normal);
        }

        public Nullable<float> Intersect(Vector3 origin, Vector3 direction, out Vector3 iPoint)
        {
            float t = Vector3.Dot(Normal, direction);

            iPoint = Vector3.Zero;
            if (Math.Abs(t) > float.Epsilon)
            {
                t = (-Dist - Vector3.Dot(Normal, origin)) / t;
                iPoint = origin + t * direction;
                for (int i = 0; i != 3; i++)
                {
                    Vector3 a = this[i];
                    Vector3 b = this[(i + 1) % 3];
                    Vector3 c = a + Normal;
                    Vector3 n = Vector3.Normalize(Vector3.Cross(c - b, b - a));
                    float d = -Vector3.Dot(a, n);
                    float side = Vector3.Dot(iPoint, n) + d;

                    if (side > 0)
                    {
                        return null;
                    }
                }
                return t;
            }
            return null;
        }
    }
}
