using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class JointStateSubscriberLBR : UnitySubscriber<MessageTypes.Sensor.JointState>
    {
        public Dictionary<string, float> JointAngles = new Dictionary<string, float>();

        protected override void ReceiveMessage(MessageTypes.Sensor.JointState message)
        {
            //print($"Received {message.name.Length} joint names and {message.position.Length} positions.");

            for (int i = 0; i < message.name.Length; i++)
            {
                string jointName = message.name[i];
                float angleInDegrees = (float)message.position[i] * Mathf.Rad2Deg;

                if (JointAngles.ContainsKey(jointName))
                {
                    JointAngles[jointName] = angleInDegrees;
                }
                else
                {
                    JointAngles.Add(jointName, angleInDegrees);
                }
            }

            //foreach (var joint in JointAngles)
            //{
            //    print($"{joint.Key}: {joint.Value}?);
            //}
        }

        public float GetJointAngle(string jointName)
        {
            if (JointAngles.ContainsKey(jointName))
            {
                return JointAngles[jointName];
            }
            else
            {
                return 0f;
            }
        }
    }
}