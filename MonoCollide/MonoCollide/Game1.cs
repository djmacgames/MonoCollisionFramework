using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoCollisionFramework;

namespace MonoCollide
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model scene;
        Platformer platformer = new Platformer(new Collider());
        Vector3 startPosition;
        SpriteFont font;
        Texture2D sky;
        int triangeCount = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            scene = Content.Load<Model>("Scene");

            foreach (ModelMesh mesh in scene.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Vector3.Zero;
                }
                if (mesh.Name == "ball")
                {
                    BoundingBox b = MeshHelper.GetBounds(mesh);

                    platformer.Position = (b.Max + b.Min) / 2;
                    platformer.CollisionRadius = (b.Max.Y - b.Min.Y) / 2;
                }
                else
                {
                    List<Vector3> positions = new List<Vector3>();

                    MeshHelper.AddTriangles(mesh, positions);

                    ITriangleSelector selector = new StaticTriangleSelector(Matrix.Identity, positions, 16);

                    platformer.Collider.Selectors.Add(selector);
                    triangeCount += selector.Count;
                }
            }
            startPosition = platformer.Position;

            platformer.JumpHeight = 600;
            platformer.HorizontalSpeed = 200;
            platformer.JumpHorizontalSpeed = 300;

            font = Content.Load<SpriteFont>("Font");
            sky = Content.Load<Texture2D>("Sky");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            platformer.HandleMouseAndKeyboard(GraphicsDevice.Viewport);
            platformer.Update(gameTime);

            if (platformer.Position.Y < -400)
            {
                platformer.Position = startPosition;
                platformer.Angle = 0;
                platformer.DirectionMatrix = Matrix.Identity;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            string[] lines = new string[]
            {
                "Tested " + platformer.Collider.TrianglesTested + " of " + triangeCount + " triangle(s), in " + platformer.Collider.LoopCount + " loop(s)",
                "Frame rate = " + (int)(1.0f / gameTime.ElapsedGameTime.TotalSeconds),
                "Press space key to jump",
                "Press and hold left mouse button to move in the direction of the mouse"
            };

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(sky, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            Vector3 position = platformer.Position;

            if (position.Y < 0)
            {
                position.Y = 0;
            }

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.Viewport.AspectRatio, 1, 5000);
            Matrix view = Matrix.CreateLookAt(position + platformer.Offset, position, Vector3.UnitY);

            foreach (ModelMesh mesh in scene.Meshes)
            {
                foreach (IEffectMatrices effect in mesh.Effects)
                {
                    effect.Projection = projection;
                    effect.View = view;
                    if (mesh.Name == "ball")
                    {
                        effect.World = platformer.CalcWorld(1, -startPosition);
                    }
                    else
                    {
                        effect.World = Matrix.Identity;
                    }
                }
                mesh.Draw();
            }

            spriteBatch.Begin(SpriteSortMode.Immediate);
            for (int i = 0; i != lines.Length; i++)
            {
                spriteBatch.DrawString(font, lines[i], new Vector2(0, i * font.MeasureString(lines[i]).Y), Color.DarkBlue);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
