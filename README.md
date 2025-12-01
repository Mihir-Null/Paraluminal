# Paraluminal
A Game Engine that simulates Special Relativistic effects in real time while (trying to be) as accurate as causally possible.

This project implements a custom **special-relativistic game engine** designed for real-time simulation/integration of physics and optics in accelerated frames. It does this by utilizing a variant of Rindler coordinates (and derivation equations) to integrate four vectors and along the spacetime manifold as it evolves from the players perspective. Optics are handled by treating current game state as the local tangent Minkowski plane and using appropriate Lorentz transforms

The core idea is that every physical object evolves inside a **continuously re-oriented Rindler frame** whose +X direction is dynamically controlled by the player’s acceleration input. Objects keep their own Rindler-space positions/velocities and undergo time-dilation, Lorentz-factor–consistent motion, and proper acceleration–based thrust.
---

## **Features**

* Dynamic Rindler coordinate system (accelerated reference frame)
* Proper acceleration thrust in arbitrary player-controlled directions
* Rindler-consistent velocity updates, time dilation and space contraction via 4th-order symplectic-like Yoshida integration
* Optional use of real-world units (m, s, c)
* Seamless world-space ↔ Rindler-space coordinate transforms (Current Unity state-space corresponds to the player's simultaneity hyperplane)
* Automatic Floating origin and Player frame Global Coordinates
* Lorentz transform based shaders for relativistic doppler effect and beaming
* Simulation of lightspeed lag from the player's POV

## **Caveats**
* All RelativisticBodies (including the player) are currently assumed to be pointlike and are integrated as such.
* The engine currently does not account for rotation of the player frame as well as errors caused by instantaneous frame by frame acceleration, though these errors have been minimized as much as possible and are currently being worked on
* shaders treat each mesh as a monolith and apply the same color/light colorations across the object, more detailed shaders that operate on a vertex by vertex basis are currently being worked on.
* Similarly lightspeed "lag" for large objects (with respect to c) is currently inaccurate as massshadow operates on an object by object basis, also being worked on
---

## **Project Structure**

Most of the physics, integrators, and frame-handling scripts live in:

```
Assets/Physics/
```
As this is a WIP, scripts are constantly being updated and refactored, the most up to date versions of each script/component will have the highest numbering.
The key components there include:


### **PFrame**

The player-frame controller that updates the ship’s acceleration direction and communicates the thrust vector (`alpha`, `direction`) to all relativistic bodies. Also includes utilities such as dynamic change-of-basis transforms.


### **GlobalPhysics**

A simple ScriptableObject for storing and sharing the player’s instantaneous proper acceleration magnitude and direction across all physics objects, as well as other salient information.

### **RelativisticBody**

A full 3+1-dimensional **Rindler-frame relativistic integrator** using a 4th-order Yoshida drift–kick method. Tracks Rindler time `T`, proper time `τ`, local coordinates `(X,Y,Z)`, and relativistic velocity, while applying proper acceleration and transforming between player-aligned frames.

### **RelativisticBodyC** (***Deprecated***) 

A “units-correct” version of *RelativisticBody* that explicitly includes the speed of light `c` and works in real-world SI units (meters, seconds, m/s²). Useful for realism-focused gameplay or scientific visualization. **All functionality this added has been transferred to relativisticbody**

### **MassShadow**

The "Mass Shadow" refers to the version of the object that the player actually sees due to the fact that light can no longer be assumed to reach the player instantaneously. This is implemented via a ringbuffer system of object histories per Update call and can generate an accurate "shadow" of the object as the player would (have) seen it.

### **RelativisticCamera**

Provides for player side viewing of Mass Shadows as well as optical relativistic effects, currently provided by the "Umbra" class of Shaders

---

## **Usage**

1. Add **RelativisticBody** to any moving object.
2. Add **PFrame** to your player object and reference a **GlobalPhysics** asset.
4. Objects automatically transform into the new local basis each frame, with appropriate spatial contraction.
5. Scene objects update their world positions and proper time (tau) based on relativistic covariant integration.
6. Attach **MassShadow** to any RelativisticBody that needs simulation of lightspeed lag.
7. Utilize the provided shaders for simulation of redshift/blueshift and relativistic aberration.
8. Attach a RelativisticCamera to the Player
