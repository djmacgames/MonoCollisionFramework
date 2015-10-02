using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

namespace MonoCollisionPipeline
{
    [ContentImporter(".txt", DisplayName = "Text Model Importer", DefaultProcessor = "ModelProcessor")]
    public class TextModelImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            TImport root = new TImport();
            MeshBuilder builder = new MeshBuilder();
            Matrix m = Matrix.Identity;
            Noise noise = new Noise(typeof(TextModelImporter), "MonoCollisionPipeline.Noise.txt");
            Dictionary<string, MaterialContent> materials = new Dictionary<string, MaterialContent>();
            string[] lines = File.ReadAllLines(filename);
            int lineNum = 1;

            try
            {
                foreach (string line in lines)
                {
                    string[] tokens = line.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (line.StartsWith("#") || line.Length == 0)
                    {
                        // Do Nothing
                    }
                    else if (tokens[0] == "material")
                    {
                        BasicMaterialContent material = new BasicMaterialContent();

                        material.DiffuseColor = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), float.Parse(tokens[4]));
                        material.SpecularColor = new Vector3(float.Parse(tokens[5]), float.Parse(tokens[6]), float.Parse(tokens[7]));
                        material.SpecularPower = float.Parse(tokens[8]);
                        material.EmissiveColor = new Vector3(0, 0, 0);
                        material.Alpha = 1;
                        material.VertexColorEnabled = false;

                        if (tokens.Length == 10)
                        {
                            string file = Path.Combine(Path.GetDirectoryName(filename), tokens[9]);

                            material.Texture = new ExternalReference<TextureContent>(file);
                        }
                        materials.Add(tokens[1], material);
                    }
                    else if (tokens[0] == "box")
                    {
                        float sx = float.Parse(tokens[1]);
                        float sy = float.Parse(tokens[2]);
                        float sz = float.Parse(tokens[3]);

                        builder.MakeBox(sx, sy, sz, int.Parse(tokens[4]), int.Parse(tokens[5]), int.Parse(tokens[6]));
                        builder.CalcTexCoords(new Vector3(sx / 2, sy / 2, sz / 2), float.Parse(tokens[7]));
                    }
                    else if (tokens[0] == "calcNormals")
                    {
                        builder.CalcNormals(bool.Parse(tokens[1]));
                    }
                    else if (tokens[0] == "perturb")
                    {
                        builder.Perturb(noise, float.Parse(tokens[1]), float.Parse(tokens[2]));
                    }
                    else if (tokens[0] == "smooth")
                    {
                        builder.Smooth();
                    }
                    else if (tokens[0] == "subdivide")
                    {
                        builder.Subdivide();
                    }
                    else if (tokens[0] == "translate")
                    {
                        m *= Matrix.CreateTranslation(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                    }
                    else if (tokens[0] == "scale")
                    {
                        m *= Matrix.CreateScale(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                    }
                    else if (tokens[0] == "rotate")
                    {
                        m *= Matrix.CreateRotationX(MathHelper.ToRadians(float.Parse(tokens[1])));
                        m *= Matrix.CreateRotationY(MathHelper.ToRadians(float.Parse(tokens[2])));
                        m *= Matrix.CreateRotationZ(MathHelper.ToRadians(float.Parse(tokens[3])));
                    }
                    else if (tokens[0] == "build")
                    {
                        builder.Transform(m);

                        MeshContent mesh = builder.Build(materials[tokens[2]]);

                        mesh.Name = tokens[1];
                        root.Children.Add(mesh);
                        m = Matrix.Identity;
                        builder.Clear();
                    }
                    else
                    {
                        throw new Exception("undefined");
                    }
                    lineNum++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " : line '" + lineNum + "'");
            }
            return root;
        }
    }
}
