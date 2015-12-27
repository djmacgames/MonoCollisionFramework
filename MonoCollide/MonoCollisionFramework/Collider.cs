using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public class Collider
    {
        private List<Triangle> triangles;
        private List<ITriangleSelector> selectors = new List<ITriangleSelector>();
        private int trianglesTested = 0;
        private int loopCount = 0;

        public Collider(int triangleCapacity = 128000)
        {
            triangles = new List<Triangle>(triangleCapacity);
        }

        public List<ITriangleSelector> Selectors
        {
            get { return selectors; }
        }

        public int TrianglesTested
        {
            get { return trianglesTested; }
        }

        public int LoopCount
        {
            get { return loopCount; }
        }

        public void Collide(ref Vector3 position, float radius, IContact contact, int loopCount = 5)
        {
            trianglesTested = 0;
            this.loopCount = 0;

            foreach (ITriangleSelector triangleSelector in selectors)
            {
                BoundingBox bounds;

                bounds.Min.X = position.X - radius * 1.25f;
                bounds.Min.Y = position.Y - radius * 1.25f;
                bounds.Min.Z = position.Z - radius * 1.25f;
                bounds.Max.X = position.X + radius * 1.25f;
                bounds.Max.Y = position.Y + radius * 1.25f;
                bounds.Max.Z = position.Z + radius * 1.25f;
                triangleSelector.Select(bounds, triangles);

                for (int i = 0; i != loopCount; i++, this.loopCount++)
                {
                    Vector3 resolvedPosition = Vector3.Zero;
                    Vector3 iPoint = Vector3.Zero;
                    Triangle iTriangle;
                    int iEdge = -1;
                    float iMin = float.MaxValue;
                    bool collided = false;

                    iTriangle.A = Vector3.Zero;
                    iTriangle.B = Vector3.Zero;
                    iTriangle.C = Vector3.Zero;
                    iTriangle.Normal = Vector3.Zero;
                    iTriangle.Dist = 0;

                    for (int j = 0; j != triangles.Count; j++)
                    {
                        Triangle triangle = triangles[j];
                        Vector3 point;
                        Nullable<float> t = triangle.Intersect(position, -triangle.Normal, out point);
                        bool hit = false;

                        trianglesTested++;

                        if (t.HasValue)
                        {
                            if (t.Value > 0 && t.Value < iMin && t.Value < radius)
                            {
                                resolvedPosition = point + triangle.Normal * radius;
                                iTriangle = triangle;
                                iPoint = point;
                                iEdge = -1;
                                iMin = t.Value;
                                collided = true;
                                hit = true;
                            }
                        }
                        if (!hit)
                        {
                            for (int k = 0; k != 3; k++)
                            {
                                Vector3 a = triangle[k];
                                Vector3 b = triangle[(k + 1) % 3];
                                Vector3 a2b = b - a;
                                Vector3 a2p = position - a;
                                Vector3 c = a;
                                float s = Vector3.Dot(a2b, a2p);

                                if (s > 0)
                                {
                                    s /= a2b.LengthSquared();
                                    if (s < 1)
                                    {
                                        c = a + s * a2b;
                                    }
                                    else
                                    {
                                        c = b;
                                    }
                                }

                                Vector3 d = position - c;
                                float len = d.Length();

                                if (len < radius && len < iMin)
                                {
                                    resolvedPosition = c + Vector3.Normalize(d) * radius;
                                    iTriangle = triangle;
                                    iPoint = c;
                                    iEdge = k;
                                    iMin = len;
                                    collided = true;
                                }
                            }
                        }
                    }
                    if (collided)
                    {
                        if (contact.ContactMade(triangleSelector, iTriangle, iPoint, iEdge))
                        {
                            position = resolvedPosition;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                triangles.Clear();
            }
        }
    }
}
