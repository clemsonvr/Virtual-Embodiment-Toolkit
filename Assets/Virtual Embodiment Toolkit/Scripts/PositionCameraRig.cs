using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CalibrationType
{
    NoCalibration,
    BasestationB,
    BasestationC
}

public class PositionCameraRig : MonoBehaviour {

    [Header("Configuration")]
    public CalibrationType CalibrationOn = CalibrationType.BasestationB;
    public bool ShowVizObjects;

    [Header("Objects")]
    public SteamVR_TrackedObject BasestationB;
    public SteamVR_TrackedObject BasestationC;
    public GameObject BasestationBTarget;
    public GameObject BasestationCTarget;
    public GameObject BasestationBViz;
    public GameObject BasestationCViz;
    public GameObject CameraRig;

    [Header("Debug")]
    public bool ReportDistanceOnly = false;
    public float DesiredDistanceBetweenBasestations;

    // Use this for initialization
    void Start()
    {

    }

    private void Update()
    {
        BasestationBViz.SetActive(ShowVizObjects);
        BasestationCViz.SetActive(ShowVizObjects);
    }


    void LateUpdate()
    {
        if (ReportDistanceOnly)
        {
            DesiredDistanceBetweenBasestations = (BasestationB.transform.position - BasestationC.transform.position).magnitude;
        }
        else
        {
            // Check to make sure the basestations are at approximately the correct distance from each other. This
            // check ensures that the basestations are being tracked before repositioning the room. We want the error between actual distance 
            // and desired distance to be less than 5%
            float basestationDistance = (BasestationB.transform.position - BasestationC.transform.position).magnitude;
            float distanceCheck = Mathf.Abs(basestationDistance - DesiredDistanceBetweenBasestations) / DesiredDistanceBetweenBasestations;
            if (distanceCheck < 0.05)
            {
                GameObject basestation = null;
                GameObject target = null;
                switch (CalibrationOn)
                {
                    case CalibrationType.NoCalibration:
                        return;
                    case CalibrationType.BasestationB:
                        basestation = BasestationB.gameObject;
                        target = BasestationBTarget;
                        break;
                    case CalibrationType.BasestationC:
                        basestation = BasestationC.gameObject;
                        target = BasestationCTarget;

                        break;
                }

                // Find the distance between the basestation and it's target
                Vector3 offset = target.transform.position - basestation.transform.position;

                // The devices are assigned incorrectly, swap them
                if (offset.sqrMagnitude > 1.0f)
                {
                    SteamVR_TrackedObject.EIndex tmpIndex = BasestationC.index;
                    BasestationC.SetDeviceIndex((int)BasestationB.index);
                    BasestationB.SetDeviceIndex((int)tmpIndex);
                }
                else
                {
                    // Correct the rotation
                    Quaternion rotationOffset = target.transform.rotation * Quaternion.Inverse(basestation.transform.rotation);
                    CameraRig.transform.rotation = rotationOffset * CameraRig.transform.rotation;

                    // Correct the position
                    offset = target.transform.position - basestation.transform.position;
                    CameraRig.transform.position += offset;
                }

            }
        }
    }
}
