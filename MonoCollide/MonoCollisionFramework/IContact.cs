using System;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public interface IContact
    {
        bool ContactMade(ITriangleSelector selector, Triangle triangle, Vector3 iPoint, int edge);
    }
}
