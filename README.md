# Procedural Animation

This project implements Procedural Animation in the form of Inverse Kinematics. Unity's Animation Rigging package is used to facilitate Inverse Kinematics. A robo sphere is used to showcase the procedural 3 legged walk animation on uneven terrain. In addition to this lerping and smoothing effects for animations are applied via my own Curve System which is based on Unity's animation curve.

## Highlights
+ [Character Controller](#character-controller)
+ [Movement Controller](#movement-controller)
+ [Procedural Animation Scripts](#procedural-animation-scripts)
  + [Two-Bone IK Constraint](#two-bone-ik-constraint)
  + [Multi-Position Constraint](#multi-position-constraint)
  + [LegConstraintController](#legconstraintcontroller)
  + [Grounding](#grounding)
+ [Utility Scripts](#utility-scripts)
+ [References](#references)
+ [Dependencies](#dependencies)


## **[Character Controller](Assets/Scripts/CharacterController/RoboSphereCharacterController.cs)**
The character controller coordinates with animator, rig builder, movement controller, etc to update the state of the character. Depending upon the current state an animation is triggered by setting an animator parameter, and using curve system to interpolate animation state times and update rig weights or run speeds.

## **[Movement Controller](/Assets/Scripts/MovementSystem/MovementController.cs)**
A generic movement controller is made to move a rigid body with physics force. It is capable of basic directional movements of forward/backward and sideways strafing, the turning is achieved by attaching a Rotator script. Running is achieved by multiplying the current walk speed when specific key is being pressed. Action to jump is achievable with additional mid air jumps.

![Movement](https://user-images.githubusercontent.com/43366313/213876008-8d018b33-5e60-421d-be74-320fba65ca66.gif)<br>*Robot Movement*

## **Procedural Animation Scripts**
These are the scripts facilitating the procedural walking of the robot

### Two-Bone IK Constraint
This constraint is a component provided by Unity's Animation Rigging system and is responible for reading a target transform and updating two transforms(usually attached to a single joint) by performing inverse kinematics calcuations based on target's position and rotation. This component is attached to all the legs of the robot and the target is updated

![Two-Bone IK](https://user-images.githubusercontent.com/43366313/213876731-00683e8d-f592-4374-aeaf-b67b603ae7fc.png)<br>*Two-Bone IK Constriant*

### Multi-Position Constraint
This constraint deals with updating a target transform based on multiple source transforms. This component is attached to the main body with sources of leg transforms to give more procedural body shake on movement.

![Multi-Position](https://user-images.githubusercontent.com/43366313/213876626-0e27b13a-0954-4458-8290-71762522f101.png)<br>*Multi-Position Constraint*

### Other Constraint
There are many more constraints provided by Unity's Animation Rigging system to faciliate for various procedural animations like looking at objects, twisting transforms, using chain IK, etc.

### **[LegConstraintController](/Assets/Scripts/ProceduralAnimation/LegConstraintController.cs)**
This script is responsible for updating the leg position. Depending upon the step speed, step height and step distance the target transform is updated each frame. 
The path to follow using these properties is interpolated by the curves, different types of curves can provide various effects like spring, bouncy, ease, etc.
It also takes care of resetting the leg to its ideal position after being stationary for some time.
By providing different controllers to its *CoordinatingControllers* array, it talks with other controllers to decide whether it is appropriate to take a step. This behaviour helps avoid stepping multiple legs at the same time.

### **Grounding**
This script is responsible to provide a grounding transform the LegConstrainController. Grounding can be achieved either by [raycasting](/Assets/Scripts/ProceduralAnimation/RaycastGrounding.cs) or [spherecasting](/Assets/Scripts/ProceduralAnimation/SphereCastGrounding.cs) from an arbitrary point above the desired leg foot placement.

![AnimationTest](https://user-images.githubusercontent.com/43366313/213876062-58ad07fe-7b93-4e00-ba98-585522eb3660.gif)<br>*Animation Test*

![FootRebalance](https://user-images.githubusercontent.com/43366313/213876059-b54a2c13-4e15-40ce-9519-6dd2ffc51a95.gif)<br>*Foot Rebalancing*

## **Utility Scripts**
These scripts provide basic utilities like updating transforms by following or rotating them. They are also used in the form of a basic third person controller by attaching them to the camera.

## References
1. [Catlike Coding](https://catlikecoding.com/unity/tutorials)
2. [Animation Rigging Documentation](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.0/manual/index.html)

## Dependencies
1. [Robo Sphere Asset](https://assetstore.unity.com/packages/3d/characters/robots/robot-sphere-136226)
