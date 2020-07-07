using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCamera : CameraTracker
{
    protected override void TrackPlayer() {
        if (target != null) {
            transform.position = new Vector3(target.position.x, offsetHeight, target.position.z - offsetY);
        }
    }
}
