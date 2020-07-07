using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDrop : Drop
{
    public Gun gun;

    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        GunController gunController = c.GetComponent<GunController>();
        if (gunController.GetEquipped().GetState() != Gun.State.Ulting) {
            gunController.EquipGun(gun);
            Destroy(gameObject);
        }
    }
}
