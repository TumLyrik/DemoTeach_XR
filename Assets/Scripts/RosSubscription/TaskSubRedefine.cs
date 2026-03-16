using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.TeachinUnity;
using System.Collections.Generic;

public class TaskSubRedefine : UnitySubscriber<PoseStampedSimpleArray>
{
    public GameObject markerPrefab;
    public Transform markerParent;
    public float scale = 0.02f;

    private PoseStampedSimpleArray latestMessage = null;
    private bool isMessageReceived = false;

    protected override void Start()
    {
        base.Start();
        Debug.Log("PoseStampedSimpleArraySubscriber started.");
    }

    protected override void ReceiveMessage(PoseStampedSimpleArray message)
    {
        latestMessage = message;
        isMessageReceived = true;
    }

    private void Update()
    {
        if (isMessageReceived)
        {
            VisualizeWaypoints(latestMessage);
            isMessageReceived = false;
        }
    }

    private void VisualizeWaypoints(PoseStampedSimpleArray message)
    {
        if (message.poses == null)
        {
            Debug.LogWarning("Received null poses array.");
            return;
        }

        Debug.Log($"Visualizing {message.poses.Length} taskpoints");

        foreach (Transform child in markerParent)
            Destroy(child.gameObject);  // 清除旧的

        foreach (var pose in message.poses)
        {
            var p = pose.position;
            var q = pose.orientation;

            Vector3 position = new Vector3(-(float)p.x, (float)p.z, -(float)p.y);
            Quaternion rotation = new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);

            // marker relativ to the world central coordination system
            GameObject marker = Instantiate(markerPrefab, position, rotation, markerParent); 
            marker.transform.localScale = Vector3.one * scale;

            //Debug.Log($"Waypoint: Pos({position}), Rot({rotation})");
        }
    }
}

//using UnityEngine;
//using RosSharp.RosBridgeClient;
//using RosSharp.RosBridgeClient.MessageTypes.RosMessages;
//using static UnityEngine.Rendering.DebugUI.Table;

//public class PoseStampedSimpleArraySubscriber : UnitySubscriber<PoseStampedSimpleArray>
//{

//    public GameObject markerPrefab;  // 拖一个Sphere或Arrow Prefab到 Inspector
//    public Transform markerParent;   // 方便统一管理/清除
//    public float scale = 0.02f;

//    protected override void Start()
//    {
//        base.Start();  // 确保 ROS# 正确注册订阅器
//        Debug.Log("PoseStampedSimpleArraySubscriber started.");
//    }

//    protected override void ReceiveMessage(PoseStampedSimpleArray message)
//    {
//        if (message.poses == null)
//        {
//            Debug.LogWarning("Received null poses array.");
//            return;
//        }

//        Debug.Log($"Received waypoint array. Count: {message.poses.Length}");

//        foreach (Transform child in markerParent)
//        {
//            Destroy(child.gameObject);  // 清除旧的
//        }

//        foreach (var pose in message.poses)
//        {
//            var p = pose.position;
//            var q = pose.orientation;

//            Vector3 position = new Vector3((float)p.x, (float)p.y, (float)p.z);
//            Quaternion rotation = new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);

//            GameObject marker = Instantiate(markerPrefab, position, rotation, markerParent);
//            marker.transform.localScale = Vector3.one * scale;

//            Debug.Log($"Waypoint: Pos({position}), Rot({rotation})");
//        }
//    }
//}