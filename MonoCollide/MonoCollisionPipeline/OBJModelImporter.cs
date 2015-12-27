using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

namespace MonoCollisionPipeline
{
    [ContentImporter(".obj", DisplayName = "OBJ Model Importer", DefaultProcessor = "ModelProcessor")]
    public class OBJModelImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            Dictionary<string, BasicMaterialContent> materials = new Dictionary<string, BasicMaterialContent>();
            List<Vector3> plist = new List<Vector3>();
            List<Vector2> tlist = new List<Vector2>();
            List<Vector3> nlist = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            TImport root = new TImport();
            string[] lines = File.ReadAllLines(filename);
            MeshContent mesh = null;
            GeometryContent geomerty = null;
            string directory = Path.GetDirectoryName(filename);

            foreach (string l in lines)
            {
                string line = l.Trim();

                if (!line.StartsWith("#"))
                {
                    string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length != 0)
                    {
                        if (tokens[0] == "mtllib")
                        {
                            ImportMaterials(Path.Combine(directory, tokens[1]), materials);
                        }
                        else if (tokens[0] == "o")
                        {
                            mesh = new MeshContent();
                            mesh.Name = tokens[1];
                            root.Children.Add(mesh);
                        }
                        else if (tokens[0] == "usemtl")
                        {
                            if (geomerty != null)
                            {
                                geomerty.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), texCoords);
                                geomerty.Vertices.Channels.Add(VertexChannelNames.Normal(0), normals);
                            }
                            geomerty = new GeometryContent();
                            geomerty.Material = materials[tokens[1]];
                            mesh.Geometry.Add(geomerty);
                            texCoords.Clear();
                            normals.Clear();
                        }
                        else if (tokens[0] == "v")
                        {
                            plist.Add(new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])));
                        }
                        else if (tokens[0] == "vt")
                        {
                            tlist.Add(new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2])));
                        }
                        else if (tokens[0] == "vn")
                        {
                            nlist.Add(new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])));
                        }
                        else if (tokens[0] == "f")
                        {
                            int count = mesh.Positions.Count;
                            List<Vector2> vt = new List<Vector2>();
                            List<Vector3> vn = new List<Vector3>();

                            for (int i = 1; i != tokens.Length; i++)
                            {
                                string[] vertex = tokens[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                                mesh.Positions.Add(plist[int.Parse(vertex[0]) - 1]);
                                vt.Add(tlist[int.Parse(vertex[1]) - 1]);
                                vn.Add(nlist[int.Parse(vertex[2]) - 1]);
                            }

                            int triangles = vt.Count - 2;

                            for (int i = 0; i != triangles; i++)
                            {
                                int n = geomerty.Vertices.VertexCount;

                                geomerty.Vertices.Add(count + i + 2);
                                geomerty.Vertices.Add(count + i + 1);
                                geomerty.Vertices.Add(count);
                                geomerty.Indices.Add(n++);
                                geomerty.Indices.Add(n++);
                                geomerty.Indices.Add(n++);

                                texCoords.Add(vt[i + 2]);
                                texCoords.Add(vt[i + 1]);
                                texCoords.Add(vt[0]);

                                normals.Add(vn[i + 2]);
                                normals.Add(vn[i + 1]);
                                normals.Add(vn[0]);
                            }
                        }
                    }
                }
            }
            if (geomerty != null)
            {
                geomerty.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), texCoords);
                geomerty.Vertices.Channels.Add(VertexChannelNames.Normal(0), normals);
            }
            return root;
        }

        private void ImportMaterials(string filename, Dictionary<string, BasicMaterialContent> materials)
        {
            string[] lines = File.ReadAllLines(filename);
            string directory = Path.GetDirectoryName(filename);
            BasicMaterialContent material = null;

            foreach (string l in lines)
            {
                string line = l.Trim();

                if (!line.StartsWith("#"))
                {
                    string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length != 0)
                    {
                        if (tokens[0] == "newmtl")
                        {
                            material = new BasicMaterialContent();
                            material.Name = tokens[1];
                            material.SpecularColor = new Vector3(0, 0, 0);
                            material.EmissiveColor = new Vector3(0, 0, 0);
                            material.DiffuseColor = new Vector3(1, 1, 1);
                            material.SpecularPower = 8;
                            material.VertexColorEnabled = false;
                            material.Alpha = 1;

                            materials.Add(material.Name, material);
                        }
                        else if (tokens[0] == "map_Kd")
                        {
                            material.Texture = new ExternalReference<TextureContent>(Path.Combine(directory, tokens[1]));
                        }
                        else if (tokens[0] == "Kd")
                        {
                            material.DiffuseColor =
                                new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                        }
                    }
                }
            }
        }
    }
}
