using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RDA;
using UnityEngine.VFX;
using System;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;

public class VFXPointcloud : MonoBehaviour
{
    RDA.Client rda;
    string RDAModule;
    public VisualEffect vfx1, vfx2;
    public DepthCamera[] depthCameras;

    private object[] ArrayObjects = new object[] { new float[,] { }, "" };   // New for attribute from rda
    private int numAttributes = 7;
    private float Timestamp, HRTimestamp; // TODO?
    private VisualEffect selectedPointCloud;
    private bool debugging = false;

    byte[] rawdata;
    public Texture2D[] cameraImages;
    public Texture2D[] depthImages;
    byte[] colorRGB24;
    byte[] depthUINT16;


    public ComputeShader erodeShader;
    public RenderTexture eroded;

    public RawImage debugImage;
    public RawImage debugImage2;    
    public RawImage debugDepthImage;
    public RawImage debugDepthImage2;

    public GameObject debugCanvas;

    public struct DepthCamera
    {
        public DepthCamera(float[] attributes, int startIndex)
        {
            ImageWidth = (int)attributes[0 + startIndex]; 
            ImageHeight = (int)attributes[1 + startIndex];
            ColorChannels = (int)attributes[2 + startIndex];
            FX = attributes[3 + startIndex]; FY = attributes[4 + startIndex];
            CX = attributes[5 + startIndex]; CY = attributes[6 + startIndex];
        }

        public void Init(float[] attributes, int startIndex)
        {
            ImageWidth = (int)attributes[0 + startIndex];
            ImageHeight = (int)attributes[1 + startIndex];
            ColorChannels = (int)attributes[2 + startIndex];
            FX = attributes[3 + startIndex]; FY = attributes[4 + startIndex];
            CX = attributes[5 + startIndex]; CY = attributes[6 + startIndex];
        }

        public int ImageWidth, ImageHeight;
        public int ColorChannels;
        public float FX, FY;
        public float CX, CY;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Read RDA module name from file
        string rdaTextFilePath = Application.streamingAssetsPath + "/rda_module.txt";
        if (!File.Exists(rdaTextFilePath)) {
            File.WriteAllText(rdaTextFilePath, "");
        }
        string rdaModule = rdaModule = File.ReadAllText(rdaTextFilePath);
        if (!string.IsNullOrEmpty(rdaModule)) {
            RDAModule = rdaModule;
        }
        else {            
            Debug.Log("No RDA module name provided in the configuration file in StreamingAssets folder"); 
        }

        // Read positions of pointclouds from file        
        string pcPosTextFilePath = Application.streamingAssetsPath + "/pointcloud_positions.txt";
        if (!File.Exists(pcPosTextFilePath)) {
            WritePointcloudPositionsFile(pcPosTextFilePath);           
        }

        string[] pointcloudPositionsText = File.ReadAllLines(pcPosTextFilePath);
        // if (pointcloudPositionsText.Length == 6) {
        //     string[] position = pointcloudPositionsText[0].Split(' ');
        //     string[] eulerAngles = pointcloudPositionsText[1].Split(' ');
        //     string[] localScale = pointcloudPositionsText[2].Split(' ');
        //     vfx1.transform.position = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
        //     vfx1.transform.rotation = Quaternion.Euler(float.Parse(eulerAngles[0]), float.Parse(eulerAngles[1]), float.Parse(eulerAngles[2]));
        //     vfx1.transform.localScale = new Vector3(float.Parse(localScale[0]), float.Parse(localScale[1]), float.Parse(localScale[2]));

        //     position = pointcloudPositionsText[3].Split(' ');
        //     eulerAngles = pointcloudPositionsText[4].Split(' ');
        //     localScale = pointcloudPositionsText[5].Split(' ');
        //     vfx2.transform.position = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
        //     vfx2.transform.rotation = Quaternion.Euler(float.Parse(eulerAngles[0]), float.Parse(eulerAngles[1]), float.Parse(eulerAngles[2]));
        //     vfx2.transform.localScale = new Vector3(float.Parse(localScale[0]), float.Parse(localScale[1]), float.Parse(localScale[2]));
        // }
        // else {            
        //     Debug.Log("Unable to read pointcloud positions from file"); 
        // }

        // Try to connect to RDA
        try { rda = new RDA.Client(""); }
        catch (Exception e) 
        { 
            gameObject.SetActive(false); 
            Debug.Log("No RDA found: " + e.Message); 
            return; 
        }

        Debug.Log("Succesfully started RDA");

        cameraImages = new Texture2D[2];
        depthImages = new Texture2D[2];

        vfx1.enabled = true;
        vfx2.enabled = true;

        selectedPointCloud = vfx1;        
    }

    void Update()
    {
        // Get RawData with attributes
        ArrayObjects = rda.GetEx(RDAModule + ".output");
        string attributes = (string)ArrayObjects[1];

        ParseAttributes(attributes);

        // Get rawdata in an array
        rawdata = (byte[])ArrayObjects[0];
        int numBytes = depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * (depthCameras[0].ColorChannels * 2 + 4);

        // TODO: make dynamic for number of cameras
        // Camera 1
        int offset = 0;
        GetImage(rawdata, 0, offset);
        UpdateVFX(vfx1, cameraImages[0], depthImages[0]);
        // Camera 2
        offset = depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 3
            + depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 2;
        GetImage(rawdata, 1, offset);
        UpdateVFX(vfx2, cameraImages[1], depthImages[1]);

        // Debug images
        debugImage.texture = cameraImages[0];
        debugImage2.texture = cameraImages[1];
        debugDepthImage.texture = depthImages[0];
        debugDepthImage2.texture = depthImages[1];

        // User controls
        //SelectPointcloud();
        //MovePointclouds();
        //ScalePointClouds();
        
        // Reset both pointclouds
        if (Input.GetKeyUp(KeyCode.R)) {
            ResetPointclouds();
        }

        // Create objects to check if rendering works
        if (Input.GetKeyUp(KeyCode.C)) {
            if (debugging) {
                debugging = false;
            } else {
                debugging = true;
            }
            debugCanvas.SetActive(debugging);
        }
    }

    void ResetPointclouds() {
        vfx1.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        vfx1.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        vfx1.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        vfx2.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        vfx2.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        vfx2.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    void SelectPointcloud() {
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            selectedPointCloud = vfx1;
        }        
        if (Input.GetKeyUp(KeyCode.Alpha2)) {            
            selectedPointCloud = vfx2;
        }
    }

    void MovePointclouds() {
        // Move horizontally
        Vector3 horizontalForward = new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z);
        Vector3 horizontalRight = new Vector3(Camera.main.transform.right.x, 0.0f, Camera.main.transform.right.z);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            selectedPointCloud.transform.position += horizontalForward * .5f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            selectedPointCloud.transform.position -= horizontalForward * .5f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            selectedPointCloud.transform.position -= horizontalRight * .5f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            selectedPointCloud.transform.position += horizontalRight * .5f * Time.deltaTime;
        }

        // Move vertically and rotate
        if (Input.GetKey(KeyCode.W))
        {
            selectedPointCloud.transform.position += Vector3.up * .25f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            selectedPointCloud.transform.position -= Vector3.up * .25f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            selectedPointCloud.transform.Rotate(Vector3.up, 20f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {            
            selectedPointCloud.transform.Rotate(Vector3.up, -20f * Time.deltaTime);
        }
    }

    void ScalePointClouds() {
        if (Input.GetKey(KeyCode.Q))
        {
            selectedPointCloud.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            selectedPointCloud.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    /* RDA Attributes contain the following structure:
     * [timestamp] [High res timestamp] 
     * [image width] [image height] [number of channels]
     * [focal length X (FX)] [focal length Y (FY)] 
     * [central pixel X (CX)] [central pixel Y (CY)]
     * 
     * Note: CX and CY used to be PPX and PPY
     * Note 2: more cameras can be added at the end of the array
     */
    void ParseAttributes(string attributes)
    {
        float[] floatData;
        try 
        {
            floatData = Array.ConvertAll(attributes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), float.Parse);
        }
        catch (Exception e)
        {
            gameObject.SetActive(false); 
            Debug.LogWarning("Could not parse attributes: " + e.Message); 
            return;
        }

        // Get the timestamps for the frame
        Timestamp = floatData[0];
        HRTimestamp = floatData[1];

        Debug.Log(Timestamp);

        // Create camera struct per Kinect
        int numCameras = floatData.Length / numAttributes;
        //Debug.Log(numCameras);
        
        depthCameras = new DepthCamera[numCameras];
        for (int i = 0; i < numCameras; i++)
            depthCameras[i].Init(floatData, 2 + i * numAttributes);
    }

    void GetImage(byte[] rawdata, int camNo, int offset)
    {
        int start = offset;

        // Color Image
        if (cameraImages[camNo] == null)
            cameraImages[camNo] = new Texture2D(depthCameras[0].ImageWidth, depthCameras[0].ImageHeight,
            TextureFormat.RGB24, false, true);

        if (colorRGB24 == null || colorRGB24.Length != depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 3) {
            colorRGB24 = new byte[depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 3];
            // byte[] colorRGB24 = new byte[depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 3];
        }

        Buffer.BlockCopy(rawdata, offset, colorRGB24, 0, colorRGB24.Length);

        cameraImages[camNo].LoadRawTextureData(colorRGB24);
        cameraImages[camNo].Apply();

        // Do the same for depth image
        if (depthImages[camNo] == null)
            depthImages[camNo] = new Texture2D(depthCameras[0].ImageWidth, depthCameras[0].ImageHeight,
            TextureFormat.R16, false, true);
            depthImages[camNo].filterMode = FilterMode.Point;
            depthImages[camNo].wrapMode = TextureWrapMode.Clamp;

        if (depthUINT16 == null || depthUINT16.Length != depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 2) {
            // byte[] depthUINT16 = new byte[depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 2];
            depthUINT16 = new byte[depthCameras[0].ImageWidth * depthCameras[0].ImageHeight * 2];
        }

        start += colorRGB24.Length;
        Buffer.BlockCopy(rawdata, start, depthUINT16, 0, depthUINT16.Length);

        depthImages[camNo].LoadRawTextureData(depthUINT16);
        depthImages[camNo].Apply();
    }

    void UpdateVFX(VisualEffect vfx, Texture2D color, Texture2D depth)
    {
        vfx.Play();
        //Set shader parameters

        //Debug.Log(depthCameras[0].CX);

        vfx.SetUInt("DataWidth", (uint)depthCameras[0].ImageWidth);
        vfx.SetUInt("DataHeight", (uint)depthCameras[0].ImageHeight);
        vfx.SetFloat("CX", depthCameras[0].CX);
        vfx.SetFloat("CY", depthCameras[0].CY);
        vfx.SetFloat("Fx", depthCameras[0].FX);
        vfx.SetFloat("Fy", depthCameras[0].FY);

        vfx.SetTexture("ColorTexture", color);
        vfx.SetTexture("DepthTexture", depth);
    }

    void WritePointcloudPositionsFile(string path)
    {
        string text = vfx1.transform.position + Environment.NewLine + vfx1.transform.eulerAngles + Environment.NewLine + vfx1.transform.localScale + Environment.NewLine 
                    + vfx2.transform.position + Environment.NewLine + vfx2.transform.eulerAngles + Environment.NewLine + vfx2.transform.localScale;

        text = text.Replace(",", "");
        text = text.Replace("(", "");        
        text = text.Replace(")", "");

        File.WriteAllText(path, text);
    }
    
    void OnApplicationQuit()
    {
        //WritePointcloudPositionsFile(Application.streamingAssetsPath + "/pointcloud_positions.txt");
    }
}
