using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EzySlice;
using RosSharp.RosBridgeClient;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

// 这个脚本的核心功能是根据ROS系统的切割状态信号，对带有特定标签的3D物体进行实时切割操作。
public class CastingSlicingPlay : MonoBehaviour
{
    public Material slicedMaterial; // 切割后新生成面的材质
    public Collider tool; // public Collider part;

    private Renderer partRenderer; // 用于访问 Mesh 的材质
    private Renderer toolRenderer; // 用于访问 Mesh 的材质

    private List<GameObject> upperPartsToRemove = new List<GameObject>(); // 存储需要移除的上半部分物体

    // Use this for initialization
    void Start()
    {
        //partRenderer = part.GetComponent<Renderer>();
        toolRenderer = tool.GetComponent<Renderer>();
    }

    // 每帧检查ROS系统的切割状态，如果切割未激活则清理之前切割产生的上半部分物体。
    private void Update()
    {
        
    }

    // 碰撞检测和切割逻辑 
    private void OnTriggerStay(Collider other)
    {
        partRenderer = other.GetComponent<Renderer>();
        // 普通可切割物体（"Sliceable"标签, 使用当前transform的位置和法向量进行切割, 下半部分标记为"Sliceable"
        if (other.gameObject.CompareTag("Sliceable"))
        {
            //Debug.Log($"collide with: {other.gameObject.name}");
            Vector3 planeNormal = -transform.up;
            Vector3 planePoint = transform.position;
            SliceObject(other.gameObject, planePoint, planeNormal, "Sliceable");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log($"Exit: {other.gameObject.name}");
        RemoveUpperParts();
    }

    // 切割执行方法,切割：target.Slice(planePoint, planeNormal, slicedMaterial)
    // 创建上半部分：标记为"ToRemove"，添加到待移除列表
    // 创建下半部分：使用传入的标签标记
    // 销毁原物体
    // 为新物体添加碰撞器和物理属性
    private void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal, string lowerHullFlag)
    {
        //Debug.Log("slicing");
        SlicedHull slicedHull = target.Slice(planePoint, planeNormal, slicedMaterial);

        if (slicedHull != null)
        {
            GameObject upperHull = slicedHull.CreateUpperHull(target, slicedMaterial);
            upperHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
            CreateNewPartInUnity(upperHull, "ToRemove", target.transform);
            //SwitchToPhysicsMode(upperHull); // dynamical simulation

            upperPartsToRemove.Add(upperHull);

            GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
            lowerHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
            CreateNewPartInUnity(lowerHull, lowerHullFlag, target.transform);

            Destroy(target);
        }
    }

    void SwitchToPhysicsMode(GameObject obj)
    {
        var meshCol = obj.GetComponent<MeshCollider>();
        var rb = obj.GetComponent<Rigidbody>();

        if (meshCol != null)
        {
            meshCol.convex = true; // 切换为凸面以支持物理
        }

        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
        }

        rb.isKinematic = false; // 启用物理模拟
    }

    // 清理方法
    // 销毁所有标记为移除的上半部分物体
    // 重置浇口激活状态
    // 清空待移除列表
    private void CreateNewPartInUnity(GameObject obj, string hullTag, Transform targetTransform)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.tag = hullTag;

        obj.transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
    }

    public void RemoveUpperParts()
    {
        foreach (GameObject upper in upperPartsToRemove)
        {
            if (upper != null)
                Destroy(upper);
        }
        upperPartsToRemove.Clear();
    }
}

//// This script is only triggered upon entering the trigger zone – “OnTriggerEnter” – so we don’t need 
//// to worry about multiple counts per visit in the trigger zone.
//void OnTriggerEnter(Collider partCollider)
//{
//    Debug.Log($"collide with: {partCollider.gameObject.name}");
//    partRenderer = partCollider.GetComponent<Renderer>();
//    if (partCollider.gameObject.name == "reference")
//    {
//        partRenderer.material.color = new Color(1.0f, 0.2f, 0.2f);
//    }
//    else
//    {
//        partRenderer.material.color = new Color(0.2f, 0.2f, 1.0f);
//    }

//    audioSource.PlayOneShot(beep, 0.7F);
//    toolRenderer.material.color = new Color(0.2f, 1.0f, 0.2f);
//}