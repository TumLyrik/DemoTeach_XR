/* 
 * 将相机tracking的源数据写入trj.csv文件之中
 * 
*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using RosSharp.RosBridgeClient;
using System.Globalization;


/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class OptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public Int32 RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;

    //Rui:file for record
    //public GameObject cube;
    private TrailController TrailController;
    //private List<string> trajectoryData;
    //public string fileName = "trj.csv";
    //private StreamWriter writer;
    //private int recordID = 1; // Initialize the ID counter
    private UpdateRobotPose RobotConnect;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterRigidBody( this, RigidBodyId );

        //Rui:Add headers
        //trajectoryData = new List<string>();
        //trajectoryData.Add("Record,ID,PosX,PosY,PosZ,RotW,RotX,RotY,RotZ");
        //string path = "C:/unity-ar/Files/" + fileName;
        //writer = new StreamWriter(path, false); // false表示清空内容
        RobotConnect = FindObjectOfType<UpdateRobotPose>();
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void Update()
    {
        UpdatePose();
    }


    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId, NetworkCompensation);
        if ( rbState != null && !RobotConnect.rivz_connect)
        {
            //Rui:Format data for CSV
            //TrailController = cube.GetComponent<TrailController>();
            Vector3 position_disk;
            Quaternion rotation_disk;
            position_disk.x = rbState.Pose.Position[0];
            position_disk.y = rbState.Pose.Position[1];
            position_disk.z = rbState.Pose.Position[2];
            rotation_disk.x = rbState.Pose.Orientation[0];
            rotation_disk.y = rbState.Pose.Orientation[1];
            rotation_disk.z = rbState.Pose.Orientation[2];
            rotation_disk.w = rbState.Pose.Orientation[3];

            //this.transform.localPosition = rbState.Pose.Position;
            //this.transform.localRotation = rbState.Pose.Orientation;
            this.transform.localPosition = position_disk;
            this.transform.localRotation = rotation_disk;

            //mode A
            /*
            if (0 < TrailController.recordtrigger && TrailController.recordtrigger < 3)
            {
                Vector3 position = rbState.Pose.Position;
                Quaternion rotation = rbState.Pose.Orientation;
                string data = $"{recordID},{TrailController.recordtrigger}," +
                              $"{position.x.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{position.y.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{position.z.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.w.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.x.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.y.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.z.ToString("F4", CultureInfo.InvariantCulture)}";
                //trajectoryData.Add(data);
                writer.WriteLine(data);
                recordID++;
            }
            */
            // Simple version
            /*
            if (TrailController.recordtrigger == 1)
            {
                Vector3 position = rbState.Pose.Position;
                Quaternion rotation = rbState.Pose.Orientation;
                string data = $"{recordID},{TrailController.recordtrigger}," +
                              $"{position.x.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{position.y.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{position.z.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.w.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.x.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.y.ToString("F4", CultureInfo.InvariantCulture)}," +
                              $"{rotation.z.ToString("F4", CultureInfo.InvariantCulture)}";
                //trajectoryData.Add(data);
                writer.WriteLine(data);
                recordID++;
            }
            if (TrailController.roslaunch_cmd == 1)
            {
                writer.Close();
            }*/
        }
    }

    /*
    void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
            Debug.Log("CSV file closed.");
        }
        //SaveToFile();
    }
    
    private void SaveToFile()
    {
        //string path = Path.Combine(Application.persistentDataPath, fileName);
        string path = "C:/unity-ar/Files/" + fileName;

        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (string line in trajectoryData)
            {
                writer.WriteLine(line);
            }
        }

        Debug.Log($"Trajectory data saved to {path}");
    }
    */
}
