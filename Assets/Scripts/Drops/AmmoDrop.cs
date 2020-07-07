using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDrop : Drop
{
    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        Gun gun = c.GetComponent<GunController>().GetEquipped();
        if (gun.GetBulletsRemaining() < gun.magazineSize || gun.totalAmmo < gun.GetMaxAmmo()) {
            print(gun.totalAmmo);
            print(gun.GetMaxAmmo());
            gun.FullyLoad();
            Destroy(gameObject);
        }        
    }
    
}
