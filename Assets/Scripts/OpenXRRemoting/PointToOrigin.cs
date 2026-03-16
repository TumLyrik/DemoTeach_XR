using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToOrigin : MonoBehaviour
{
    private Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 direction = Vector3.zero - cameraTransform.position;

        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * 1.5f;

        transform.position = targetPosition;

        transform.rotation = Quaternion.LookRotation(-direction, cameraTransform.up);
        
        transform.LookAt(Vector3.zero);        
    }
}
