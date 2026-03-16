using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSlicing : MonoBehaviour
{
    public Material slicedMaterial;

    public float moveSpeed = 2f;
    private Rigidbody rb;
    private GameObject upperHullToRemove = null;

    public GameObject linkObject;

    [Header("Ingate")]
    public int maxSlices = 3;
    private int currentSlice = 0;
    private List<GameObject> upperPartsToRemove = new List<GameObject>();
    private bool startMulitpleCut = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveY = 0;

        if (Input.GetKey(KeyCode.Q)) moveY = 1;
        if (Input.GetKey(KeyCode.E)) moveY = -1;

        Vector3 moveDirection = new Vector3(moveX, moveY, moveZ).normalized;
        rb.velocity = moveDirection * moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"collide with: {other.gameObject.name}");
        if (other.gameObject.CompareTag("Sliceable"))
        {
            Debug.Log("Enter cut zone...");
            Vector3 planeNormal = transform.up;
            Vector3 planePoint = transform.position;

            SliceObject(other.gameObject, planePoint, planeNormal, "Sliceable");
        }
        else if (other.gameObject.CompareTag("Ingate"))
        {
            Vector3 planeNormal = transform.up;
            Vector3 planePoint = transform.position;

            if (startMulitpleCut == false)
            {
                SliceObject(other.gameObject, planePoint, planeNormal, "Ingate");
                Debug.Log("Remove first part");
            }
            else
            {
                Debug.Log($"Execute multiple cut, current slice number: {currentSlice}");
                MultiSliceObject(other.gameObject, planePoint, planeNormal);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (upperHullToRemove != null)
        {
            Destroy(upperHullToRemove);
            upperHullToRemove = null;
        }

        if (other.gameObject.CompareTag("Ingate"))
        {
            if (startMulitpleCut == false)
            {
                startMulitpleCut = true;
                Debug.Log("Ready for multiple cut");
                return;
            }

            currentSlice++;
            Debug.Log(currentSlice);

            if (currentSlice == maxSlices)
            {
                RemoveUpperParts();
                currentSlice = 0;
            }
        }
    }

    private void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal, string lowerHullTag)
    {
        SlicedHull slicedHull = target.Slice(planePoint, planeNormal, slicedMaterial);

        if (slicedHull != null)
        {
            upperHullToRemove = slicedHull.CreateUpperHull(target, slicedMaterial);

            GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
            ApplyComponents(lowerHull, lowerHullTag);

            Destroy(target);
        }
    }

    private void MultiSliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal)
    {
        if (currentSlice < maxSlices)
        {
            SlicedHull slicedHull = target.Slice(planePoint, planeNormal, slicedMaterial);

            if (slicedHull != null)
            {
                GameObject upperHull = slicedHull.CreateUpperHull(target, slicedMaterial);
                ApplyComponents(upperHull, "ToRemove");
                upperPartsToRemove.Add(upperHull);

                GameObject lowerHull = slicedHull.CreateLowerHull(target, slicedMaterial);
                ApplyComponents(lowerHull, "Ingate");

                //currentSlice++;

                Destroy(target);
                return;
            }
        }
    }

    private void ApplyComponents(GameObject obj, string hullTag)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.tag = hullTag;
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
}
