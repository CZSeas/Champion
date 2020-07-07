using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumAR : Gun
{
    public Transform[] ultMuzzle;
    public GameObject[] ultFlashHolders;
    public GameObject ultArray;
    public GameObject mainGun;
    public float rotationSpeed = 2;

    public override event System.Action<Projectile> OnShoot;

    protected override void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        distToMuzzle = transform.Find("MainGun").Find("MuzzleFlash").localPosition.magnitude;
        maxAmmo = totalAmmo;
        ultArray.SetActive(false);
    }

    public override IEnumerator Ult() {
        //CHANGE TIME TO INCREASE RATE
        if (currentState != State.Ulting && Time.time > ultCooldownTime && !reloading) {
            currentState = State.Ulting;
            transform.localPosition = Vector3.zero;
            recoilAngle = 0;
            ultArray.SetActive(true);
            GameObject[] prevFlashHolders = muzzleFlash.flashHolders;
            muzzleFlash.flashHolders = ultFlashHolders;
            muzzleFlash.Deactivate();
            mainGun.SetActive(false);
            float initialMSBetweenShots = msBetweenShots;
            msBetweenShots = 50;
            float ultTimer = 0;
            while (ultTimer < ultDuration) {
                ultArray.transform.eulerAngles = ultArray.transform.eulerAngles + Vector3.up * rotationSpeed;
                ultTimer += Time.deltaTime;
                yield return null;
            }
            muzzleFlash.flashHolders = prevFlashHolders;
            ultArray.SetActive(false);
            mainGun.SetActive(true);
            msBetweenShots = initialMSBetweenShots;
            currentState = State.Normal;
            ultCooldownTime = Time.time + ultCooldown;
        }
        
    }

    public override void Shoot() {
        if (Time.time > nextShotTime && bulletsRemaining > 0 && !reloading) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Quaternion randRot = Quaternion.Euler(0, Random.Range(-spread / 2, spread / 2), 0);
            if (currentState == State.Ulting) {
                for (int i = 0; i < ultMuzzle.Length; i++) {
                    Projectile newProjectile = Instantiate(projectile,
                        ultMuzzle[i].position, ultMuzzle[i].rotation * randRot) as Projectile;
                    newProjectile.SetSpeed(bulletSpeed);
                    newProjectile.SetDamage(damage);
                    newProjectile.SetKnockForce(knockBackForce);
                    if (OnShoot != null) {
                        OnShoot(newProjectile);
                    }
                }
            } else {
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, distToMuzzle, collisionMask, QueryTriggerInteraction.Collide)) {
                    Projectile newProjectile = Instantiate(projectile, transform.position, transform.rotation * randRot) as Projectile;
                    newProjectile.SetSpeed(bulletSpeed);
                    newProjectile.SetDamage(damage);
                    newProjectile.SetKnockForce(knockBackForce);
                    if (OnShoot != null) {
                        OnShoot(newProjectile);
                    }
                } else {
                    for (int i = 0; i < muzzle.Length; i++) {
                        Projectile newProjectile = Instantiate(projectile, muzzle[i].position, muzzle[i].rotation * randRot) as Projectile;
                        newProjectile.SetSpeed(bulletSpeed);
                        newProjectile.SetDamage(damage);
                        newProjectile.SetKnockForce(knockBackForce);
                        if (OnShoot != null) {
                            OnShoot(newProjectile);
                        }
                    }
                }
                transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
                recoilAngle += Random.Range(angleMinMax.x, angleMinMax.y);
                recoilAngle = Mathf.Clamp(recoilAngle, 0, angleMinMax.y);
                bulletsRemaining--;
            }
            for (int i = 0; i < shellEjectors.Length; i++) {
                if (shellEjectors[i].gameObject.active) {
                    Instantiate(shell, shellEjectors[i].position, shellEjectors[i].rotation);
                }
            }
            muzzleFlash.Activate();
            if (bulletsRemaining <= 0) {
                StartCoroutine(Reload());
            }
            AudioManager.instance.PlaySound(shotSound);
        }
    }

    public override void Aim(Vector3 aimPoint) {
        if (!reloading && currentState != State.Ulting) {
            transform.LookAt(aimPoint);
        }
    }
}
