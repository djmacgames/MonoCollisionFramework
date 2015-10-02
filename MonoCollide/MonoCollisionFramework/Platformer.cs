using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCollisionFramework
{
    public class Platformer : IContact
    {
        private Collider collider;
        private Vector3 position = Vector3.Up * 150;
        private Vector3 offset = new Vector3(0, 200, 200);
        private Vector3 velocity = Vector3.Zero;
        private Vector3 groundNormal = Vector3.Zero;
        private Matrix orientation = Matrix.Identity;
        private Matrix directionMatrix = Matrix.Identity;
        private bool onGround = false;
        private bool spaceDown = false;
        private float jump = 0;
        private float jumpHeight = 600;
        private float gravityDec = 100;
        private float maxGravity = 1000;
        private float jumpDec = 100;
        private float horizontalSpeed = 100;
        private float jumpHorizontalSpeed = 400;
        private float collisionRadius = 7;
        private float angularVelocity = -180;
        private float angle = 0;
        private float groundSlope = 0.45f;

        public Platformer(Collider collider)
        {
            this.collider = collider;
        }

        public Collider Collider
        {
            get { return collider; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Vector3 GroundNormal
        {
            get { return groundNormal; }
            set { groundNormal = value; }
        }

        public Matrix Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        public Matrix DirectionMatrix
        {
            get { return directionMatrix; }
            set { directionMatrix = value; }
        }

        public bool OnGround
        {
            get { return onGround; }
            set { onGround = value; }
        }

        public float Jump
        {
            get { return jump; }
            set { jump = value; }
        }

        public float JumpHeight
        {
            get { return jumpHeight; }
            set { jumpHeight = value; }
        }

        public float GravityDec
        {
            get { return gravityDec; }
            set { gravityDec = value; }
        }

        public float MaxGravity
        {
            get { return maxGravity; }
            set { maxGravity = value; }
        }

        public float JumpDec
        {
            get { return jumpDec; }
            set { jumpDec = value; }
        }

        public float HorizontalSpeed
        {
            get { return horizontalSpeed; }
            set { horizontalSpeed = value; }
        }

        public float JumpHorizontalSpeed
        {
            get { return jumpHorizontalSpeed; }
            set { jumpHorizontalSpeed = value; }
        }

        public float CollisionRadius
        {
            get { return collisionRadius; }
            set { collisionRadius = value; }
        }

        public float AngularVelocity
        {
            get { return angularVelocity; }
            set { angularVelocity = value; }
        }

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public float GroundSlope
        {
            get { return groundSlope; }
            set { groundSlope = value; }
        }

        public virtual Matrix CalcWorld(float scale, Vector3 zeroTranslation)
        {
            return Matrix.CreateTranslation(zeroTranslation) * Matrix.CreateRotationX(MathHelper.ToRadians(angle)) * directionMatrix * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
        }

        public virtual bool ContactMade(Collider collider, ITriangleSelector selector, Triangle triangle, Vector3 iPoint, int edge)
        {
            Vector3 n = triangle.Normal;

            if (edge != -1)
            {
                n = Vector3.Normalize(position - iPoint);
            }

            if (n.Y > groundSlope)
            {
                onGround = true;
                velocity.Y = 0;
                groundNormal += n;
            }
            else if (n.Y < -groundSlope)
            {
                velocity.Y = 0;
                jump = 0;
            }
            return true;
        }

        public virtual void HandleMouseAndKeyboard(Viewport viewport)
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();
            bool sdown = keyboard.IsKeyDown(Keys.Space);

            if (sdown && !spaceDown && onGround)
            {
                spaceDown = true;
                jump = jumpHeight;
                velocity.Y = 0;
                onGround = false;
            }
            else if (!sdown)
            {
                spaceDown = false;
            }

            float speed = horizontalSpeed;

            if (!onGround)
            {
                speed = jumpHorizontalSpeed;
            }

            velocity.X = 0;
            velocity.Z = 0;

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                float dx = mouse.X - viewport.Width / 2;
                float dy = viewport.Height / 2 - mouse.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);

                if (len > float.Epsilon)
                {
                    Vector3 f = offset;
                    Vector3 r;

                    f.Y = 0;
                    f.Normalize();
                    r = Vector3.Normalize(Vector3.Cross(f, Vector3.Up));

                    dx /= len;
                    dy /= len;

                    velocity -= dx * r * speed + dy * f * speed;
                }
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (onGround)
            {
                velocity.Y = -2 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                velocity.Y = Math.Max(-maxGravity, velocity.Y - gravityDec + jump);
                jump = Math.Max(0, jump - jumpDec);
            }

            Vector3 d = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (d.Length() > collisionRadius)
            {
                d = Vector3.Normalize(d) * collisionRadius;
            }
            d = Vector3.TransformNormal(d, orientation);

            position += d;

            onGround = false;
            orientation = Matrix.Identity;
            groundNormal = Vector3.Zero;
            collider.Collide(ref position, collisionRadius * 1.1f, this);

            Vector3 r, u, f;

            if (onGround)
            {
                u = Vector3.Normalize(groundNormal);
                r = Vector3.UnitX;
                f = Vector3.Normalize(Vector3.Cross(u, r));
                r = Vector3.Normalize(Vector3.Cross(f, u));
                orientation.Right = r;
                orientation.Up = u;
                orientation.Forward = f;
            }

            f = velocity;
            f.Y = 0;

            if (f.Length() > 0.001f)
            {
                u = Vector3.Up;
                f.Normalize();
                r = Vector3.Normalize(Vector3.Cross(f, u));
                directionMatrix.Right = r;
                directionMatrix.Up = u;
                directionMatrix.Forward = f;

                angle += angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (angle < 0)
                {
                    angle += 360;
                }
                else if (angle >= 360)
                {
                    angle -= 360;
                }
            }
        }
    }
}
