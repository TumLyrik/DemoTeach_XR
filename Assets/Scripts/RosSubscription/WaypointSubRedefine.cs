using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.TeachinUnity;
using System.Collections.Generic;

public class WaypointSubRedefine : UnitySubscriber<PoseStampedSimpleArray>
{
    public GameObject markerPrefab;
    public Transform markerParent;
    public float scale = 0.02f;
    public LineRenderer lineRenderer;

    private PoseStampedSimpleArray latestMessage = null;
    private bool isMessageReceived = false;

    private List<Vector3> pathPositions = new List<Vector3>();
    private List<Quaternion> pathRotations = new List<Quaternion>();
    private int currentIndex = 0;
    private bool isPlaying = false;

    public Transform movingObject; // 拖入你想播放动画的物体
    public float moveSpeed = 0.2f;
    public float rotSpeed = 180f;

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

        if (isPlaying && pathPositions.Count > 0 && movingObject != null)
        {
            Vector3 targetPos = pathPositions[currentIndex];
            Quaternion targetRot = pathRotations[currentIndex];

            // 移动位置
            movingObject.position = Vector3.MoveTowards(movingObject.position, targetPos, moveSpeed * Time.deltaTime);

            // 旋转朝向
            movingObject.rotation = Quaternion.RotateTowards(movingObject.rotation, targetRot, rotSpeed * Time.deltaTime);

            // 是否接近目标点
            if (Vector3.Distance(movingObject.position, targetPos) < 0.01f)
            {
                currentIndex++;
                if (currentIndex >= pathPositions.Count)
                {
                    currentIndex = 0; // 回到起点（或改成 isPlaying = false; 只播放一轮）
                }
            }
        }
    }

    private void VisualizeWaypoints(PoseStampedSimpleArray message)
    {
        if (message.poses == null)
        {
            Debug.LogWarning("Received null poses array.");
            return;
        }

        Debug.Log($"Visualizing {message.poses.Length} waypoints");
        lineRenderer.positionCount = message.poses.Length;

        foreach (Transform child in markerParent)
            Destroy(child.gameObject);  // 清除旧的

        var i = 0;
        pathPositions.Clear();
        pathRotations.Clear();
        currentIndex = 0;

        foreach (var pose in message.poses)
        {
            var p = pose.position;
            var q = pose.orientation;

            Vector3 position = new Vector3(-(float)p.x, (float)p.z, -(float)p.y);
            Quaternion rotation = new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);

            // marker relativ to the world central coordination system
            //GameObject marker = Instantiate(markerPrefab, position, rotation, markerParent);
            //marker.transform.localScale = Vector3.one * scale;

            lineRenderer.SetPosition(i, position);
            i++;

            pathPositions.Add(position);
            pathRotations.Add(rotation);
            //Debug.Log($"Waypoint: Pos({position}), Rot({rotation})");
        }

        // 将 movingObject 移动到第一个路径点
        if (pathPositions.Count > 0 && movingObject != null)
        {
            movingObject.position = pathPositions[0];
            movingObject.rotation = pathRotations[0];
            currentIndex = 1; // 下一帧将从第2个点开始
        }

        isPlaying = true;
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