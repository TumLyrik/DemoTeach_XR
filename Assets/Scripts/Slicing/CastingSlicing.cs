using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EzySlice;
using RosSharp.RosBridgeClient;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

// 这个脚本的核心功能是根据ROS系统的切割状态信号，对带有特定标签的3D物体进行实时切割操作。
public class CastingSlicing : MonoBehaviour
{
    public Material slicedMaterial; // 切割后新生成面的材质
    public Collider tool; // public Collider part;
    public GameObject triggerobject;
    public AudioClip beep;
    public bool physics;
    public bool collideTrigger = false;
    public List<int> removedlist = new List<int>(); // 存储移除的part ID

    private TriggerSignal triggerbody;
    private bool sliceExit = false;
    private Renderer partRenderer; // 用于访问 Mesh 的材质
    private Renderer toolRenderer; // 用于访问 Mesh 的材质
   
    AudioSource audioSource;
    string lastTag; // 记录最后碰撞物体的标签

    public List<string> OriginalParts = new List<string> {"base","anguss","speise","pfeiffe1","pfeiffe2","pfeiffe3","pfeiffe4","pfeiffe5","pfeiffe6"};
    private List<List<GameObject>> removeGroup = new List<List<GameObject>>(); // 存储需要移除的上半部分物体
    private List<GameObject> currentRemoveBatch = null;
    private List<List<GameObject>> removeGroup_low = new List<List<GameObject>>(); // 存储需要移除的上半部分物体
    private List<GameObject> currentRemoveBatch_low = null;
    private List<List<GameObject>> hiddenPartGrouped = new List<List<GameObject>>(); //存储slice前的状态（将要隐藏的部件）
    private List<GameObject> currentPartBatch = null;
    private List<List<int>> sliceIDGroup = new List<List<int>>(); // 存储移除的part ID
    private List<GameObject> upperPartsToRemove = new List<GameObject>(); // 存储需要移除的上半部分物体
    private List<int> currentSliceIDBatch = null;
    private bool cleaned = false;
    private UpdateRobotPose rosconnect;

    // Use this for initialization
    void Start()
    {
        triggerbody = triggerobject.GetComponent<TriggerSignal>();
        audioSource = GetComponent<AudioSource>();
        //partRenderer = part.GetComponent<Renderer>();
        toolRenderer = tool.GetComponent<Renderer>();
        rosconnect = FindObjectOfType<UpdateRobotPose>();

        // 确保对象有材质，否则抛出警告
        //if (partRenderer == null)
        //{
        //    Debug.LogWarning("No Renderer found on the object. Please add a Mesh Renderer.");
        //}
    }

    // 每帧检查ROS系统的切割状态，如果切割未激活则清理之前切割产生的上半部分物体。
    private void Update()
    {
        if (!triggerbody.recordTrigger && sliceExit)
        {
            //RemoveUpperParts(); // 清理上半部分
            collideTrigger = false;
            sliceExit = false;
            currentPartBatch = null;
            currentSliceIDBatch = null;
            currentRemoveBatch = null;
            currentRemoveBatch_low = null;
        }
        
        if (triggerbody.trailID < hiddenPartGrouped.Count && hiddenPartGrouped.Count != 0 && !rosconnect.rivz_connect)
        {
            UndoLastCut(); // 按 Z 撤销最近一次切割
        }
        if ((triggerbody.RosStream || Input.GetKey(KeyCode.X)) && !cleaned)
        {
            ResetToOriginal();
        }

        if (!physics)
        {
            RemoveUpperParts();
        }
    }

    // 碰撞检测和切割逻辑 
    private void OnTriggerStay(Collider other)
    {
        lastTag = other.tag;
        partRenderer = other.GetComponent<Renderer>();
        // 普通可切割物体（"Sliceable"标签, 使用当前transform的位置和法向量进行切割, 下半部分标记为"Sliceable"
        if (other.gameObject.CompareTag("Sliceable"))
        {
            if (triggerbody.recordTrigger || rosconnect.rivz_connect) 
            {
                partRenderer.material.color = new Color(0.0f, 0.0f, 0.8f); // blue
                audioSource.PlayOneShot(beep, 0.7F);
                toolRenderer.material.color = new Color(0.0f, 0.8f, 0.0f); // green
                //Debug.Log($"collide with: {other.gameObject.name}");
                Vector3 planeNormal = -transform.up;
                Vector3 planePoint = transform.position;
                if (currentPartBatch == null)
                {
                    currentPartBatch = new List<GameObject>();
                    hiddenPartGrouped.Add(currentPartBatch);
                    currentSliceIDBatch = new List<int>();
                    sliceIDGroup.Add(currentSliceIDBatch);
                    currentRemoveBatch = new List<GameObject>();
                    removeGroup.Add(currentRemoveBatch);
                    currentRemoveBatch_low = new List<GameObject>();
                    removeGroup_low.Add(currentRemoveBatch_low);
                }
                SliceObject(other.gameObject, planePoint, planeNormal, "Sliceable");
                cleaned = false;
            }
        }
        // 浇口物体"Ingate"标签,切割浇口，下半部分标记为"IngateSliceable"
        else if (other.gameObject.CompareTag("Reference") && triggerbody.recordTrigger)
        {
            //Debug.Log($"collide with: {other.gameObject.name}");
            partRenderer.material.color = new Color(0.8f, 0.0f, 0.0f); // red
            audioSource.PlayOneShot(beep, 0.7F);
            toolRenderer.material.color = new Color(0.8f, 0.0f, 0.0f); // red
            collideTrigger = true;
            //Vector3 planeNormal = -transform.up;
            //Vector3 planePoint = transform.position;
            //SliceObject(other.gameObject, planePoint, planeNormal, "IngateSliceable");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log($"Exit: {other.gameObject.name}");
        toolRenderer.material.color = new Color(0.5f, 0.5f, 0.5f); // normal
        if (other.gameObject.name == "base")
        {
            partRenderer = other.GetComponent<Renderer>();
            //partRenderer.material.color = new Color(0.5f, 0.5f, 0.5f); // normal
            Material mat = Resources.Load<Material>("Transparent_Voll");
            partRenderer.material = mat;
        }
        sliceExit = true;
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
            if (IsValidHull(upperHull))
            {
                upperHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
                CreateNewPartInUnity(upperHull, "ToRemove", target.transform);
                if (!physics)
                {
                    upperPartsToRemove.Add(upperHull);
                }
                else
                {
                    SwitchToPhysicsMode(upperHull); // dynamical simulation
                }
                currentRemoveBatch.Add(upperHull);
            }
            else
            {
                Destroy(upperHull); // 忽略“铁屑”
            }

            GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
            if (IsValidHull(lowerHull))
            {
                lowerHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
                CreateNewPartInUnity(lowerHull, lowerHullFlag, target.transform);
                currentRemoveBatch_low.Add(lowerHull);
            }
            else
            {
                Destroy(lowerHull);
            }

            //Destroy(target);
            target.SetActive(false); // Hidden Objects, not destroy Objects as usual

            if (currentPartBatch != null)
            {
                currentPartBatch.Add(target); // target part
            }

            if (target.name == "pfeiffe1") { AddUniqueInt(1); }
            if (target.name == "pfeiffe2") { AddUniqueInt(2); }
            if (target.name == "pfeiffe3") { AddUniqueInt(3); }
            if (target.name == "pfeiffe4") { AddUniqueInt(4); }
            if (target.name == "pfeiffe5") { AddUniqueInt(5); }
            if (target.name == "pfeiffe6") { AddUniqueInt(6); }
            if (target.name == "base") { AddUniqueInt(7); }
            if (target.name == "anguss") { AddUniqueInt(8); }
            if (target.name == "speise") { AddUniqueInt(9); }
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

    //忽略“微小切割片段”，防止物理崩溃
    private bool IsValidHull(GameObject obj)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        Mesh mesh = mf.sharedMesh;

        // 1. 顶点数不足
        if (mesh.vertexCount < 4)
            return false;

        // 2. Mesh 极小（单位为米，0.01m = 1cm）
        if (mesh.bounds.size.magnitude < 0.01f)
            return false;

        // 3. 面片过少（可选）
        if (mesh.triangles.Length < 12)
            return false;

        return true;
    }

    private void AddUniqueInt(int number)
    {
        if (!removedlist.Contains(number))
        {
            removedlist.Add(number);
            currentSliceIDBatch.Add(number);
        }
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

    //public void RemoveUpperParts()
    //{
    //    foreach (GameObject upper in currentRemoveBatch)
    //    {
    //        if (upper != null)
    //            Destroy(upper);
    //    }
    //    currentRemoveBatch.Clear();
    //    currentRemoveBatch_low.Clear();
    //}

    public void UndoLastCut()
    {
        // activate last hiddenGroup
        if (hiddenPartGrouped.Count == 0) return;
        List<GameObject> lastGroup = hiddenPartGrouped[^1]; // C# ^1 = 最后一个元素
        foreach (GameObject go in lastGroup)
        {
            if (go != null)
                go.SetActive(true);
        }
        hiddenPartGrouped.RemoveAt(hiddenPartGrouped.Count - 1);

        // destroy last lower part
        List<GameObject> lastRemoveGroup_low = removeGroup_low[^1]; // C# ^1 = 最后一个元素
        foreach (GameObject go in lastRemoveGroup_low)
        {
            if (go != null)
                Destroy(go);
        }
        removeGroup_low.RemoveAt(removeGroup_low.Count - 1);

        // remove last upper part
        if (physics) 
        {
            List<GameObject> lastRemoveGroup = removeGroup[^1]; // C# ^1 = 最后一个元素
            foreach (GameObject go in lastRemoveGroup)
            {
                if (go != null)
                    Destroy(go);
            }
            removeGroup.RemoveAt(removeGroup.Count - 1);
        }
        
        // update remove list
        if (sliceIDGroup.Count == 0) return;
        List<int> lastIDs = sliceIDGroup[^1];
        sliceIDGroup.RemoveAt(sliceIDGroup.Count - 1);
        foreach (int id in lastIDs)
        {
            // 如果其他切割组中不再包含这个 ID，就从 removedlist 删除
            bool stillUsed = false;
            foreach (List<int> group in sliceIDGroup)
            {
                if (group.Contains(id))
                {
                    stillUsed = true;
                    break;
                }
            }

            if (!stillUsed)
            {
                removedlist.Remove(id);
            }
        }

        Debug.Log("已撤销最近一次切割，当前移除列表为：" + string.Join(",", removedlist));
    }

    public void ResetToOriginal()
    {
        GameObject rootParent = GameObject.Find("Castingv2");
        foreach (Transform child in rootParent.transform)
        {
            GameObject obj = child.gameObject;

            if (!OriginalParts.Contains(obj.name))
            {
                Destroy(obj);
            }
            else
            {
                obj.SetActive(true); // 确保原始物体可见
            }
        }

        // 清空所有记录数据（确保同步）
        removedlist.Clear();
        removeGroup.Clear();
        hiddenPartGrouped.Clear();
        sliceIDGroup.Clear();
        cleaned = true;
        Debug.Log("通过标签过滤完成重置");
    }

    private void RemoveUpperParts()
    {
        foreach (GameObject upper in upperPartsToRemove)
        {
            if (upper != null)
                Destroy(upper);
        }
        upperPartsToRemove.Clear();
    }

    //public void ResetToInitialState()
    //{
    //    // 1. 恢复所有隐藏原始部件
    //    foreach (List<GameObject> batch in hiddenPartGrouped)
    //    {
    //        foreach (GameObject obj in batch)
    //        {
    //            if (obj != null)
    //                obj.SetActive(true);
    //        }
    //    }

    //    // 2. 清除所有切割生成的新物体（upper hull）
    //    foreach (GameObject upper in currentRemoveBatch)
    //    {
    //        if (upper != null)
    //            Destroy(upper);
    //    }

    //    // 3. 清空记录数据
    //    hiddenPartGrouped.Clear();
    //    currentRemoveBatch.Clear();
    //    removedlist.Clear();
    //    sliceIDGroup.Clear();

    //    // 4. 日志提示
    //    Debug.Log("reset to initial");
    //}
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