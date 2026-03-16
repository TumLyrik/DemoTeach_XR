using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.VersionControl;

namespace RosSharp.RosBridgeClient
{
    public class EEPoseSubscriber : UnitySubscriber<PoseStamped>
    {
        //public UnityEngine.Transform eeTransform;
        public float rosPosX;
        public float rosPosY;
        public float rosPosZ;
        
        public float rosOriX;
        public float rosOriY;
        public float rosOriZ;
        public float rosOriW;

        protected override void ReceiveMessage(PoseStamped message)
        {
            rosPosX = (float)message.pose.position.x;
            rosPosY = (float)message.pose.position.y;
            rosPosZ = (float)message.pose.position.z;

            rosOriX = (float)message.pose.orientation.x;
            rosOriY = (float)message.pose.orientation.y;
            rosOriZ = (float)message.pose.orientation.z;
            rosOriW = (float)message.pose.orientation.w;

            //Debug.Log($"pos x: {rosPosX}, pos y: {rosPosY}, pos z: {rosPosZ}, ori x: {rosOriX}, ori y: {rosOriY}, ori z: {rosOriZ}, ori w: {rosOriW}");
        }

        private UnityEngine.Quaternion RosToUnityRotation(UnityEngine.Quaternion rosQuaternion)
        {
            return new UnityEngine.Quaternion(-rosQuaternion.x, -rosQuaternion.z, -rosQuaternion.y, rosQuaternion.w);
        }
    }
}
