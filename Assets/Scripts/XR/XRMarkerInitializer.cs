/* NOT USED
 * 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using Microsoft.MixedReality.OpenXR.ARSubsystems;

public class XRMarkerInitializer : MonoBehaviour
{
    private XRMarkerSubsystem markerSubsystem;

    private void Start()
    {
        StartCoroutine(InitializeXR());
    }

    private void OnDisable()
    {
        StopXR();
    }

    private IEnumerator InitializeXR()
    {
        // Initialize the XR loader
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        // Start XR subsystems
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StartSubsystems();

            // Retrieve the XRMarkerSubsystem descriptor
            var markerDescriptors = new List<XRMarkerSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(markerDescriptors);

            if (markerDescriptors.Count > 0)
            {
                markerSubsystem = markerDescriptors[0].Create();
                markerSubsystem.Start();
            }
            else
            {
                Debug.LogWarning("XRMarkerSubsystem is not available.");
            }
        }
        else
        {
            Debug.LogError("Failed to initialize XR Loader.");
        }
    }

    private void StopXR()
    {
        if (markerSubsystem != null)
        {
            markerSubsystem.Stop();
            markerSubsystem.Destroy();
            markerSubsystem = null;
        }

        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
