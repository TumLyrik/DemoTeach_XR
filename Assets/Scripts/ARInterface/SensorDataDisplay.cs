using System;
using System.Collections;
using System.Collections.Generic;
//using MixedReality.Toolkit.SpatialManipulation;
//using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class SensorDataDisplay : MonoBehaviour
{
    //Vector3 localPosition;
    //Dialog dialog;

    private TextMeshPro tempUI;

    void Start()
    {
        //localPosition = transform.localPosition;
        //dialog = GetComponent<Dialog>();
        tempUI = this.gameObject.GetComponent<TextMeshPro>();
    }

    void Update()
    {
        // Set content of clipboard to dialog Window
        string clipboard = GUIUtility.systemCopyBuffer;
        if (clipboard != null) {
            //dialog.SetBody("Temp.: "+clipboard).ShowAsync();
            tempUI.text = "Trigger: " + clipboard;
        }

        /*// Disable following the camera
        Follow followComponent = GetComponent<Follow>();
        if (followComponent != null && !followComponent.IgnoreDistanceClamp) {
            followComponent.IgnoreDistanceClamp = true;
            // Make sure it kept its initial position
            transform.localPosition = localPosition;
        }*/
    }
}
