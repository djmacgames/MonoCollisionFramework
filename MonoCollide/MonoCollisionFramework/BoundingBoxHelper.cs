using System;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public static class BoundingBoxHelper
    {
        public static BoundingBox Transform(BoundingBox bounds, Matrix matrix)
        {
            BoundingBox t;

            t.Min = matrix.Translation;
            t.Max = matrix.Translation;

            t.Min.X += (matrix.M11 < 0) ? bounds.Max.X * matrix.M11 : bounds.Min.X * matrix.M11;
            t.Min.X += (matrix.M21 < 0) ? bounds.Max.Y * matrix.M21 : bounds.Min.Y * matrix.M21;
            t.Min.X += (matrix.M31 < 0) ? bounds.Max.Z * matrix.M31 : bounds.Min.Z * matrix.M31;
            t.Max.X += (matrix.M11 > 0) ? bounds.Max.X * matrix.M11 : bounds.Min.X * matrix.M11;
            t.Max.X += (matrix.M21 > 0) ? bounds.Max.Y * matrix.M21 : bounds.Min.Y * matrix.M21;
            t.Max.X += (matrix.M31 > 0) ? bounds.Max.Z * matrix.M31 : bounds.Min.Z * matrix.M31;

            t.Min.Y += (matrix.M12 < 0) ? bounds.Max.X * matrix.M12 : bounds.Min.X * matrix.M12;
            t.Min.Y += (matrix.M22 < 0) ? bounds.Max.Y * matrix.M22 : bounds.Min.Y * matrix.M22;
            t.Min.Y += (matrix.M32 < 0) ? bounds.Max.Z * matrix.M32 : bounds.Min.Z * matrix.M32;
            t.Max.Y += (matrix.M12 > 0) ? bounds.Max.X * matrix.M12 : bounds.Min.X * matrix.M12;
            t.Max.Y += (matrix.M22 > 0) ? bounds.Max.Y * matrix.M22 : bounds.Min.Y * matrix.M22;
            t.Max.Y += (matrix.M32 > 0) ? bounds.Max.Z * matrix.M32 : bounds.Min.Z * matrix.M32;

            t.Min.Z += (matrix.M13 < 0) ? bounds.Max.X * matrix.M13 : bounds.Min.X * matrix.M13;
            t.Min.Z += (matrix.M23 < 0) ? bounds.Max.Y * matrix.M23 : bounds.Min.Y * matrix.M23;
            t.Min.Z += (matrix.M33 < 0) ? bounds.Max.Z * matrix.M33 : bounds.Min.Z * matrix.M33;
            t.Max.Z += (matrix.M13 > 0) ? bounds.Max.X * matrix.M13 : bounds.Min.X * matrix.M13;
            t.Max.Z += (matrix.M23 > 0) ? bounds.Max.Y * matrix.M23 : bounds.Min.Y * matrix.M23;
            t.Max.Z += (matrix.M33 > 0) ? bounds.Max.Z * matrix.M33 : bounds.Min.Z * matrix.M33;

            return t;
        }
    }
}
