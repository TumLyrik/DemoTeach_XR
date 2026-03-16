using RosSharp.RosBridgeClient;
using UnityEngine;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient
{
    public class CuttingStatusSubscriber : UnitySubscriber<std_msgs.Bool>
    {
        public static bool isCuttingActive = false;

        protected override void ReceiveMessage(std_msgs.Bool message)
        {
            isCuttingActive = message.data;
            Debug.Log($"[Unity] 切割状态更新: {isCuttingActive}");

            // 如果切割结束 (False)，清除 upper parts
            if (!isCuttingActive)
            {
                Debug.Log("[Unity] 切割结束，移除 Upper Parts");
            }
        }
    }
}
