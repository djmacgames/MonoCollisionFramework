# MonoCollisionFramework
A triangle sphere collision framework for mono game

![Screenshot](https://github.com/djmacgames/MonoCollisionFramework/blob/master/ScreenShot.png)

## Info

The framework is designed around 3 classes/interfaces ...

* Collider
* ITriangleSelector
* IContact

A Collider holds triangle selectors and has a method for colliding a sphere against
the selectors.

A TriangleSelector is responsible for providing triangles to the collider. There are 2 
current implementations of ITriangleSelector which are StaticTriangleSelector and
DynamicTriangleSelector. StaticTriangleSelector is faster, but if the triangles need
to move as the game or simulation is running then you need to use the
DynamicTriangleSelector.

When a collision occurs the collider calls on an implementation of IContact. The contact
is passed the collider, the triangle selector containing the collision triangle, the collision
triangle, the collision point an an edge index which is -1 if the collision was not on a triangle
edge or 0 - 2 for the triangle edge index that the collision occurred against. The contact
should return true to resolve the collision and false otherwise.

If you are creating a platformer type game then there is a specialized class called Platformer
that might suit your needs. The Platformer has a collider and contact implementation and manages
the physics for gravity, velocity and jumping. You can configure the platformer as it is as well as
subclass the platformer if needed.
