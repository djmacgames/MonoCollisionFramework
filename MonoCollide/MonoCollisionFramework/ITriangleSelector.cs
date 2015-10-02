using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public interface ITriangleSelector
    {
        int Count
        {
            get;
        }

        object Tag
        {
            get;
            set;
        }

        void Select(BoundingBox bounds, List<Triangle> triangles);
    }
}
