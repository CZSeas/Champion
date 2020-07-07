using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCount : MonoBehaviour
{
    GunController playerGunController;
    Gun equippedGun;
    public Text bulletsRemaining;
    public Text totalAmmo;

    void Start() {
        playerGunController = GameObject.FindWithTag("Player").GetComponent<GunController>();
    }

    void Update() {
        equippedGun = playerGunController.GetEquipped();
        if (equippedGun != null) {
            int bulletsRemainingCount = equippedGun.GetBulletsRemaining();
            if (bulletsRemainingCount == -1) {
                bulletsRemaining.text = "∞";
            } else {
                bulletsRemaining.text = bulletsRemainingCount.ToString();
            }
            int totalAmmoCount = equippedGun.totalAmmo;
            if (totalAmmoCount == -1) {
                totalAmmo.text = "∞";
            } else {
                totalAmmo.text = totalAmmoCount.ToString();
            }
        }
    }
}
