using RobotSimulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBlockController : MonoBehaviour {
    public Camera[] cameras  = new Camera[0];
    private CameraManager cameraManager;
	// Use this for initialization
	void Start () {
        cameraManager = gameObject.GetComponent<CameraManager>();
    }

    public void ActivateCamera(int index)
    {
        cameraManager.ActivateCamera(RobotSimulation.CameraType.Normal, targetCam:cameras[index]);
    }

}
