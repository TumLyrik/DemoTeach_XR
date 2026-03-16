using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using System.IO.Ports;

public class PrerecordedTrailRenderer : MonoBehaviour
{
    public TrailRenderer trailRenderer;

    private string filePath;
    private string[][] dataArray;

    private Vector3 originalPosition;
    private Vector3 position;
    private Quaternion originalRotation;
    private Quaternion rotation;

    void Start()
    {
        //"/StreamingAssets/trajectory-data.csv"
        filePath = "C:/Users/Admin/AppData/LocalLow/DefaultCompany/SXR_AR_HDRP/trj.csv";
        var data  = File.ReadAllLines(filePath).Select(x => x.Split('\n')).ToArray();
        dataArray = data.Skip(1500).ToArray();

        originalPosition = this.gameObject.transform.localPosition;
        originalRotation = this.gameObject.transform.localRotation;
        trailRenderer.emitting = false;
        // StartRendering();

    }

    void Update() 
    {
        // Start the trail
        if (Input.GetKey(KeyCode.O))
        {
            StartRendering();
        }
        // Stop and clear the trail
        if (Input.GetKey(KeyCode.P))
        {
            StopRendering();
            ClearTrail();
        }
        string clipboard = GUIUtility.systemCopyBuffer;
       
        // Stop and clear the trail
        if (Input.GetKey(KeyCode.P))
        {
            StopRendering();
            ClearTrail();
        }
    }

    public void StartRendering()
    {
        StartCoroutine(StartTrail());
    }

    IEnumerator StartTrail()
    {
        Debug.Log("Rendering prerecorded data");

        //var data  = File.ReadAllLines(filePath).Select(x => x.Split('\n')).ToArray();
        //var data = File.ReadLines(filePath).Select(x => x.Split('\n')).ToArray();

        // Debug.Log(data[1][0]);
        // Debug.Log(data[2][0]);

        trailRenderer.emitting = true;

        foreach (string[] line in dataArray)
        {
            // split the items
            string[] sArray = line[0].Split(',');

            // Position
            position = new Vector3(
                float.Parse(sArray[3]),
                float.Parse(sArray[4]),
                float.Parse(sArray[5]));

            // Rotation
            rotation = new Quaternion(
                float.Parse(sArray[7]),
                float.Parse(sArray[8]),
                float.Parse(sArray[9]),
                float.Parse(sArray[6]));

            this.gameObject.transform.localPosition = position;
            this.gameObject.transform.localRotation = rotation;

            yield return new WaitForSeconds(.0000000001f);
        }

        trailRenderer.emitting = false;
    }

    public void StopRendering()
    {
        StopAllCoroutines();
        trailRenderer.emitting = false;
        //ClearTrail();
        this.gameObject.transform.localPosition = originalPosition;
        this.gameObject.transform.localRotation = originalRotation;
    }

    public void ClearTrail()
    {
        Debug.Log("Cleared trail!");
        trailRenderer.Clear();
    }
}
