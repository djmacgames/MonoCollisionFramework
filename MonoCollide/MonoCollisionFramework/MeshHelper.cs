using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCollisionFramework
{
    public static class MeshHelper
    {
        public static void AddTriangles(ModelMesh mesh, List<Vector3> positions, List<int> indices)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                Vector3[] vb = new Vector3[part.NumVertices];
                short[] ib = new short[part.PrimitiveCount * 3];
                int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                part.VertexBuffer.GetData<Vector3>(part.VertexOffset * stride, vb, 0, vb.Length, stride);
                part.IndexBuffer.GetData<short>(part.StartIndex * 2, ib, 0, ib.Length);

                foreach (short i in ib)
                {
                    indices.Add(positions.Count + i);
                }
                positions.AddRange(vb);
            }
        }

        public static BoundingBox GetBounds(ModelMesh mesh)
        {
            List<Vector3> points = new List<Vector3>();

            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                Vector3[] vb = new Vector3[part.NumVertices];
                int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                part.VertexBuffer.GetData<Vector3>(part.VertexOffset * stride, vb, 0, vb.Length, stride);

                points.AddRange(vb);
            }
            return BoundingBox.CreateFromPoints(points);
        }
    }
}
