using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TFTransfer : MonoBehaviour
{
    public GameObject target;
    public bool stabscheifer;
    public float px;
    public float py;
    public float pz;
    public float qx;
    public float qy;
    public float qz;
    public float qw;

    private GameObject Base;

    // Start is called before the first frame update
    void Start()
    {
        Base = GameObject.Find("iiwa14");
    }

    // Update is called once per frame
    void Update()
    {
        tf();
    }

    void tf()
    {
        // 0: get transformation on left hand coordination
        // Ttarget2base(left)= target局部坐标 -> [localToWorldMatrix] ->世界坐标 -> [worldToLocalMatrix]  Base局部坐标
        // Target必须为cut_tool
        Matrix4x4 lh_m = Matrix4x4.identity; // left hand
        lh_m = Base.transform.worldToLocalMatrix * target.transform.localToWorldMatrix;

        // 1: convert left hand to right hand (旋转映射,位置坐标映射)
        // 坐标映射，参照为KOS_tool与robot flange(0 degree) in real space
        Quaternion lh_quat = lh_m.rotation;
        // Quaternion rh_quat = new Quaternion(lh_quat.x, -lh_quat.y, lh_quat.z, -lh_quat.w);
        Quaternion unityToReal = Quaternion.LookRotation(
            new Vector3(-1, 0, 0),  //在Forward方向上Unity flange(KOS_tool)的+Z要映射到机器人flange的-X
            new Vector3(0, 0, -1)   //在Up方向上Unity flange(KOS_tool)的+Y要映射到机器人flange的-Z
        );
        Quaternion rh_quat = unityToReal * lh_quat * Quaternion.Inverse(unityToReal);

        // 位置坐标轴转换：Unity LH -> Robot RH（手动映射）
        Vector3 lh_pos = lh_m.GetColumn(3);  // Unity中 Flange 相对 Base 的位置
        Vector3 rh_pos = new Vector3(lh_pos.z, -lh_pos.x, lh_pos.y);

        // 3: Teetobase -> Tfg2base
        if (stabscheifer)
        {
            Matrix4x4 Tfg2ee = new Matrix4x4(
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, -1, 0),
                new Vector4((float)-0.102, 0, (float)0.098, 1)
            ); //stabschleifer
            Matrix4x4 Ttip2base = Matrix4x4.identity;
            Ttip2base.SetTRS(rh_pos, rh_quat, Vector3.one);
            Matrix4x4 Tfg2base = Ttip2base * Tfg2ee; // Ttip2base = Tee2base
            Quaternion fg_qua = Tfg2base.rotation;
            Vector3 fg_pos = (Vector3)Tfg2base.GetColumn(3);
            px = fg_pos.x;
            py = fg_pos.y;
            pz = fg_pos.z;
            qx = fg_qua.x;
            qy = fg_qua.y;
            qz = fg_qua.z;
            qw = fg_qua.w;
        } else
        {
            Matrix4x4 Tee2tip = new Matrix4x4(
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, -1, 0),
                new Vector4(0, 0, 0, 1)
            );
            Matrix4x4 Ttip2base = Matrix4x4.identity;
            Ttip2base.SetTRS(rh_pos, rh_quat, Vector3.one);
            Matrix4x4 Tee2base = Ttip2base * Tee2tip;
            Quaternion ee_qua = Tee2base.rotation;
            Vector3 ee_pos = (Vector3)Tee2base.GetColumn(3);
            px = ee_pos.x;
            py = ee_pos.y;
            pz = ee_pos.z;
            qx = ee_qua.x;
            qy = ee_qua.y;
            qz = ee_qua.z;
            qw = ee_qua.w;
        }
    }
}
