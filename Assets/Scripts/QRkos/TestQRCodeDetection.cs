using UnityEngine;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class TestQRCodeDetection : MonoBehaviour
{
    [SerializeField] private ARMarkerManager markerManager;

    //private GameObject QRkos;
    //private GameObject Station;
    private GameObject QRorigin;
    private GameObject QRpart;
    //public float rw;
    //public float rx;
    //public float ry;
    //public float rz;

    private void Start()
    {
        if (markerManager == null)
        {
            Debug.LogError("ARMarkerManager is not assigned.");
            return;
        }

        // Find the Crosshair Table and Station game objects
        //QRkos = GameObject.Find("QRkos");
        //Station = GameObject.Find("Station");
        QRorigin = GameObject.Find("QRorigin");
        QRpart = GameObject.Find("QRpart");
        if (QRpart == null || QRorigin == null)
        {
            Debug.LogError("QRorigin or QRpart not found in scene.");
            return;
        }

        // Subscribe to the markersChanged event
        markerManager.markersChanged += OnMarkersChanged;
    }

    /// <summary>
    /// Handles the markersChanged event and processes added, updated, and removed markers.
    /// </summary>
    /// <param name="args">Event arguments containing information about added, updated, and removed markers.</param>
    private void OnMarkersChanged(ARMarkersChangedEventArgs args)
    {
        foreach (var addedMarker in args.added)
        {
            HandleAddedMarker(addedMarker);
        }

        foreach (var updatedMarker in args.updated)
        {
            HandleUpdatedMarker(updatedMarker);
        }

        foreach (var removedMarkerId in args.removed)
        {
            HandleRemovedMarker(removedMarkerId);
        }
    }

    /// <summary>
    /// Handles logic for newly added markers.
    /// </summary>
    /// <param name="addedMarker">The newly added ARMarker.</param>
    private void HandleAddedMarker(ARMarker addedMarker)
    {
        Debug.Log($"QR Code Content: {addedMarker.GetDecodedString()}");
        // You can access more information about the marker using addedMarker properties
        // For example, addedMarker.GetDecodedString() or addedMarker.GetQRCodeProperties()
        // Additional handling logic for newly added markers

        // Debug.Log($"QR Code Position: {addedMarker.transform.position}");
    }

    /// <summary>
    /// Handles logic for updated markers.
    /// </summary>
    /// <param name="updatedMarker">The updated ARMarker.</param>
    private void HandleUpdatedMarker(ARMarker updatedMarker)
    {
        // Debug.Log($"QR Code updated! Marker ID: {updatedMarker}");
        // You can access information about the marker using updatedMarker properties
        // Additional handling logic for updated markers

        // Get the decoded string from the added marker
        string qrContent = updatedMarker.GetDecodedString().Trim();
        Debug.Log($"Detected QR Code: {qrContent}");

        if (qrContent == "QRCode Detected!")
        {
            // 更新 QRorigin 对应的对象
            InitRobotPose(updatedMarker);
        }
        else if (qrContent == "QRpartAnchor")
        {
            // 更新 QRpart 对应的对象
            InitPartPose(updatedMarker);
        }
        else
        {
            Debug.Log($"Unrecognized QR Code: {qrContent}");
        }

        
    }

    /// <summary>
    /// Handles logic for removed markers.
    /// </summary>
    /// <param name="removedMarkerId">The ID of the removed marker.</param>
    private void HandleRemovedMarker(ARMarker removedMarkerId)
    {
        Debug.Log($"QR Code Removed! Marker ID: {removedMarkerId}");
    }

    private void InitRobotPose(ARMarker detectedQRCode)
    {
        if (detectedQRCode == null)
        {
            Debug.LogError("Detected QR code is null.");
            return;
        }

        Quaternion qrCodeRotation = detectedQRCode.transform.rotation;
        Vector3 qrCodePosition = detectedQRCode.transform.position;

        // Apply the rotation to Station

        //Quaternion extraRotation = Quaternion.LookRotation(
        //        new Vector3(0, 1, 0),  //在Forward方向上Unity flange(KOS_tool)的+Z要映射到机器人flange的-X
        //        new Vector3(0, 0, 1)   //在Up方向上Unity flange(KOS_tool)的+Y要映射到机器人flange的-Z
        //    );
        //station.transform.rotation = extraRotation * qrCodeRotation * Quaternion.Inverse(extraRotation);
        //Quaternion extraRotation = Quaternion.Euler(90, 90, 0);
        //station.transform.rotation = qrCodeRotation * extraRotation;
        //rw = qrCodeRotation.w;
        //rx = qrCodeRotation.x;
        //ry = qrCodeRotation.y;
        //rz = qrCodeRotation.z;
        QRorigin.transform.rotation = qrCodeRotation;
        QRorigin.transform.position = qrCodePosition;

        // Get the current position of Crosshair Table in world space
        //Vector3 QRkosWorldPosition = QRkos.transform.position;
        // Calculate the vector from Crosshair Table to the QR Code
        //Vector3 positionOffset = qrCodePosition - QRkosWorldPosition;
        // Move the Station by applying the offset
        //QRorigin.transform.position += positionOffset;
    }

    private void InitPartPose(ARMarker detectedQRCode)
    {
        if (detectedQRCode == null)
        {
            Debug.LogError("Detected QR code is null.");
            return;
        }

        Quaternion qrCodeRotation = detectedQRCode.transform.rotation;
        Vector3 qrCodePosition = detectedQRCode.transform.position;
        QRpart.transform.rotation = qrCodeRotation;
        QRpart.transform.position = qrCodePosition;
    }
}

