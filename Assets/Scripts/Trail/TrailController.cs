/*
 * 接收triggerbox的信号以决定合适开始记录和停止记录
 * roslaunch_cmd用于告知其他程序，停止记录并给rosconnector信号
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO.Ports;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.UI;

public class TrailController : MonoBehaviour
{
    //setting for rendering
    GameObject trail1;
    GameObject trail2;
    TrailRenderer trailRenderer1;
    TrailRenderer trailRenderer2;
  
    //setting for global parameter
    public static int roslaunch_cmd = 0;

    void Start()
    {
        trail1 = GameObject.Find("trail1");
        trailRenderer1 = trail1.GetComponent<TrailRenderer>();
        trail2 = GameObject.Find("trail2");
        trailRenderer2 = trail2.GetComponent<TrailRenderer>();
        trailRenderer1.emitting = false;
        trailRenderer2.emitting = false;
    }

    void Update() 
    {
        trailRenderer1.emitting = true;
        trailRenderer1.Clear(); 
        trailRenderer1.emitting = false;
    }

    void OnDestroy()
    {

    }
}


/* 2.method: communicate with python through keyboard


using System.Net.Sockets; // Socket to python
string trigger;
[HideInInspector] public bool isTxStarted = false;
[SerializeField] string IP = "127.0.0.1"; // local host
[SerializeField] int rxPort = 8000; // port to receive data from Python
[SerializeField] int txPort = 8001; // port to send data to Python
UdpClient client; // Create necessary UdpClient objects
Thread receiveThread; // Receiving Thread

// IPEndPoint remoteEndPoint; // remoteEndPoint to MatLab
// StartStopRecording();
// Create remote endpoint (to Matlab) 
// remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);
// Create local client

//rceive data, update packets received 
private void ReceiveData()
{
    while (true)
    {
        try
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref anyIP);
            string text = Encoding.UTF8.GetString(data);
            print(">> " + text);
            trigger = text;
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
}

client = new UdpClient(rxPort);
// local endpoint define (where messages are received)
// Create a new thread for reception of incoming messages
receiveThread = new Thread(new ThreadStart(ReceiveData));
receiveThread.IsBackground = true;
receiveThread.Start();
// Initialize (seen in comments window)
print("UDP Comms Initialised");

public void ClearTrail()
{
    Debug.Log("Cleared trail!");
    //trailRenderer.Clear();
}
//Prevent crashes - close clients and threads properly!
void OnDisable()
{
    //if (receiveThread != null)
    //  receiveThread.Abort();
    //client.Close();
}
void OnDestroy()
{
    // Close the serial port when the script is destroyed
    if (serialPort != null && serialPort.IsOpen)
    {
        serialPort.Close();
        Debug.Log("Serial port closed");
    }
}
*/

/* 3.method:python端将命令输出到剪切板，unity从剪切板读取值
string clipboard = GUIUtility.systemCopyBuffer;
if (clipboard == "start")
{
    trailRenderer.emitting = true;
}
*/