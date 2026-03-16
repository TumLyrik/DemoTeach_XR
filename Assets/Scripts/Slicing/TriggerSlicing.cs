//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using EzySlice;
//using RosSharp.RosBridgeClient;

//public class TriggerSlicing : MonoBehaviour
//{
//    public Material slicedMaterial;

//    [Header("Ingate")]
//    public int maxSlices = 3;
//    public List<GameObject> upperPartsToRemove = new List<GameObject>();

//    private void Update()
//    {
//        if (!CuttingStatusSubscriber.isCuttingActive)
//        {
//            //Debug.Log($"current to remove parts length: {upperPartsToRemove.Count}");
//            RemoveUpperParts();
//        }
//    }

//    private void OnTriggerStay(Collider other)
//    {
//        Debug.Log("enter cut zone...");
//        if (!CuttingStatusSubscriber.isCuttingActive)
//        {
//            if(other.gameObject.CompareTag("Sliceable") && other.gameObject.CompareTag("Ingate"))
//                Debug.Log($"cut in ROS not activated..., collide with: {other.gameObject.name}");
//            return;
//        }

//        Debug.Log($"part position: x:{other.transform.position.x}, y:{other.transform.position.y}, z:{other.transform.position.z}");

//        if (other.gameObject.CompareTag("Sliceable"))
//        {
//            Debug.Log($"collide with: {other.gameObject.name}");
//            Vector3 planeNormal = -transform.up; 
//            Vector3 planePoint = transform.position;

//            SliceObject(other.gameObject, planePoint, planeNormal, "LowerHull");
//        }
//        else if (other.gameObject.CompareTag("Ingate"))
//        {
//            Debug.Log($"collide with: {other.gameObject.name}");

//            Vector3 planeNormal = -transform.up;
//            Vector3 planePoint = transform.position;

//            SliceObject(other.gameObject, planePoint, planeNormal, "IngateSliceable");
//        }
//        if (other.gameObject.CompareTag("IngateSliceable"))
//        {
//            Debug.Log($"collide with: {other.gameObject.name}");
//            Vector3 planeNormal = -transform.up;
//            Vector3 planePoint = transform.position;

//            SliceObject(other.gameObject, planePoint, planeNormal, "IngateSliceable");
//        }
//    }

//    private void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal, string lowerHullFlag)
//    {
//        SlicedHull slicedHull = target.Slice(planePoint, planeNormal, slicedMaterial);

//        if (slicedHull != null)
//        {
//            GameObject upperHull = slicedHull.CreateUpperHull(target, slicedMaterial);
//            upperHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
//            CreateNewPartInUnity(upperHull, "ToRemove", target.transform);

//            upperPartsToRemove.Add(upperHull);

//            GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
//            lowerHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
//            CreateNewPartInUnity(lowerHull, lowerHullFlag, target.transform);

//            Destroy(target);
//        }
//    }

//    private void CreateNewPartInUnity(GameObject obj, string hullTag, Transform targetTransform)
//    {
//        obj.AddComponent<MeshCollider>().convex = true;
//        obj.tag = hullTag;

//        //obj.transform.position = targetTransform.position;
//        //obj.transform.rotation = targetTransform.rotation;

//        obj.transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
//    }

//    public void RemoveUpperParts()
//    {
//        foreach (GameObject upper in upperPartsToRemove)
//        {
//            if (upper != null)
//                Destroy(upper);
//        }
//        upperPartsToRemove.Clear();
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EzySlice;
using RosSharp.RosBridgeClient;

public class TriggerSlicing : MonoBehaviour
{
    public Material slicedMaterial;

    [Header("Ingate")]
    public List<GameObject> upperPartsToRemove = new List<GameObject>();
    private bool ingateActive;
    string lastTag;

    public List<Transform> cuttingPlanes;

    private void Update()
    {
        if (!CuttingStatusSubscriber.isCuttingActive)
        {
            //Debug.Log($"current to remove parts length: {upperPartsToRemove.Count}");
            RemoveUpperParts();
        }
    }

    float CalculateDistanceToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        // 计算点到平面的距离公式
        float numerator = Mathf.Abs(Vector3.Dot(planeNormal, (point - planePoint)));
        float denominator = planeNormal.magnitude;
        return numerator / denominator;
    }

    Transform GetClosestCuttingPlane(List<Transform> planes, Transform tool)
    {
        Transform closestPlane = null;
        float minDistance = float.MaxValue;

        Vector3 toolPosition = tool.position; // 获取切割工具的世界坐标

        foreach (Transform plane in planes)
        {
            Vector3 planePoint = plane.position; // 平面上的某一点
            Vector3 planeNormal = plane.up; // 平面的法向量（假设 Y 轴朝上）

            float distance = CalculateDistanceToPlane(toolPosition, planePoint, planeNormal);
            Debug.Log($"Distance to {plane.name}: {distance}");

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlane = plane;
            }
        }

        return closestPlane;
    }

    private void OnTriggerStay(Collider other)
    {
        lastTag = other.tag;

        if (!CuttingStatusSubscriber.isCuttingActive)
        {
            if (other.gameObject.CompareTag("Sliceable") && other.gameObject.CompareTag("Ingate"))
                Debug.Log($"cut in ROS not activated..., collide with: {other.gameObject.name}");
            return;
        }

        if (other.gameObject.CompareTag("Sliceable") && !ingateActive)
        {
            Debug.Log($"collide with: {other.gameObject.name}");
            Vector3 planeNormal = -transform.up;
            Vector3 planePoint = transform.position;

            SliceObject(other.gameObject, planePoint, planeNormal, "LowerHull");
        }
        else if (other.gameObject.CompareTag("Ingate"))
        {
            Debug.Log($"collide with: {other.gameObject.name}");

            Vector3 planeNormal = -transform.up;
            Vector3 planePoint = transform.position;

            SliceObject(other.gameObject, planePoint, planeNormal, "IngateSliceable");
        }
        if (other.gameObject.CompareTag("IngateSliceable"))
        {
            Debug.Log($"collide with: {other.gameObject.name}");
            ingateActive = true;

            //Vector3 planeNormal = -transform.up;

            Transform closestPlane = GetClosestCuttingPlane(cuttingPlanes, transform);
            Vector3 planeNormal = -closestPlane.up;

            Vector3 planePoint = transform.position;

            SliceObject(other.gameObject, planePoint, planeNormal, "IngateSliceable");
        }
    }

    private void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal, string lowerHullFlag)
    {
        SlicedHull slicedHull = target.Slice(planePoint, planeNormal, slicedMaterial);

        if (slicedHull != null)
        {
            GameObject upperHull = slicedHull.CreateUpperHull(target, slicedMaterial);
            upperHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
            CreateNewPartInUnity(upperHull, "ToRemove", target.transform);

            upperPartsToRemove.Add(upperHull);

            GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
            lowerHull.transform.SetParent(target.transform.parent, worldPositionStays: true);
            CreateNewPartInUnity(lowerHull, lowerHullFlag, target.transform);

            Destroy(target);
        }
    }

    private void CreateNewPartInUnity(GameObject obj, string hullTag, Transform targetTransform)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.tag = hullTag;

        obj.transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
    }

    public void RemoveUpperParts()
    {
        if (upperPartsToRemove.Count > 0)
        {
            ingateActive = false;
        }
        foreach (GameObject upper in upperPartsToRemove)
        {
            if (upper != null)
                Destroy(upper);
        }
        upperPartsToRemove.Clear();
    }
}
