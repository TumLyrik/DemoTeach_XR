using System.Collections;
using System.Collections.Generic;
// using MixedReality.Toolkit;
// using MixedReality.Toolkit.UX;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PositionSelector : MonoBehaviour
{
    //public GameObject rightHandRayReticle;
    //public GameObject leftHandRayReticle;
    //public GameObject solderingStation;

    public void SetCameraPosition()
    {
        //Transform rightRingVisual = rightHandRayReticle.transform.GetChild(0);
        
        // Vector3 clickPosition;
        // if (rightRingVisual != null) {
        //     clickPosition = rightRingVisual.position;
        //     Debug.Log(clickPosition);
        //     solderingStation.transform.position = new Vector3(clickPosition.x, solderingStation.transform.position.y, clickPosition.z);
        // }
        // else {
        //     Debug.Log("Please use your right hand to set the camera position");
        // }
        Debug.Log("Resetted station");
        transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }

    void Update() {        
        if (Input.GetKeyUp(KeyCode.Space))
        {            
            SetCameraPosition();
        }
    }

    void Start() {
        SetCameraPosition();
    }
}
