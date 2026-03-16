using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

public class test : MonoBehaviour
{
    public string fileName = "trj.csv";
    private StreamWriter writer;

    // Start is called before the first frame update
    void Start()
    {
        string path = "C:/unity-ar/Files/" + fileName;
        writer = new StreamWriter(path, false); // false表示清空内容
    }

    // Update is called once per frame
    void Update()
    {
        if (TrailController.roslaunch_cmd == 0)
        {
            string data = "0.5,0.0,0.5,1.0,0.0,0.0,0.0";
            writer.WriteLine(data);
        }
        if (TrailController.roslaunch_cmd == 1)
        {
            writer.Close();
        }
    }
}
