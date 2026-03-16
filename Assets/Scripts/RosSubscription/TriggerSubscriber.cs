using RosSharp.RosBridgeClient;
using UnityEngine;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient
{
    public class TriggerSubscriber : UnitySubscriber<std_msgs.Int32>
    {
        public int triggerStatus = 0;

        protected override void ReceiveMessage(std_msgs.Int32 message)
        {
            triggerStatus = message.data;
            // Debug.Log($"[Unity] trigger signal: {triggerStatus}");
        }
    }
}
