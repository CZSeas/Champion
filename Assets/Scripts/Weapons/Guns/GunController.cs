using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public delegate void UpdateGun();
    public static event UpdateGun ChangeGun;

    public Transform[] weaponHold;
    public Gun startingGun;
    Gun equippedGun;

    void Awake() {
        if (startingGun != null) {
            EquipGun(startingGun);
        }
    }

    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null) {
            float randomDir = Random.Range(0f, 1f);
            Vector3 randomDist = (new Vector3(randomDir, 2, 1 - randomDir))
                * GetComponent<PlayerController>().collisionRadius * 3;
            Destroy(Instantiate(equippedGun.gunDrop, transform.position + randomDist,
                Quaternion.identity).gameObject, 20);
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold[gunToEquip.GetWeaponHoldIdx()].position,
            weaponHold[gunToEquip.GetWeaponHoldIdx()].rotation) as Gun;
        equippedGun.transform.parent = weaponHold[gunToEquip.GetWeaponHoldIdx()];
        if (ChangeGun != null) {
            ChangeGun();
        }
        equippedGun.LoadGun();
    }

    public Gun GetEquipped() {
        return equippedGun;
    }

    public void Shoot() {
        if (equippedGun != null) {
            equippedGun.Shoot();
        }
    }
    
    public void Reload() {
        if (equippedGun != null) {
            equippedGun.StartCoroutine(equippedGun.Reload());
        }
    }

    public void Ult() {
        if (equippedGun != null) {
            equippedGun.StartUlt();
        }
    }

    public float GunHeight() {
        return weaponHold[0].position.y;
    }

    public Vector3 MuzzlePosition() {
        if (equippedGun != null && equippedGun.muzzle.Length > 0) {
            return equippedGun.muzzle[0].position;
        }
        return Vector3.zero;
    }
    public void Aim(Vector3 aimPoint) {
        if (equippedGun != null) {
            equippedGun.Aim(aimPoint);
        }
    }

}
