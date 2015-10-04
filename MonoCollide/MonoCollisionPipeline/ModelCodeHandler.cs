using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoCollisionPipeline
{
    public class ModelCodeHandler : ICodeHandler
    {
        private NodeContent root = new NodeContent();
        private MeshBuilder builder = new MeshBuilder();
        private Matrix matrix = Matrix.Identity;
        private Noise noise = new Noise();
        private Dictionary<string, MaterialContent> materials = new Dictionary<string, MaterialContent>();

        public NodeContent Root
        {
            get { return root; }
        }

        public MeshBuilder Builder
        {
            get { return builder; }
        }

        public Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        public Noise Noise
        {
            get { return noise; }
        }

        public Dictionary<string, MaterialContent> Materials
        {
            get { return materials; }
        }

        public virtual bool HandleCommand(string name, Arguments args, ContentImporterContext context)
        {
            if (name == "material")
            {
                BasicMaterialContent material = new BasicMaterialContent();

                material.DiffuseColor = new Vector3(args[1].FloatValue, args[2].FloatValue, args[3].FloatValue);
                material.SpecularColor = new Vector3(args[4].FloatValue, args[5].FloatValue, args[6].FloatValue);
                material.SpecularPower = args[7].FloatValue;
                material.EmissiveColor = new Vector3(0, 0, 0);
                material.Alpha = 1;
                material.VertexColorEnabled = false;

                if (args.Count == 9)
                {
                    material.Texture = new ExternalReference<TextureContent>(args[8].StrValue);
                }
                materials.Add(args[0].StrValue, material);
            }
            else if (name == "box")
            {
                float sx = args[0].FloatValue;
                float sy = args[1].FloatValue;
                float sz = args[2].FloatValue;

                builder.MakeBox(sx, sy, sz, args[3].IntValue, args[4].IntValue, args[5].IntValue);
                builder.CalcTexCoords(new Vector3(sx / 2, sy / 2, sz / 2), args[6].FloatValue);
            }
            else if (name == "calcNormals")
            {
                builder.CalcNormals(args[0].BoolValue);
            }
            else if (name == "perturb")
            {
                builder.Perturb(noise, args[0].FloatValue, args[1].FloatValue);
            }
            else if (name == "smooth")
            {
                builder.Smooth();
            }
            else if (name == "subdivide")
            {
                builder.Subdivide();
            }
            else if (name == "translate")
            {
                matrix *= Matrix.CreateTranslation(args[0].FloatValue, args[1].FloatValue, args[2].FloatValue);
            }
            else if (name == "scale")
            {
                matrix *= Matrix.CreateScale(args[0].FloatValue, args[1].FloatValue, args[2].FloatValue);
            }
            else if (name == "rotate")
            {
                matrix *= Matrix.CreateRotationX(MathHelper.ToRadians(args[0].FloatValue));
                matrix *= Matrix.CreateRotationY(MathHelper.ToRadians(args[1].FloatValue));
                matrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(args[2].FloatValue));
            }
            else if (name == "rotateOrientation")
            {
                Vector3 u = Vector3.Normalize(new Vector3(args[0].FloatValue, args[1].FloatValue, args[2].FloatValue));
                Vector3 r = Vector3.Normalize(new Vector3(args[3].FloatValue, args[4].FloatValue, args[5].FloatValue));
                Vector3 f = Vector3.Normalize(Vector3.Cross(r, u));
                Matrix o = Matrix.Identity;

                r = Vector3.Normalize(Vector3.Cross(f, u));
                o.Up = u;
                o.Right = r;
                o.Forward = f;
                matrix *= o;
                matrix *= Matrix.CreateTranslation(u * args[6].FloatValue);
            }
            else if (name == "rotateAxisAngle")
            {
                Vector3 axis = new Vector3(args[0].FloatValue, args[1].FloatValue, args[2].FloatValue);

                matrix *= Matrix.CreateFromAxisAngle(Vector3.Normalize(axis), MathHelper.ToRadians(args[3].FloatValue));
            }
            else if (name == "build")
            {
                builder.Transform(matrix);

                MeshContent mesh = builder.Build(materials[args[1].StrValue]);

                mesh.Name = args[0].StrValue;
                root.Children.Add(mesh);
                matrix = Matrix.Identity;
                builder.Clear();
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
