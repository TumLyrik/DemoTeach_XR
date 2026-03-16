********** Check before running **********
QRorigin => Station => MoCap => Rigidbody
	Optitrack Rigid Body (Rigid body ID ???) 
	Trigger Signal (Port Connect ???)
	Client Optitrack ???
QRorigin => Station => MoCap => Rigidbody => Tool => Slicer
	 Casting Slicing (Physics ???)
RosConnector
	Mixed Reality (Holographic Remoting)
Unity:
	0_robot_sim.sh
	1_rosbridge.sh
	2_tf_task.sh

********** KOS Setting **********
QRorigin in Easyteach_Kuka(world)
	Vector3(-0.16900003,0.0109999776,0)
	Vector3(270,90,0)
Station in QRorigin
	normal position
		Vector3(0,-0.16900003,-0.0109999776)
		Vector3(0,270,270)
	for recording
		Vector3(0.018999977,-0.136000007,-0.02999999)
MoCap in Station
	Vector3(0.0119781904,0.00176917005,0.200300097)
	Vector3(0,0,0)
Cam in MoCap
	Vector3(0,0,0)
	Vector3(0,0,0)
RigidBody in MoCap
	Vector3(-0.207000002,0.421999991,0.474999994)
	Vector3(270,90,0)
Client - Optitrack in RigidBody
	Vector3(0,0,0)
	Vector3(0,270,270)
Tool in RigidBody
	Vector3(0,0,1.35000002)
	Vector3(0,270,270)
Slicer in Tool
	Vector3(0,0,0)
	Vector3(0,0,180)
iiwa 14 in Station
	Vector3(0,0,0)
	Vector3(0,0,0)
QRpart in Station
	update(20251114)
	#此位置为优化位置，因为这样才能够切到Speiser下面的Anschnitt
	Vector3(0.222000003,0.218999997,0.657999992) 
	------------------
	Vector3(0.0717383027,0.192394435,0.597680867)
	Vector3(270,90,0)
part in QRpart
	update(20251114)
	Vector3(-0.0029096778,0.00866670907,-0.101710647)
	Vector3(0,270,270)
	------------------
	Vector3(-0.00100034266,0.010500133,-0.0929988921)
	Vector3(2.99999595,270,270)
铸件的优化位置
 <arg name="model_px" default="0.66"/> # unity,z of base in world tf#
 <arg name="model_py" default="-0.21"/> # unity,-x of base in world tf#
 <arg name="model_pz" default="0.1"/> # unity,y of base in world tf#
 <arg name="model_qz" default="0.92"/> # unity,-x of base in world tf#
 <arg name="model_qw" default="0.38"/> # unity,y of base in world tf#