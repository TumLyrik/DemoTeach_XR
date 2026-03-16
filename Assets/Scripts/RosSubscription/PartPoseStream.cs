using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RosSharp.RosBridgeClient.MessageTypes.TeachinUnity;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using RosQuaternion = RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion;
using System.Globalization;
using UnityEngine;

public class PartPoseStream : UnityPublisher<PoseStampedSimpleArray>
{
    public GameObject TaskTeach;
    public GameObject part;
    public UnityEngine.Transform rosRoot;
    private PoseStampedSimpleArray poseArrayMsg;
    private bool isWaiting = false;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        TriggerSignal trigger = TaskTeach.GetComponent<TriggerSignal>();
        if (trigger.RosStream) {
            PublishPartPose();
            //StartCoroutine(ResetTriggerAfterDelay(trigger, 3f)); // 
        }
    }

    private IEnumerator ResetTriggerAfterDelay(TriggerSignal trigger, float delay)
    {
        isWaiting = true;
        yield return new WaitForSeconds(delay);  // 
        trigger.RosStream = false;
        isWaiting = false;
    }

    public static class UnityToRos
    {
        // This function maps Unity pose to ROS pose (right-handed)
        public static void ConvertTransform(UnityEngine.Transform tf, UnityEngine.Transform rosRoot, out UnityEngine.Vector3 rosPos, out UnityEngine.Quaternion rosQuat)
        {
            UnityEngine.Vector3 localPos = rosRoot.InverseTransformPoint(tf.position);
            UnityEngine.Quaternion localRot = UnityEngine.Quaternion.Inverse(rosRoot.rotation) * tf.rotation;
            // ========= 1) Position =========
            float ux = localPos.x;
            float uy = localPos.y;
            float uz = localPos.z;

            // Apply position mapping
            rosPos = new UnityEngine.Vector3(
                uz,    // +unity.z → +ros.x
                -ux,   // +unity.x → -ros.y
                uy     // +unity.y → +ros.z
            );

            // ----- 2) Rotation mapping using matrix conjugation
            // Get Unity rotation matrix (left-handed)
            Matrix4x4 R_u = Matrix4x4.Rotate(localRot);

            // Build mapping S (fixed)
            Matrix4x4 S = new Matrix4x4();
            S.SetRow(0, new Vector4(0, 0, 1, 0));
            S.SetRow(1, new Vector4(-1, 0, 0, 0));
            S.SetRow(2, new Vector4(0, 1, 0, 0));
            S.SetRow(3, new Vector4(0, 0, 0, 1)); // Homogeneous

            Matrix4x4 S_T = S.transpose;

            // Compute right-handed ROS rotation matrix
            Matrix4x4 R_r = S * R_u * S_T;

            // Convert back to quaternion (correct handedness)
            rosQuat = QuaternionFromMatrix(R_r);
            rosQuat.Normalize();

            //Debug.Log($"[UNITY] POS (LHS) = {p}");
            //Debug.Log($"[UNITY] QUAT (LHS x,y,z,w) = {q.x:F6}, {q.y:F6}, {q.z:F6}, {q.w:F6}");
            //Debug.Log($"[UNITY] EULER (deg) = {q.eulerAngles}");

            // -------------------------------
            // 4) 输出转换后 ROS 四元数
            // -------------------------------
            Debug.Log($"[ROS] POS = {rosPos}");
            Debug.Log($"[ROS] QUAT (x,y,z,w) = {rosQuat.x:F6}, {rosQuat.y:F6}, {rosQuat.z:F6}, {rosQuat.w:F6}");
            Debug.Log($"[ROS] EULER (deg) = {rosQuat.eulerAngles}");
        }

        // Extract quaternion from 3x3 rotation matrix
        // (Unity's built-in Matrix4x4.rotation is not bidirectional)
        public static UnityEngine.Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            return UnityEngine.Quaternion.LookRotation(
                new UnityEngine.Vector3(m[0, 2], m[1, 2], m[2, 2]),
                new UnityEngine.Vector3(m[0, 1], m[1, 1], m[2, 1])
            );
        }
    }

    private void PublishPartPose()
    {
        if (part == null)
        {
            Debug.LogError("Part GameObject is not assigned.");
            return;
        }

        UnityEngine.Transform tf = part.transform;

        // Call conversion
        UnityToRos.ConvertTransform(tf, rosRoot, out UnityEngine.Vector3 rosPos, out UnityEngine.Quaternion rosQuat);

        // Construct pose (single)
        var poseEntry = new PoseStampedSimple
        {
            position = new Point { x = rosPos.x, y = rosPos.y, z = rosPos.z },
            orientation = new RosQuaternion { x = rosQuat.x, y = rosQuat.y, z = rosQuat.z, w = rosQuat.w }
        };

        // Wrap in array (required by ROS msg type)
        var poseArrayMsg = new PoseStampedSimpleArray
        {
            poses = new PoseStampedSimple[] { poseEntry }  // Wrapper array
        };

        // Publish array message
        Publish(poseArrayMsg);
    }
}
