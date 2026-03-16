using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class MeshSlicer : MonoBehaviour
{
    public GameObject targetObject; 
    public Material cutMaterial; 

    // Start is called before the first frame update
    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
            return;
        }

        Vector3 planePoint = targetObject.transform.position + Vector3.up * 0.04f;

        Vector3 planeNormal = Vector3.up;

        SlicedHull slicedHull = targetObject.Slice(planePoint, planeNormal);

        if (slicedHull != null)
        {
            // create upper part
            GameObject upperHull = slicedHull.CreateUpperHull(targetObject, cutMaterial);
            if (upperHull != null)
            {
                upperHull.transform.position = targetObject.transform.position;
                upperHull.transform.rotation = targetObject.transform.rotation;
            }

            // create lower part
            GameObject lowerHull = slicedHull.CreateLowerHull(targetObject, cutMaterial);
            if (lowerHull != null)
            {
                lowerHull.transform.position = targetObject.transform.position;
                lowerHull.transform.rotation = targetObject.transform.rotation;
            }

            Destroy(targetObject);
        }
        else
        {
            Debug.LogError("Failed to slice the object.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
