using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Visualization;
using RosSharp.RosBridgeClient.MessageTypes.RosMessages;

public class TaskSub : UnitySubscriber<MarkerArray>
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void ReceiveMessage(MarkerArray message)
    {
        Debug.Log("Received MarkerArray with " + message.markers.Length + " markers.");

        foreach (var marker in message.markers)
        {
            Vector3 pos = new Vector3(
                (float)marker.pose.position.x,
                (float)marker.pose.position.y,
                (float)marker.pose.position.z
            );
            Debug.Log($"Marker {marker.id} at position: {pos}");
        }
    }
}
