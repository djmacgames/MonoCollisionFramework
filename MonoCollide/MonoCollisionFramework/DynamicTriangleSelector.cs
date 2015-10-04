using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public class DynamicTriangleSelector : ITriangleSelector
    {
        private List<Triangle> triangles;
        private IWorld world;
        private BoundingBox bounds;
        private int count;

        public DynamicTriangleSelector(IWorld world, List<Vector3> positions)
        {
            triangles = new List<Triangle>(positions.Count / 3);
            for (int i = 0; i != positions.Count; )
            {
                Vector3 a = positions[i++];
                Vector3 b = positions[i++];
                Vector3 c = positions[i++];

                triangles.Add(new Triangle(a, b, c));
            }
            bounds = BoundingBox.CreateFromPoints(positions);

            this.world = world;

            count = triangles.Count;
            Tag = null;
        }

        public int Count
        {
            get { return count; }
        }

        public object Tag
        {
            get;
            set;
        }

        public void Select(BoundingBox bounds, List<Triangle> triangles)
        {
            BoundingBox tbounds = BoundingBoxHelper.Transform(this.bounds, world.World);

            if (tbounds.Contains(bounds) != ContainmentType.Disjoint)
            {
                foreach (Triangle triangle in this.triangles)
                {
                    Triangle t = triangle;

                    t.A = Vector3.Transform(t.A, world.World);
                    t.B = Vector3.Transform(t.B, world.World);
                    t.C = Vector3.Transform(t.C, world.World);
                    t.CalcPlane();

                    triangles.Add(t);
                }
            }
        }
    }
}
