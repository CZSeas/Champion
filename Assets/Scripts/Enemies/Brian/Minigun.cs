using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigun : Gun
{
    public override void Aim(Vector3 aimPoint) {
        if (!reloading) {
            muzzle[0].LookAt(new Vector3(aimPoint.x, aimPoint.y - 1f, aimPoint.z));
        }
    }
}
