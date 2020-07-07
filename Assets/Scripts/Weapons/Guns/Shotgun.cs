using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    int ultBullets;
    public Transform[] ultMuzzle;

    public override event System.Action<Projectile> OnShoot;

    public override IEnumerator Ult() {
        //CHANGE TIME TO INCREASE RATE
        if (currentState != State.Ulting && Time.time > ultCooldownTime && !reloading) {
            currentState = State.Ulting;
            float initialSpeed = bulletSpeed;
            int initialDamage = damage;
            bulletSpeed = 30;
            damage = 2;
            FullyLoad();
            ultBullets = magazineSize;
            yield return new WaitUntil(() => ultBullets == 0);
            bulletSpeed = initialSpeed;
            damage = initialDamage;
            currentState = State.Normal;
            ultCooldownTime = Time.time + ultCooldown;
            StartCoroutine(Reload());
        }
        
    }

    public override void Shoot() {
        if (Time.time > nextShotTime && (bulletsRemaining > 0 || bulletsRemaining == -1) && !reloading) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Quaternion randRot = Quaternion.Euler(0, Random.Range(-spread / 2, spread / 2), 0);
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distToMuzzle, collisionMask, QueryTriggerInteraction.Collide)) {
                Projectile newProjectile = Instantiate(projectile, transform.position, transform.rotation * randRot) as Projectile;
                newProjectile.SetSpeed(bulletSpeed);
                newProjectile.SetDamage(damage);
                newProjectile.SetKnockForce(knockBackForce);
                ultBullets--;
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
                    ultBullets--;
                }
            }
            for (int i = 0; i < shellEjectors.Length; i++) {
                if (shellEjectors[i].gameObject.active) {
                    Instantiate(shell, shellEjectors[i].position, shellEjectors[i].rotation);
                }
            }
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(angleMinMax.x, angleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, angleMinMax.y);
            if (!unlimitedMag) {
                bulletsRemaining--;
            }
            if (bulletsRemaining <= 0) {
                StartCoroutine(Reload());
            }
            if (AudioManager.instance != null) {
                AudioManager.instance.PlaySound(shotSound);
            }
        }
    }
}
