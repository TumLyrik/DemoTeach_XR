# DemoTeach – XR-Enhanced Robot Programming for Foundry Applications

## Overview
DemoTeach is an XR-assisted robot programming framework for intuitive programming of industrial robots in foundry environments. The system combines motion demonstration, digital twin visualization, and extended reality (XR) to enable operators to define robot tasks by physically demonstrating tool motions while receiving real-time visual feedback.

The framework integrates a Unity-based XR interface, ROS middleware, and industrial robot communication to support interactive trajectory teaching and automated trajectory generation.

This repository contains the research prototype used in the publication:

**XR-Enhanced DemoTeach: An Intuitive Digital Twin-Based Robot Programming for Foundry Applications**

---

## Features
- XR-based interactive robot programming
- Digital twin visualization of robot and workspace
- Motion recording via handheld tool demonstration
- Automatic trajectory generation from demonstrations
- ROS-based communication architecture
- Real-time inverse kinematics computation
- Visual feedback for collision and feasibility checks

---

## System Architecture
The system consists of three main modules:

### XR Interface (Unity)
- Visualization of robot digital twin
- User interaction through XR head-mounted display
- Motion recording during demonstration

### ROS Middleware
- Communication bridge between XR interface and robot controller
- Inverse kinematics (IK) computation
- Motion processing and task definition
- Trajectory generation

### Robot Control Layer
- Communication with industrial robot controller
- Execution of generated robot trajectories

---
## License

This project is licensed under the Apache License 2.0. See the LICENSE file for details.

---
## Acknowledgment

The code was developed during research conducted at the Fraunhofer Institute for Foundry, Composite and Processing Technology (IGCV) in collaboration with the Technical University of Munich (TUM).

---
## Disclaimer

This repository contains a research prototype intended for academic and experimental use. It may require further adaptation for industrial deployment.
