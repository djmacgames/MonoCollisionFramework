using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoCollisionFramework;

namespace MonoPlanet
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model scene;
        SpriteFont font;
        Platformer platformer = new Platformer(new Collider());
        Vector3 zeroTranslation = Vector3.Zero;
        Texture2D sky;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;
            IsMouseVisible = true;
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
                    BoundingBox bounds = MeshHelper.GetBounds(mesh);

                    platformer.Position = (bounds.Max + bounds.Min) / 2;
                    platformer.CollisionRadius = (bounds.Max.Y - bounds.Min.Y) / 2;
                }
                else
                {
                    List<Vector3> positions = new List<Vector3>();

                    MeshHelper.AddTriangles(mesh, positions);

                    platformer.Collider.Selectors.Add(new StaticTriangleSelector(Matrix.Identity, positions, 16));
                }
            }
            zeroTranslation = -platformer.Position;
            platformer.PlanetCollision = true;

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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(sky, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            // TODO: Add your drawing code here
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.Viewport.AspectRatio, 1, 5000);
            Matrix view = Matrix.CreateLookAt(platformer.Position + 100 * platformer.Up + 100 * platformer.Forward, platformer.Position, platformer.Up);

            foreach (ModelMesh mesh in scene.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.DirectionalLight0.Direction = platformer.Right;
                    effect.DirectionalLight1.Direction = -platformer.Up;
                    effect.DirectionalLight2.Direction = -platformer.Forward;
                    effect.Projection = projection;
                    effect.View = view;
                    if (mesh.Name == "ball")
                    {
                        effect.World = platformer.CalcWorld(1, zeroTranslation);
                    }
                    else
                    {
                        effect.World = Matrix.Identity;
                    }
                }
                mesh.Draw();
            }

            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.DrawString(font, "Press space key to jump and hold left mouse button down to move in that direction", Vector2.Zero, Color.LightBlue);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
