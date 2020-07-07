using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera currentCam;
    Camera nextCam;

    public static CameraController instance;

    void Start() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }

    public void SetCurrentCam(Camera cam) {
        currentCam = cam;
        if (currentCam != null) {
            currentCam.enabled = true;
        }
    }

    public void SetNextCam(Camera cam) {
        nextCam = cam;
    }

    public void NextCam() {
        Destroy(currentCam.gameObject);
        SetCurrentCam(nextCam);
        nextCam = null;
    }

    public Camera GetCurrentCam() {
        return currentCam;
    }

    public Camera GetNextCam() {
        return nextCam;
    }
}
