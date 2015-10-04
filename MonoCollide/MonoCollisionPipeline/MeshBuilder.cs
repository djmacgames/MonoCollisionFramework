using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoCollisionPipeline
{
    public class MeshBuilder
    {
        private struct Vertex
        {
            public int Index;
            public int Edge;
            public Vector3 Position;
            public Vector3 Normal;

            public Vertex(int index, Vector3 position)
            {
                Index = index;
                Edge = -1;
                Position = position;
                Normal = Vector3.UnitY;
            }
        }

        private struct Edge
        {
            public int Index;
            public int Next;
            public int Pair;
            public int Vertex;
            public int Face;
            public Vector2 TexCoord;
            public Vector3 Normal;

            public Edge(int index)
            {
                Index = index;
                Next = -1;
                Pair = -1;
                Vertex = -1;
                Face = -1;
                TexCoord = Vector2.Zero;
                Normal = Vector3.UnitY;
            }
        }

        private struct Face
        {
            public int Index;
            public int Edge;
            public Vector3 Normal;

            public Face(int index)
            {
                Index = index;
                Edge = -1;
                Normal = Vector3.UnitY;
            }
        }

        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();
        private List<Face> faces = new List<Face>();
        private Dictionary<string, int> vertexEdges = new Dictionary<string, int>();

        public MeshBuilder()
        {
        }

        public int VertexCount
        {
            get { return vertices.Count; }
        }

        public int EdgeCount
        {
            get { return edges.Count; }
        }

        public int FaceCount
        {
            get { return faces.Count; }
        }

        public void Clear()
        {
            vertices.Clear();
            edges.Clear();
            faces.Clear();
            vertexEdges.Clear();
        }

        public int GetVertexEdge(int vertex)
        {
            return vertices[vertex].Edge;
        }

        public Vector3 GetVertexPosition(int vertex)
        {
            return vertices[vertex].Position;
        }

        public void SetVertexPosition(int vertex, Vector3 position)
        {
            Vertex v = vertices[vertex];

            v.Position = position;
            vertices[vertex] = v;
        }

        public Vector3 GetVertexNormal(int vertex)
        {
            return vertices[vertex].Normal;
        }

        public void SetVertexNormal(int vertex, Vector3 normal)
        {
            Vertex v = vertices[vertex];

            v.Normal = normal;
            vertices[vertex] = v;
        }

        public int GetEdgeNext(int edge)
        {
            return edges[edge].Next;
        }

        public int GetEdgePair(int edge)
        {
            return edges[edge].Pair;
        }

        public int GetEdgeVertex(int edge)
        {
            return edges[edge].Vertex;
        }

        public int GetEdgeFace(int edge)
        {
            return edges[edge].Face;
        }

        public Vector2 GetEdgeTexCoord(int edge)
        {
            return edges[edge].TexCoord;
        }

        public void SetEdgeTexCoord(int edge, Vector2 texCoord)
        {
            Edge e = edges[edge];

            e.TexCoord = texCoord;
            edges[edge] = e;
        }

        public Vector3 GetEdgeNormal(int edge)
        {
            return edges[edge].Normal;
        }

        public void SetEdgeNormal(int edge, Vector3 normal)
        {
            Edge e = edges[edge];

            e.Normal = normal;
            edges[edge] = e;
        }

        public int GetFaceEdge(int face)
        {
            return faces[face].Edge;
        }

        public Vector3 GetFaceNormal(int face)
        {
            return faces[face].Normal;
        }

        public void SetFaceNormal(int face, Vector3 normal)
        {
            Face f = faces[face];

            f.Normal = normal;
            faces[face] = f;
        }

        public int AddVertex(Vector3 position)
        {
            Vertex v = new Vertex(vertices.Count, position);

            vertices.Add(v);

            return v.Index;
        }

        public int AddFace(params int[] indices)
        {
            if (indices.Length < 3)
            {
                throw new Exception("can not add a face with < 3 vertices");
            }

            Face f = new Face(faces.Count);
            int[] edges = new int[indices.Length];
            int count = this.edges.Count;

            for (int i = 0; i != indices.Length; i++)
            {
                Edge e = new Edge(this.edges.Count);
                int index = indices[i];

                if (index < 0 || index >= vertices.Count)
                {
                    throw new Exception("can not add a face with an invalid vertex index");
                }

                Vertex v = vertices[index];

                if (v.Edge == -1)
                {
                    v.Edge = e.Index;
                    vertices[index] = v;
                }
                e.Vertex = index;
                e.Face = f.Index;
                e.Next = count + (i + 1) % indices.Length;
                edges[i] = e.Index;
                this.edges.Add(e);
            }
            f.Edge = edges[0];
            faces.Add(f);

            for (int i = 0; i != indices.Length; i++)
            {
                int v1 = indices[i];
                int v2 = indices[(i + 1) % indices.Length];
                string key = KeyFor(v1, v2);

                if (vertexEdges.ContainsKey(key))
                {
                    Edge e1 = this.edges[vertexEdges[key]];
                    Edge e2 = this.edges[edges[i]];

                    if (e1.Pair != -1)
                    {
                        throw new Exception("edge already has a pair");
                    }
                    e1.Pair = e2.Index;
                    e2.Pair = e1.Index;
                    this.edges[e1.Index] = e1;
                    this.edges[e2.Index] = e2;
                }
                else
                {
                    vertexEdges.Add(key, edges[i]);
                }
            }
            return f.Index;
        }

        public void CalcNormals(bool smooth)
        {
            for (int i = 0; i != FaceCount; i++)
            {
                int e1 = GetFaceEdge(i);
                int e2 = GetEdgeNext(e1);
                int e3 = GetEdgeNext(e2);
                Vector3 p1 = GetVertexPosition(GetEdgeVertex(e1));
                Vector3 p2 = GetVertexPosition(GetEdgeVertex(e2));
                Vector3 p3 = GetVertexPosition(GetEdgeVertex(e3));

                SetFaceNormal(i, Vector3.Normalize(Vector3.Cross(p1 - p2, p2 - p3)));
            }

            for (int i = 0; i != VertexCount; i++)
            {
                int e1 = GetVertexEdge(i);
                int e2 = e1;
                Vector3 n = Vector3.Zero;

                do
                {
                    n += GetFaceNormal(GetEdgeFace(e1));
                    if (GetEdgePair(e1) != -1)
                    {
                        e1 = GetEdgePair(e1);
                    }
                    e1 = GetEdgeNext(e1);
                } while (e1 != e2);

                SetVertexNormal(i, Vector3.Normalize(n));
            }

            for (int i = 0; i != EdgeCount; i++)
            {
                if (smooth)
                {
                    SetEdgeNormal(i, GetVertexNormal(GetEdgeVertex(i)));
                }
                else
                {
                    SetEdgeNormal(i, GetFaceNormal(GetEdgeFace(i)));
                }
            }
        }

        public void CalcTexCoords(Vector3 translation, float unit)
        {
            for (int i = 0; i != FaceCount; i++)
            {
                Vector3 n = GetFaceNormal(i);
                int e1 = GetFaceEdge(i);
                int e2 = e1;
                float x = Math.Abs(n.X);
                float y = Math.Abs(n.Y);
                float z = Math.Abs(n.Z);

                do
                {
                    Vector3 p = GetVertexPosition(GetEdgeVertex(e1)) + translation;
                    Vector2 t;

                    if (x >= y && x >= z)
                    {
                        t.X = p.Z / unit;
                        t.Y = p.Y / unit;
                    }
                    else if (y >= x && y >= z)
                    {
                        t.X = p.X / unit;
                        t.Y = p.Z / unit;
                    }
                    else
                    {
                        t.X = p.X / unit;
                        t.Y = p.Y / unit;
                    }
                    SetEdgeTexCoord(e1, t);

                    e1 = GetEdgeNext(e1);
                } while (e1 != e2);
            }
        }

        public void Perturb(Noise noise, float min, float max)
        {
            for (int i = 0; i != VertexCount; i++)
            {
                Vector3 n = GetVertexNormal(i);
                Vector3 p = GetVertexPosition(i);

                p += n * (min + noise.Next() * (max - min));
                SetVertexPosition(i, p);
            }
        }

        public void Transform(Matrix matrix)
        {
            Matrix it = Matrix.Transpose(Matrix.Invert(matrix));

            for (int i = 0; i != VertexCount; i++)
            {
                SetVertexPosition(i, Vector3.Transform(GetVertexPosition(i), matrix));
                SetVertexNormal(i, Vector3.TransformNormal(GetVertexNormal(i), it));
            }
            for (int i = 0; i != EdgeCount; i++)
            {
                SetEdgeNormal(i, Vector3.TransformNormal(GetEdgeNormal(i), it));
            }
            for (int i = 0; i != FaceCount; i++)
            {
                SetFaceNormal(i, Vector3.TransformNormal(GetFaceNormal(i), it));
            }
        }

        public void Subdivide()
        {
            Dictionary<string, int> edgeMidPoints = new Dictionary<string, int>();
            MeshBuilder mesh = new MeshBuilder();

            for (int i = 0; i != VertexCount; i++)
            {
                mesh.AddVertex(GetVertexPosition(i));
            }
            for (int i = 0; i != FaceCount; i++)
            {
                int e1 = GetFaceEdge(i);
                int e2 = e1;
                List<int> cornerPoints = new List<int>();
                List<int> midPoints = new List<int>();
                List<int> edges = new List<int>();
                Vector3 center = Vector3.Zero;
                Vector2 centerCoord = Vector2.Zero;

                do
                {
                    int v1 = GetEdgeVertex(e1);
                    int v2 = GetEdgeVertex(GetEdgeNext(e1));
                    Vector3 p1 = GetVertexPosition(v1);
                    Vector3 p2 = GetVertexPosition(v2);
                    string key = KeyFor(v1, v2);

                    if (!edgeMidPoints.ContainsKey(key))
                    {
                        edgeMidPoints[key] = mesh.AddVertex((p1 + p2) / 2);
                    }
                    midPoints.Add(edgeMidPoints[key]);
                    cornerPoints.Add(v1);
                    center += p1;
                    centerCoord += GetEdgeTexCoord(e1);
                    edges.Add(e1);

                    e1 = GetEdgeNext(e1);
                } while (e1 != e2);

                int centerPoint = mesh.AddVertex(center / (float)cornerPoints.Count);

                centerCoord /= (float)cornerPoints.Count;

                for (int j = 0; j != cornerPoints.Count; j++)
                {
                    int f, fe1, fe2, fe3, fe4;

                    if (j == 0)
                    {
                        f = mesh.AddFace(cornerPoints[0], midPoints[0], centerPoint, midPoints[midPoints.Count - 1]);
                        fe1 = mesh.GetFaceEdge(f);
                        fe2 = mesh.GetEdgeNext(fe1);
                        fe3 = mesh.GetEdgeNext(fe2);
                        fe4 = mesh.GetEdgeNext(fe3);
                        mesh.SetEdgeTexCoord(fe1, GetEdgeTexCoord(edges[0]));
                        mesh.SetEdgeTexCoord(fe2, (GetEdgeTexCoord(edges[1]) + GetEdgeTexCoord(edges[0])) / 2);
                        mesh.SetEdgeTexCoord(fe3, centerCoord);
                        mesh.SetEdgeTexCoord(fe4, (GetEdgeTexCoord(edges[edges.Count - 1]) + GetEdgeTexCoord(edges[0])) / 2);
                    }
                    else
                    {
                        f = mesh.AddFace(midPoints[j - 1], cornerPoints[j], midPoints[j], centerPoint);
                        fe1 = mesh.GetFaceEdge(f);
                        fe2 = mesh.GetEdgeNext(fe1);
                        fe3 = mesh.GetEdgeNext(fe2);
                        fe4 = mesh.GetEdgeNext(fe3);
                        mesh.SetEdgeTexCoord(fe1, (GetEdgeTexCoord(edges[j]) + GetEdgeTexCoord(edges[j - 1])) / 2);
                        mesh.SetEdgeTexCoord(fe2, GetEdgeTexCoord(edges[j]));
                        mesh.SetEdgeTexCoord(fe3, (GetEdgeTexCoord(edges[(j + 1) % edges.Count]) + GetEdgeTexCoord(edges[j])) / 2);
                        mesh.SetEdgeTexCoord(fe4, centerCoord);
                    }
                }
            }
            vertices = mesh.vertices;
            this.edges = mesh.edges;
            faces = mesh.faces;
            vertexEdges = mesh.vertexEdges;
        }

        public void Smooth()
        {
            List<Vector3> positions = new List<Vector3>(VertexCount);
            List<float> degrees = new List<float>(VertexCount);

            for (int i = 0; i != VertexCount; i++)
            {
                int e1 = GetVertexEdge(i);
                int e2 = e1;
                Vector3 position = Vector3.Zero;
                int degree = 0;

                do
                {
                    Vector3 center = Vector3.Zero;
                    int count = 0;
                    int e = e1;

                    do
                    {
                        center += GetVertexPosition(GetEdgeVertex(e));
                        count++;
                        e = GetEdgeNext(e);
                    } while (e != e1);

                    center /= count;
                    position += center;
                    degree++;

                    if (GetEdgePair(e1) != -1)
                    {
                        e1 = GetEdgePair(e1);
                    }
                    e1 = GetEdgeNext(e1);
                } while (e1 != e2);

                degrees.Add(degree);
                positions.Add(position / degree);
            }

            for (int i = 0; i != VertexCount; i++)
            {
                Vector3 p1 = GetVertexPosition(i);
                Vector3 p2 = positions[i];

                SetVertexPosition(i, p1 + 4.0f / degrees[i] * (p2 - p1));
            }
        }

        public void MakeBox(float sx, float sy, float sz, int divsX, int divsY, int divsZ)
        {
            List<int> vertices = new List<int>((divsX + 1) * (divsY + 1) * (divsZ + 1));

            for (int i = 0; i != divsX + 1; i++)
            {
                for (int j = 0; j != divsY + 1; j++)
                {
                    for (int k = 0; k != divsZ + 1; k++)
                    {
                        vertices.Add(-1);
                    }
                }
            }

            Clear();

            for (int i = 0; i != divsX + 1; i++)
            {
                for (int j = 0; j != divsY + 1; j++)
                {
                    AddVertex(
                        vertices,
                        -sx / 2 + i / (float)divsX * sx,
                        -sy / 2 + j / (float)divsY * sy,
                        -sz / 2,
                        i, j, 0, divsY, divsZ);
                    AddVertex(
                        vertices,
                        -sx / 2 + i / (float)divsX * sx,
                        -sy / 2 + j / (float)divsY * sy,
                        +sz / 2,
                        i, j, divsZ, divsY, divsZ);
                }
            }

            for (int i = 0; i != divsY + 1; i++)
            {
                for (int j = 0; j != divsZ + 1; j++)
                {
                    AddVertex(
                        vertices,
                        -sx / 2,
                        -sy / 2 + i / (float)divsY * sy,
                        -sz / 2 + j / (float)divsZ * sz,
                        0, i, j, divsY, divsZ);
                    AddVertex(
                        vertices,
                        +sx / 2,
                        -sy / 2 + i / (float)divsY * sy,
                        -sz / 2 + j / (float)divsZ * sz,
                        divsX, i, j, divsY, divsZ);
                }
            }

            for (int i = 0; i != divsX + 1; i++)
            {
                for (int j = 0; j != divsZ + 1; j++)
                {
                    AddVertex(
                        vertices,
                        -sx / 2 + i / (float)divsX * sx,
                        -sy / 2,
                        -sz / 2 + j / (float)divsZ * sz,
                        i, 0, j, divsY, divsZ);
                    AddVertex(
                        vertices,
                        -sx / 2 + i / (float)divsX * sx,
                        +sy / 2,
                        -sz / 2 + j / (float)divsZ * sz,
                        i, divsY, j, divsY, divsZ);
                }
            }

            for (int i = 0; i != divsX; i++)
            {
                for (int j = 0; j != divsY; j++)
                {
                    AddFace(vertices[GetVertexIndex(i, j, 0, divsY, divsZ)],
                        vertices[GetVertexIndex(i, j + 1, 0, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, j + 1, 0, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, j, 0, divsY, divsZ)]);
                    AddFace(vertices[GetVertexIndex(i, j, divsZ, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, j, divsZ, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, j + 1, divsZ, divsY, divsZ)],
                        vertices[GetVertexIndex(i, j + 1, divsZ, divsY, divsZ)]);
                }
            }

            for (int i = 0; i != divsY; i++)
            {
                for (int j = 0; j != divsZ; j++)
                {
                    AddFace(vertices[GetVertexIndex(0, i, j, divsY, divsZ)],
                        vertices[GetVertexIndex(0, i, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(0, i + 1, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(0, i + 1, j, divsY, divsZ)]);
                    AddFace(vertices[GetVertexIndex(divsX, i, j, divsY, divsZ)],
                        vertices[GetVertexIndex(divsX, i + 1, j, divsY, divsZ)],
                        vertices[GetVertexIndex(divsX, i + 1, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(divsX, i, j + 1, divsY, divsZ)]);
                }
            }

            for (int i = 0; i != divsX; i++)
            {
                for (int j = 0; j != divsZ; j++)
                {
                    AddFace(vertices[GetVertexIndex(i, 0, j, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, 0, j, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, 0, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(i, 0, j + 1, divsY, divsZ)]);
                    AddFace(vertices[GetVertexIndex(i, divsY, j, divsY, divsZ)],
                        vertices[GetVertexIndex(i, divsY, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, divsY, j + 1, divsY, divsZ)],
                        vertices[GetVertexIndex(i + 1, divsY, j, divsY, divsZ)]);
                }
            }

            float units = sx;

            if (sy < units)
            {
                units = sy;
            }
            if (sz < units)
            {
                units = sz;
            }

            CalcNormals(false);
            CalcTexCoords(new Vector3(sx / 2, sy / 2, sz / 2), units);
        }

        public MeshContent Build(MaterialContent material)
        {
            MeshContent mesh = new MeshContent();
            GeometryContent geometry = new GeometryContent();
            List<Vector2> texCoordChannel = new List<Vector2>();
            List<Vector3> normalChannel = new List<Vector3>();

            geometry.Material = material;

            for (int i = 0; i != FaceCount; i++)
            {
                int e1 = GetFaceEdge(i);
                int e2 = e1;
                int count = mesh.Positions.Count;
                int triangles = 0;
                List<Vector2> tlist = new List<Vector2>();
                List<Vector3> nlist = new List<Vector3>();

                do
                {
                    mesh.Positions.Add(GetVertexPosition(GetEdgeVertex(e1)));
                    tlist.Add(GetEdgeTexCoord(e1));
                    nlist.Add(GetEdgeNormal(e1));
                    triangles++;
                    e1 = GetEdgeNext(e1);
                } while (e1 != e2);

                triangles -= 2;

                for (int j = 0; j != triangles; j++)
                {
                    int n = geometry.Vertices.VertexCount;

                    geometry.Vertices.Add(count + j + 2);
                    geometry.Vertices.Add(count + j + 1);
                    geometry.Vertices.Add(count);
                    geometry.Indices.Add(n++);
                    geometry.Indices.Add(n++);
                    geometry.Indices.Add(n++);

                    texCoordChannel.AddRange(new Vector2[] { tlist[j + 2], tlist[j + 1], tlist[0] });
                    normalChannel.AddRange(new Vector3[] { nlist[j + 2], nlist[j + 1], nlist[0] });
                }
                if (geometry.Vertices.VertexCount >= 32000)
                {
                    geometry.Vertices.Channels.Add<Vector3>(VertexChannelNames.Normal(0), normalChannel);
                    geometry.Vertices.Channels.Add<Vector2>(VertexChannelNames.TextureCoordinate(0), texCoordChannel);
                    mesh.Geometry.Add(geometry);
                    normalChannel.Clear();
                    texCoordChannel.Clear();
                    geometry = new GeometryContent();
                    geometry.Material = material;
                }
            }
            if (geometry.Vertices.VertexCount != 0)
            {
                geometry.Vertices.Channels.Add<Vector3>(VertexChannelNames.Normal(0), normalChannel);
                geometry.Vertices.Channels.Add<Vector2>(VertexChannelNames.TextureCoordinate(0), texCoordChannel);
                mesh.Geometry.Add(geometry);
            }
            return mesh;
        }

        public string KeyFor(int v1, int v2)
        {
            if (v1 < v2)
            {
                return v1 + ":" + v2;
            }
            else
            {
                return v2 + ":" + v1;
            }
        }

        private int AddVertex(List<int> vertices, float x, float y, float z, int i, int j, int k, int divsY, int divsZ)
        {
            int l = GetVertexIndex(i, j, k, divsY, divsZ);

            if (vertices[l] == -1)
            {
                vertices[l] = AddVertex(new Vector3(x, y, z));
            }
            return vertices[l];
        }

        private int GetVertexIndex(int i, int j, int k, int divsY, int divsZ)
        {
            return i * (divsY + 1) * (divsZ + 1) + j * (divsZ + 1) + k;
        }
    }
}
