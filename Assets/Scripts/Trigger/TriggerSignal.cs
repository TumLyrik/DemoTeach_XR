using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using System.Globalization;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;
using RosSharp.RosBridgeClient;

public class TrailSegment
{
    public string id;
    public GameObject gameObject;
    public TrailRenderer trail;

    public TrailSegment(string id, GameObject go)
    {
        this.id = id;
        this.gameObject = go;
        this.trail = go.GetComponent<TrailRenderer>();
    }

    public void SetEmitting(bool state) => trail.emitting = state;

    public void Clear() => trail.Clear();

    public void Destroy(float delay = 0f)
    {
        GameObject.Destroy(gameObject, delay);
    }
}

[Serializable]
public class TrajectoryPoint
{
    public Vector3 position;
    public Quaternion rotation;

    public TrajectoryPoint(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}

public class TriggerSignal : MonoBehaviour
{
    public bool PortConnect;
    public string mode;
    public string portName = "COM7";  // Set the port name here
    public string trigger = "4";
    //public GameObject cam;
    public GameObject Slicing;
    public GameObject trailParent;
    public GameObject trailPrefab;
    public int trailID = 0;
    public bool recordTrigger = false;
    private int triggerState = 0;
    public bool RosStream = false;
    private List<int> Tasklist = new List<int>();

    private CastingSlicing sliceUnit;
    private string removedID;

    private float lastTriggerTime = 0f;
    private float triggerInterval = 0.01f; // 2Hz = every 0.5 seconds

    private List<TrajectoryPoint> currentTrajectory = new List<TrajectoryPoint>();
    private Dictionary<int, List<TrajectoryPoint>> trajectoryDict = new Dictionary<int, List<TrajectoryPoint>>();
    private Dictionary<string, TrailSegment> trails = new Dictionary<string, TrailSegment>();
    
    private SerialPort serialPort;
    private int baudRate = 9600;       // Set the baud rate here
    
    private int lastIndex = 0;
    private string savePath = "Assets/Daten";
    private CastingSlicing collidbody;
    private TFTransfer TFClient;

    // Start is called before the first frame update
    void Start()
    {
        sliceUnit = Slicing.GetComponent<CastingSlicing>();
        TFClient = FindObjectOfType<TFTransfer>();
        // Initialize and open the serial port with specified settings
        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = 1000, // Optional timeout to prevent blocking
            DtrEnable = true    // Optional: Enables the Data Terminal Ready signal if needed
        };
        if (PortConnect)
        {
            try
            {
                serialPort.Open();
                Debug.Log("Serial port opened");
                ClearTrajectoryFolder();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error opening serial port: {e.Message}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.time - lastTriggerTime < triggerInterval)
        //    return; // Skip if not yet 0.5s since last read

        //lastTriggerTime = Time.time; // Update time marker

        // Check if the serial port is open and has data available
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // Read all available data without blocking
                string incomingData = serialPort.ReadExisting();

                // Process only if data is received
                if (!string.IsNullOrEmpty(incomingData))
                {
                    incomingData = incomingData.Trim(); // Remove any whitespace

                    // Handle specific values if they match "1", "2", or "3"
                    foreach (char c in incomingData)
                    {
                        trigger = c.ToString();
                        if (trigger == "5")
                        {
                            recordTrigger = true;
                            triggerState = 5;
                            RosStream = false;
                            AddTrajectoryPoint();
                            if (trailID == lastIndex) {
                                trailID++;
                                string trailstring = "trail_" + trailID.ToString();
                                CreateTrail(trailstring);
                                SetEmitting(trailstring, true);
                            }
                        }
                        else if (trigger == "6")
                        {
                            recordTrigger = false;
                            triggerState = 6;
                        }
                        else if (trigger == "7")
                        {
                            recordTrigger = false;
                            triggerState = 7;
                            RosStream = true;
                        }
                        else //no trigger, triggerState -> 4
                        {
                            if (triggerState == 5)
                            {
                                triggerState = 4;
                                SetEmitting("trail_" + trailID.ToString(), false);
                                SaveCurrentTrajectory(); // save to file
                            }
                            else if (triggerState == 6)
                            {
                                DeleteLastTrajectory(); // delete from file
                                triggerState = 4;
                                DestroyTrail("trail_" + trailID.ToString());
                            }
                            else // triggerState == "4" or triggerState == "7"
                            {
                                recordTrigger = false;
                                triggerState = 4;
                                currentTrajectory.Clear();
                                //RosStream = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading from serial port: {e.Message}");
            }
        }

        // only for test through keyboard input
        if (!PortConnect) 
        {
            if (trigger == "5")
            {
                recordTrigger = true;
                triggerState = 5;
                RosStream = false;
                AddTrajectoryPoint();
                if (trailID == lastIndex)
                {
                    trailID++;
                    string trailstring = "trail_" + trailID.ToString();
                    CreateTrail(trailstring);
                    SetEmitting(trailstring, true);
                }
            }
            else if (trigger == "6")
            {
                recordTrigger = false;
                triggerState = 6;
            }
            else if (trigger == "7")
            {
                recordTrigger = false;
                triggerState = 7;
                RosStream = true;
            }
            else //no trigger, triggerState -> 4
            {
                if (triggerState == 5)
                {
                    triggerState = 4;
                    SetEmitting("trail_" + trailID.ToString(), false);
                    SaveCurrentTrajectory(); // save to file
                }
                else if (triggerState == 6)
                {
                    DeleteLastTrajectory(); // delete from file
                    triggerState = 4;
                    DestroyTrail("trail_" + trailID.ToString());
                }
                else // triggerState == "4" or triggerState == "7"
                {
                    recordTrigger = false;
                    triggerState = 4;
                    currentTrajectory.Clear();
                    //RosStream = false;
                }
            }
        }
    }

    void AddTrajectoryPoint()
    {
        //// Matrix4x4 tf = this.transform.worldToLocalMatrix;
        //Vector3 lh_pos = transform.localPosition;
        //Quaternion lh_quat = transform.localRotation;
        //// Left-handed to right-handed conversion
        //Quaternion unityToReal = Quaternion.LookRotation(
        //    new Vector3(0, 0, 1), //在Forward方向上Unity要映射cam的Z
        //    new Vector3(0, 1, 0) //在Up方向上Unity要映射到cam的Y
        //    );
        //Quaternion rh_quat = unityToReal * lh_quat * Quaternion.Inverse(unityToReal);
        //Vector3 rh_pos = new Vector3(-lh_pos.x, lh_pos.y, lh_pos.z);

        Vector3 ee_pos = new Vector3(TFClient.px, TFClient.py, TFClient.pz); //flange pos
        Quaternion ee_quat = new Quaternion(TFClient.qx, TFClient.qy, TFClient.qz, TFClient.qw); //flange quaternion
        currentTrajectory.Add(new TrajectoryPoint(ee_pos, ee_quat));
    }

    void SaveCurrentTrajectory()
    {
        lastIndex++;
        Tasklist.Add(lastIndex);
        trajectoryDict[lastIndex] = new List<TrajectoryPoint>(currentTrajectory);


        if (sliceUnit != null && sliceUnit.removedlist != null && sliceUnit.removedlist.Count > 0)
        {
            removedID = string.Join(", ", sliceUnit.removedlist);
        }
        else
        {
            removedID = "0";
        }

        //Debug.Log("List as string: " + removedID);

        string filePath = Path.Combine(savePath, $"trajectory_{lastIndex}.txt");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var point in currentTrajectory)
            {
                writer.WriteLine(
                    removedID + ";" +
                    point.position.x.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.position.y.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.position.z.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.rotation.x.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.rotation.y.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.rotation.z.ToString(CultureInfo.InvariantCulture) + ";" +
                    point.rotation.w.ToString(CultureInfo.InvariantCulture)
                );
            }
        }

        Debug.Log($"Saved trajectory #{lastIndex} with {currentTrajectory.Count} points.");
    }

    void DeleteLastTrajectory()
    {
        if (trajectoryDict.ContainsKey(lastIndex))
        {
            trajectoryDict.Remove(lastIndex);
            string filePath = Path.Combine(savePath, $"trajectory_{lastIndex}.txt");
            if (File.Exists(filePath))
                File.Delete(filePath);

            Debug.Log($"Deleted trajectory #{lastIndex}");
            Tasklist.Remove(lastIndex);
            lastIndex--;
        }
        else
        {
            Debug.LogWarning("No trajectory to delete.");
        }
    }

    void ClearTrajectoryFolder()
    {
        if (Directory.Exists(savePath))
        {
            string[] files = Directory.GetFiles(savePath, "*.txt");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to delete {file}: {e.Message}");
                }
            }
        }
    }

    public void CreateTrail(string id)
    {
        if (trails.ContainsKey(id)) return;

        GameObject go = Instantiate(trailPrefab, trailParent.transform.position, trailParent.transform.rotation);
        go.transform.SetParent(trailParent.transform);
        go.name = id;
        var segment = new TrailSegment(id, go);
        trails.Add(id, segment);
    }

    public void SetEmitting(string id, bool state)
    {
        if (trails.TryGetValue(id, out var segment))
            segment.SetEmitting(state);
    }

    public void ClearTrail(string id)
    {
        if (trails.TryGetValue(id, out var segment))
            segment.Clear();
    }

    public void DestroyTrail(string id)
    {
        if (trails.TryGetValue(id, out var segment))
        {
            Destroy(segment.gameObject);
            trails.Remove(id);
            Debug.Log("Destroy trail: " + id);
            trailID--;
        }
    }
}
