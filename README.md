# Procedural Animation

This project implements Procedural Animation in the form of Inverse Kinematics. Unity's Animation Rigging package is used to facilitate Inverse Kinematics. A robo sphere is used to showcase the procedural 3 legged walk animation on uneven terrain. In addition to this lerping and smoothing effects for animations are applied via my own Curve System which is based on Unity's animation curve.

## Highlights
+ [Character Controller](#character-controller)
+ [Movement Controller](#movement-controller)
+ [Procedural Animation Scripts](#procedural-animation-scripts)
  + [LegConstraintController](#legconstraintcontroller)
  + [Grounding](#grounding)
+ [Utility Scripts](#utility-scripts)
+ [References](#references)
+ [Dependencies](#dependencies)


## **[Character Controller](Assets/Scripts/CharacterController/RoboSphereCharacterController.cs)**
The character controller coordinates with animator, rig builder, movement controller, etc to update the state of the character. Depending upon the current state an animation is triggered by setting an animator parameter, and using curve system to interpolate animation state times and update rig weights or run speeds.

## **[Movement Controller](/Assets/Scripts/MovementSystem/MovementController.cs)**
A generic movement controller is made to move a rigid body with physics force. It is capable of basic directional movements of forward/backward and sideways strafing, the turning is achieved by attaching a Rotator script. Running is achieved by multiplying the current walk speed when specific key is being pressed. Action to jump is achievable with additional mid air jumps.

## **Procedural Animation Scripts**
These are the scripts facilitating the procedural walking of the robot

### **[LegConstraintController](/Assets/Scripts/ProceduralAnimation/LegConstraintController.cs)**
This script is responsible for updating the leg position. Depending upon the step speed, step height and step distance the target transform is updated each frame. 
The path to follow using these properties is interpolated by the curves, different types of curves can provide various effects like spring, bouncy, ease, etc.
It also takes care of resetting the leg to its ideal position after being stationary for some time.
By providing different controllers to its *CoordinatingControllers* array, it talks with other controllers to decide whether it is appropriate to take a step. This behaviour helps avoid stepping multiple legs at the same time.

### **Grounding**
This script is responsible to provide a grounding transform the LegConstrainController. Grounding can be achieved either by [raycasting](/Assets/Scripts/ProceduralAnimation/RaycastGrounding.cs) or [spherecasting](/Assets/Scripts/ProceduralAnimation/SphereCastGrounding.cs) from an arbitrary point above the desired leg foot placement.

## **Utility Scripts**
These scripts provide basic utilities like updating transforms by following or rotating them. They are also used in the form of a basic third person controller by attaching them to the camera.

## References
1. [Catlike Coding](https://catlikecoding.com/unity/tutorials)
2. [Animation Rigging Documentation](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.0/manual/index.html)

## Dependencies
1. [Robo Sphere Asset](https://assetstore.unity.com/packages/3d/characters/robots/robot-sphere-136226)