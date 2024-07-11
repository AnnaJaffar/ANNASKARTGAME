using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverCameraView : MonoBehaviour
{
    
    // PAPA edit
    public float cameraLoweringAmount = 0.35f; // Amount to lower the camera view   

    public void LowerCamera()
    {
        // PAPA Lower the camera view by cameraLoweringAmount
        transform.localPosition -= new Vector3(0f, cameraLoweringAmount, 0f);
    }

    public void RaiseCamera()
    {
        // PAPA Lower the camera view by cameraLoweringAmount
        transform.localPosition += new Vector3(0f, cameraLoweringAmount, 0f);
    }
}
