/*
 * 通过rosbridge调用moveit的/compute_ik服务
 * 将获得的结果写入ik_j变量之中
*/

using WebSocketSharp;
using UnityEngine;
using TMPro;
using RosSharp.RosBridgeClient.MessageTypes.Moveit;
using RosSharp.RosBridgeClient;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

namespace RosSharp.RosBridgeClient
{

    public class IKClient : MonoBehaviour
    {
        // Array of transforms for the robot's joints in Unity
        //public Transform[] robotJoints; // Assign these in the Unity Inspector
        public float ik_j1;
        public float ik_j2;
        public float ik_j3;
        public float ik_j4;
        public float ik_j5;
        public float ik_j6;
        public float ik_j7;

        private WebSocket ws;
        private TriggerSignal TriggerSubscriber;
        private TFTransfer TFClient;
        private string ik_ee;
        private string ik_group;

        void Start()
        {
            // Connect to ROSBridge WebSocket server
            ws = new WebSocket("ws://172.16.2.5:9090");
            ws.OnMessage += OnMessageReceived;
            ws.Connect();
            TriggerSubscriber = FindObjectOfType<TriggerSignal>();
            TFClient = FindObjectOfType<TFTransfer>();
        }

        void Update()
        {
            if (!TriggerSubscriber.RosStream)
            {
                SendIKRequest();
            }
        }

        // Method to send the IK request
        public void SendIKRequest()
        {
            if (TFClient.stabscheifer)
            {
                ik_group = "arm";
                ik_ee = "lbr_link_ee";
            }
            else
            {
                ik_group = "arm_disc";
                ik_ee = "tool";
            }
            // Build the service request with the updated target pose
            string requestJson = JsonUtility.ToJson(new ServiceRequest
            {
                op = "call_service",
                service = "/lbr/compute_ik",
                args = new GetPositionIKRequest
                {
                    ik_request = new PositionIKRequest
                    {
                        group_name = ik_group, // arm
                        ik_link_name = ik_ee, // lbr_link_ee
                        pose_stamped = new PoseStamped_LBR
                        {
                            header = new Header
                            {
                                frame_id = "lbr_link_0"
                            },
                            pose = new Pose
                            {
                                position = new Position
                                {
                                    x = TFClient.px, //targetPosition.x, //
                                    y = TFClient.py, //targetPosition.y, //
                                    z = TFClient.pz //targetPosition.z //
                                },
                                orientation = new Orientation
                                {
                                    x = TFClient.qx, //(float)0.0,
                                    y = TFClient.qy, //(float)0.0,
                                    z = TFClient.qz, //(float)0.0,
                                    w = TFClient.qw //(float)1.0
                                }
                            }
                        },
                        robot_state = new RobotState
                        {
                            joint_state = new JointState
                            {
                                name = new string[] { "lbr_joint_1", "lbr_joint_2", "lbr_joint_3", "lbr_joint_4", "lbr_joint_5", "lbr_joint_6", "lbr_joint_7" },
                                position = new float[] { 0 * Mathf.Deg2Rad, 20 * Mathf.Deg2Rad, 0 * Mathf.Deg2Rad, -70 * Mathf.Deg2Rad, 0 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad, 0 * Mathf.Deg2Rad }
                            }
                        }, //不能开启，否则计算结果会有偏差
                        avoid_collisions = false
                    }
                }
            });
            ws.Send(requestJson); // Send the request
            //Debug.Log("IK Request Sent: " + requestJson);
        }

        void OnMessageReceived(object sender, MessageEventArgs e)
        {
            // Debug.Log("Received: " + e.Data);
            // Parse the response
            var response = JsonUtility.FromJson<ServiceResponse>(e.Data);
            if (response.result && response.values.error_code.val == 1)
            {
                //Debug.Log("IK Calculation Successful");
                // Extract joint positions
                float[] jointPositions = response.values.solution.joint_state.position;

                // Log the joint positions
                //Debug.Log("Joint Positions: " + string.Join(", ", jointPositions));
                for (int i = 0; i < 7; i++)
                {
                    GetType().GetField($"ik_j{i + 1}").SetValue(this, jointPositions[i] * Mathf.Rad2Deg);
                };
                // Assign joint positions to the Unity robot model
                //AssignJointsToRobot(jointPositions);
            }
            //else
            //{
            //    //Debug.LogError("IK Calculation Failed. Error Code: " + response.values.error_code.val);
            //    Debug.Log("IK Calculation Failed");
            //}
        }

        void OnDestroy()
        {
            if (ws != null && ws.IsAlive)
            {
                ws.Close();
            }
        }
    }

    // Define classes to represent the ROS messages
    [System.Serializable]
    public class ServiceRequest
    {
        public string op;
        public string service;
        public GetPositionIKRequest args;
    }

    [System.Serializable]
    public class GetPositionIKRequest
    {
        public PositionIKRequest ik_request;
    }

    [System.Serializable]
    public class PositionIKRequest
    {
        public string group_name;
        public string ik_link_name;
        public PoseStamped_LBR pose_stamped;
        public bool avoid_collisions;
        public RobotState robot_state;
    }

    [System.Serializable]
    public class RobotState
    {
        public JointState joint_state;
    }

    [System.Serializable]
    public class PoseStamped_LBR
    {
        public Header header;
        public Pose pose;
    }

    [System.Serializable]
    public class Header
    {
        public int seq;
        public string stamp;
        public string frame_id;
    }

    [System.Serializable]
    public class Pose
    {
        public Position position;
        public Orientation orientation;
    }

    [System.Serializable]
    public class Position
    {
        public float x, y, z;
    }

    [System.Serializable]
    public class Orientation
    {
        public float x, y, z, w;
    }

    [System.Serializable]
    public class ServiceResponse
    {
        public string op;
        public string service;
        public bool result;
        public IKResponseValues values;
    }

    [System.Serializable]
    public class IKResponseValues
    {
        public IKSolution solution;
        public ErrorCode error_code;
    }

    [System.Serializable]
    public class IKSolution
    {
        public JointState joint_state;
        public MultiDOFJointState multi_dof_joint_state;
        public bool is_diff;
    }

    [System.Serializable]
    public class JointState
    {
        public Header header;
        public string[] name;
        public float[] position; // Joint positions in radians
        public float[] velocity;
        public float[] effort;
    }

    [System.Serializable]
    public class MultiDOFJointState
    {
        public Header header;
        public string[] joint_names;
        public ROSTransform[] transforms; // Updated class name
        public Twist[] twist;
        public Wrench[] wrench;
    }

    [System.Serializable]
    public class ErrorCode
    {
        public int val;
    }

    [System.Serializable]
    public class ROSTransform // Renamed from Transform to avoid conflicts
    {
        public Position translation;
        public Orientation rotation;
    }

    [System.Serializable]
    public class Twist
    {
        public Position linear;
        public Position angular;
    }

    [System.Serializable]
    public class Wrench
    {
        public Position force;
        public Position torque;
    }
}