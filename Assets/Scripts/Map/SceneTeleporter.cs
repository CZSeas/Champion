using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTeleporter : Teleporter
{
    public static event Action EndScene;

    protected override void OnTriggerEnter(Collider player) {
        if (player.tag != "Player") {
            return;
        }
        if (CameraController.instance.GetCurrentCam().gameObject.GetComponent<CameraTracker>() != null) {
            CameraController.instance.GetCurrentCam().gameObject.GetComponent<CameraTracker>().enabled = false;
        }
        EndScene();
    }
}
