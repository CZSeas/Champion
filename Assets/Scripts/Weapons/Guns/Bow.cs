using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Gun
{
    public int maxDamage = 6;
    public float maxForce = 25;
    public float maxSpeed = 100;
    public Animator animator;
    public LineRenderer bowString;
    public Transform bowModel;
    public Transform stringPointA;
    public Transform stringPointB;
    public Transform stringPointC;
    public Transform stringPointD;
    public GameObject arrow;
    public float chargeTime;

    public override event System.Action<Projectile> OnShoot;

    bool isHolding = false;

    protected override void Start() {
        maxAmmo = totalAmmo;
        arrow.SetActive(false);
    }

    public override IEnumerator Reload() {
        yield break;
    }

    public override void Shoot() {
        if (Time.time > nextShotTime && !isHolding && (bulletsRemaining > 0 || bulletsRemaining == -1) && !reloading) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            isHolding = true;
            StartCoroutine(Pull());
        }
    }

    protected override void LateUpdate() {
        bowString.SetPosition(0, bowModel.InverseTransformPoint(stringPointA.position));
        bowString.SetPosition(2, bowModel.InverseTransformPoint(stringPointB.position));
        if (!isHolding) {
            bowString.SetPosition(1, bowModel.InverseTransformPoint(stringPointC.position));
        }
        arrow.transform.position = bowModel.TransformPoint(bowString.GetPosition(1));
    }

    IEnumerator PullString() {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Pull"));
        arrow.SetActive(true);
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1
            && animator.GetCurrentAnimatorStateInfo(0).IsName("Pull")) {
            bowString.SetPosition(1, Vector3.Lerp(bowModel.InverseTransformPoint(stringPointC.position),
                bowModel.InverseTransformPoint(stringPointD.position),
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
            yield return null;
        }
    }

    IEnumerator ReleaseString() {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Release"));
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1
            && animator.GetCurrentAnimatorStateInfo(0).IsName("Release")) {
            bowString.SetPosition(1, Vector3.Lerp(bowString.GetPosition(1),
                bowModel.InverseTransformPoint(stringPointC.position),
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
            yield return null;
        }
        arrow.SetActive(false);
        isHolding = false;
    }

    IEnumerator Pull() {
        animator.SetTrigger("Pull");
        StartCoroutine("PullString");
        int arrowDamage = damage;
        float arrowSpeed = bulletSpeed;
        float arrowForce = knockBackForce;
        float percent = 0;
        while(Input.GetMouseButton(0)) {
            percent += Time.deltaTime;
            if (percent < chargeTime) {
                arrowDamage = damage + (int) ((maxDamage + 1 - damage) * percent / chargeTime);
                arrowSpeed = bulletSpeed + (maxSpeed - bulletSpeed) * percent / chargeTime;
                arrowForce = knockBackForce + (maxForce - knockBackForce) * percent / chargeTime;
            }
            yield return null;
        }
        animator.SetTrigger("Release");
        StopCoroutine("PullString");
        StartCoroutine(ReleaseString());
        Quaternion randRot = Quaternion.Euler(0, Random.Range(-spread / 2, spread / 2), 0);
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distToMuzzle, collisionMask, QueryTriggerInteraction.Collide)) {
            Projectile newProjectile = Instantiate(projectile, transform.position, transform.rotation * randRot) as Projectile;
            newProjectile.SetSpeed(arrowSpeed);
            newProjectile.SetDamage(arrowDamage);
            newProjectile.SetKnockForce(arrowForce);
            if (OnShoot != null) {
                OnShoot(newProjectile);
            }
        } else {
            for (int i = 0; i < muzzle.Length; i++) {
                Projectile newProjectile = Instantiate(projectile, muzzle[i].position, muzzle[i].rotation * randRot) as Projectile;
                newProjectile.SetSpeed(arrowSpeed);
                newProjectile.SetDamage(arrowDamage);
                newProjectile.SetKnockForce(arrowForce);
                if (OnShoot != null) {
                    OnShoot(newProjectile);
                }
            }
        }
        if (AudioManager.instance != null) {
            AudioManager.instance.PlaySound(shotSound);
        }
    }
}
