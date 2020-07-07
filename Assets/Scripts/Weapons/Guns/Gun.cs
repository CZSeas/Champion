using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State {
        Normal,
        Ulting
    };
    protected State currentState = State.Normal;
    protected bool reloading;

    public virtual event System.Action<Projectile> OnShoot;

    [Header("References")]
    public Transform[] muzzle;
    public Projectile projectile;
    public LayerMask collisionMask;
    public GunDrop gunDrop;

    [Header("Gun Parameters")]
    public int weaponHoldIdx = 0;
    public float msBetweenShots = 100;
    protected float nextShotTime;
    public float bulletSpeed;
    public int damage;
    public float knockBackForce;
    public float spread = 0;
    public int magazineSize = 12;
    public int totalAmmo = 60;
    protected int maxAmmo;
    public bool infiniteAmmo = false;
    public float ultCooldown = 5;
    public float ultDuration = 3;
    protected float ultCooldownTime;
    protected bool unlimitedMag = false;
    protected int bulletsRemaining;
    protected float distToMuzzle;
    public float recoilTime = 0.1f;
    public float reloadTime = 1f;
    public float maxReloadAngle = 60;
    public Vector2 kickMinMax = new Vector2(0.2f, 0.5f);
    public Vector2 angleMinMax = new Vector2(10, 30);

    [Header("Effects")]
    public Transform shell;
    public Transform[] shellEjectors;
    protected MuzzleFlash muzzleFlash;
    protected Vector3 recoilVelocity;
    protected float recoilRotVelocity;
    protected float recoilAngle;
    public AudioClip shotSound;
    public AudioClip reloadSound;

    protected virtual void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        if (transform.Find("MuzzleFlash") != null) {
            distToMuzzle = transform.Find("MuzzleFlash").localPosition.magnitude;
        }
        maxAmmo = totalAmmo;
    }

    public int GetWeaponHoldIdx() {
        return weaponHoldIdx;
    }

    public void LoadGun() {
        bulletsRemaining = magazineSize;
    }

    public void FullyLoad() {
        totalAmmo = maxAmmo;
        LoadGun();
    }

    public int GetBulletsRemaining() {
        return bulletsRemaining;
    }

    public int GetMaxAmmo() {
        return maxAmmo;
    }

    public State GetState() {
        return currentState;
    }

    protected virtual void LateUpdate() {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition,
            Vector3.zero, ref recoilVelocity, recoilTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotVelocity, recoilTime);
        if (!reloading) {
            transform.localEulerAngles = Vector3.left * recoilAngle + new Vector3(0, transform.localEulerAngles.y, 0);
        }
    }

    public virtual void Shoot() {
        if (Time.time > nextShotTime && (bulletsRemaining > 0 || bulletsRemaining == -1) && !reloading) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Quaternion randRot = Quaternion.Euler(0, Random.Range(-spread / 2,  spread / 2), 0);
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

    public virtual IEnumerator Reload() {
        if (!reloading && (totalAmmo > 0 || infiniteAmmo) && bulletsRemaining < magazineSize 
            && currentState != State.Ulting) {
            reloading = true;
            recoilAngle = 0;
            float reloadAngle = 0;
            float reloadTimer = 0;
            float percent = 0;
            while (reloadTimer < reloadTime / 3) {
                reloadAngle = Mathf.Lerp(reloadAngle, maxReloadAngle, reloadTimer * 3 / reloadTime);
                transform.localEulerAngles = Vector3.left * reloadAngle;
                reloadTimer += Time.deltaTime;
                percent += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(reloadTime / 3);
            reloadTimer = 0;
            while (reloadTimer < reloadTime / 3) {
                reloadAngle = Mathf.Lerp(reloadAngle, 0, reloadTimer * 3 / reloadTime);
                transform.localEulerAngles = Vector3.left * reloadAngle;
                reloadTimer += Time.deltaTime;
                yield return null;
            }
            if (!infiniteAmmo) {
                int bulletsLeftOver = bulletsRemaining;
                bulletsRemaining += (totalAmmo < magazineSize) ? totalAmmo % magazineSize : magazineSize;
                bulletsRemaining = Mathf.Min(magazineSize, bulletsRemaining);
                totalAmmo = Mathf.Max(0, totalAmmo - (magazineSize - bulletsLeftOver));
            } else {
                bulletsRemaining = magazineSize;
            }
            reloading = false;
            if (AudioManager.instance != null) {
                AudioManager.instance.PlaySound(reloadSound);
            }
        }
    }

    public virtual void Aim(Vector3 aimPoint) {
        if (!reloading) {
            transform.LookAt(aimPoint);
        }
    }

    public void StartUlt() {
        StartCoroutine(Ult());
    }

    public virtual IEnumerator Ult() {
        yield break;
    }
}
