using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public class StaticTriangleSelector : ITriangleSelector
    {
        private class TreeNode
        {
            private BoundingBox bounds;
            private List<Triangle> triangles = new List<Triangle>();
            private List<TreeNode> children = new List<StaticTriangleSelector.TreeNode>();

            public TreeNode(BoundingBox bounds, List<Triangle> triangles, int minimumTrianglesPerNode)
            {
                this.bounds = bounds;

                if (triangles.Count > minimumTrianglesPerNode)
                {
                    Vector3 l = bounds.Min;
                    Vector3 h = bounds.Max;
                    Vector3 c = (bounds.Max + bounds.Min) / 2;
                    BoundingBox[] blist = new BoundingBox[] {
						new BoundingBox(new Vector3(l.X, l.Y, l.Z), new Vector3(c.X, c.Y, c.Z)),
						new BoundingBox(new Vector3(c.X, l.Y, l.Z), new Vector3(h.X, c.Y, c.Z)),
						new BoundingBox(new Vector3(l.X, l.Y, c.Z), new Vector3(c.X, c.Y, h.Z)),
						new BoundingBox(new Vector3(c.X, l.Y, c.Z), new Vector3(h.X, c.Y, h.Z)),
						new BoundingBox(new Vector3(l.X, c.Y, l.Z), new Vector3(c.X, h.Y, c.Z)),
						new BoundingBox(new Vector3(c.X, c.Y, l.Z), new Vector3(h.X, h.Y, c.Z)),
						new BoundingBox(new Vector3(l.X, c.Y, c.Z), new Vector3(c.X, h.Y, h.Z)),
						new BoundingBox(new Vector3(c.X, c.Y, c.Z), new Vector3(h.X, h.Y, h.Z))
					};

                    for (int i = 0; i != 8; i++)
                    {
                        BoundingBox b = blist[i];
                        List<Triangle> keep = new List<Triangle>();
                        List<Triangle> add = new List<Triangle>();

                        for (int j = 0; j != triangles.Count; j++)
                        {
                            Triangle t = triangles[j];

                            if (b.Contains(t.A) == ContainmentType.Contains && 
                                b.Contains(t.B) == ContainmentType.Contains && b.Contains(t.C) == ContainmentType.Contains)
                            {
                                add.Add(t);
                            }
                            else
                            {
                                keep.Add(t);
                            }
                        }
                        if (add.Count != 0)
                        {
                            children.Add(new TreeNode(b, add, minimumTrianglesPerNode));
                        }
                        triangles = keep;
                    }
                    this.triangles.AddRange(triangles);
                }
                else
                {
                    this.triangles.AddRange(triangles);
                }
            }

            public BoundingBox Bounds
            {
                get { return bounds; }
            }

            public void Select(BoundingBox bounds, List<Triangle> triangles)
            {
                if (this.bounds.Contains(bounds) != ContainmentType.Disjoint)
                {
                    triangles.AddRange(this.triangles);
                    foreach (TreeNode child in children)
                    {
                        child.Select(bounds, triangles);
                    }
                }
            }
        }

        private TreeNode root;
        private int count;

        public StaticTriangleSelector(Matrix world, List<Vector3> positions, int minimumTrianglesPerNode)
        {
            List<Triangle> triangles = new List<Triangle>(positions.Count / 3);
            BoundingBox bounds;

            for (int i = 0; i != positions.Count; i++)
            {
                positions[i] = Vector3.Transform(positions[i], world);
            }
            bounds = BoundingBox.CreateFromPoints(positions);

            bounds.Min -= new Vector3(1, 1, 1);
            bounds.Max += new Vector3(1, 1, 1);

            for (int i = 0; i != positions.Count; )
            {
                Vector3 a = positions[i++];
                Vector3 b = positions[i++];
                Vector3 c = positions[i++];

                triangles.Add(new Triangle(a, b, c));
            }
            root = new TreeNode(bounds, triangles, minimumTrianglesPerNode);

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

        public BoundingBox Bounds
        {
            get { return root.Bounds; }
        }

        public void Select(BoundingBox bounds, List<Triangle> triangles)
        {
            root.Select(bounds, triangles);
        }
    }
}
