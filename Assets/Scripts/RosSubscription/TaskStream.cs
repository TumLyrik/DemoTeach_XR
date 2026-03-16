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

public class TaskStream : UnityPublisher<PoseStampedSimpleArray>
{

    private string dataFolder = "Assets/Daten";
    private PoseStampedSimpleArray trajectoryMessage;
    public GameObject TaskTeach;
    private bool isWaiting = false;

    //public double posx;
    //public double posy;
    //public double posz;
    //public double quax;
    //public double quay;
    //public double quaz;
    //public double quaw;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        TriggerSignal trigger = TaskTeach.GetComponent<TriggerSignal>();
        if (trigger.RosStream) {
            LoadAndPublishTrajectories();
            StartCoroutine(ResetTriggerAfterDelay(trigger, 3f)); // 启动协程延时3秒
        }
    }

    private IEnumerator ResetTriggerAfterDelay(TriggerSignal trigger, float delay)
    {
        isWaiting = true;
        yield return new WaitForSeconds(delay);  // 等待3秒
        trigger.RosStream = false;
        isWaiting = false;
    }

    private void LoadAndPublishTrajectories()
    {
        List<PoseStampedSimple> allPoses = new List<PoseStampedSimple>();
        int order = 0;
        string[] files = Directory.GetFiles(dataFolder, "*.txt");
        if (files.Length == 0)
        {
            Debug.LogError($"Waypoint file not found at: {dataFolder}");
            return;
        }

        foreach (string file in files)
        {
            order++;
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] tokens = line.Split(';');
                if (tokens.Length != 8)
                {
                    Debug.LogWarning("Invalid line format: " + line);
                    continue;
                }

                try
                {
                    string removelist = tokens[0];
                    double x = double.Parse(tokens[1], CultureInfo.InvariantCulture);
                    double y = double.Parse(tokens[2], CultureInfo.InvariantCulture);
                    double z = double.Parse(tokens[3], CultureInfo.InvariantCulture);
                    double qx = double.Parse(tokens[4], CultureInfo.InvariantCulture);
                    double qy = double.Parse(tokens[5], CultureInfo.InvariantCulture);
                    double qz = double.Parse(tokens[6], CultureInfo.InvariantCulture);
                    double qw = double.Parse(tokens[7], CultureInfo.InvariantCulture);

                    Point pos = new Point { x = x, y = y, z = z };
                    RosQuaternion rosQuat = new RosQuaternion
                    {
                        x = qx,
                        y = qy,
                        z = qz,
                        w = qw
                    };

                    //posx = x;
                    //posy = y;
                    //posz = z;
                    //quax = qx;  
                    //quay = qy;
                    //quaz = qz;
                    //quaw = qw;

                    allPoses.Add(new PoseStampedSimple(removelist,order,pos,rosQuat));
                }
                catch
                {
                    Debug.LogWarning("Failed to parse line");
                }
            }
        }

        trajectoryMessage = new PoseStampedSimpleArray
        {
            poses = allPoses.ToArray()
        };

        PoseStampedSimpleArray msg = new PoseStampedSimpleArray(allPoses.ToArray());
        Publish(msg);
        Debug.Log($"Published trajectory with {allPoses.Count} poses.");
    }
}
