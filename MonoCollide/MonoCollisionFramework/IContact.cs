using System;
using Microsoft.Xna.Framework;

namespace MonoCollisionFramework
{
    public interface IContact
    {
        bool ContactMade(Collider collider, ITriangleSelector selector, Triangle triangle, Vector3 iPoint, int edge);
    }
}
