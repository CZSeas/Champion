using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(GunController))]
public class GunEnemy : Enemy
{

    GunController gunController;
    Gun gun;

    float sqrDistToMuzzle;

    protected override void FixedUpdate() {
        if (hasTarget) {
            base.FixedUpdate();
            float sqrDistToPoint = (new Vector2(target.position.x, target.position.z)
                    - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude;
            if (sqrDistToPoint > sqrDistToMuzzle) {
                gunController.Aim(target.position);
            }
        }
    }

    protected override void Start() {
        base.Start();
        gunController = gameObject.GetComponent<GunController>();
        gun = gunController.GetEquipped();
        if (gun != null) {
            gun.LoadGun();
        }
        Vector3 muzzlePos = gunController.MuzzlePosition();
        sqrDistToMuzzle = (new Vector2(muzzlePos.x, muzzlePos.z)
            - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude;
    }

    bool hasClearLOS() {
        if (hasTarget) {
            Ray ray = new Ray(transform.position, transform.forward);
            //MARK FOR FUTURE CONCERN -> NON ZERO Y
            float distToPlayer = (target.position - transform.position).magnitude;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distToPlayer,
                collisionMask, QueryTriggerInteraction.Collide)) {
                return false;
            }
            return true;
        }
        return false;
    }

    public override IEnumerator Attack() {
        if (gun != null) {
            if (player.GetPlayerController().GetState() == PlayerController.State.Invincible || !hasClearLOS()) {
                yield break;
            }
            currentState = State.Attacking;
            gun.LoadGun();
            while (gun.GetBulletsRemaining() > 0 && hasClearLOS()) {
                gunController.Shoot();
                yield return null;
            }
            gun.LoadGun();
            nextAttackTime = Time.time + timeBetweenAttacks;
            currentState = State.Moving;
        }
    }

    public override IEnumerator UpdatePath() {
        float updateRate = 0.25f;

        while (hasTarget) {
            if (!dead && agent.enabled) {
                if ((target.position - transform.position).sqrMagnitude < Mathf.Pow(stoppingDist, 2) && hasClearLOS()) {
                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    agent.SetDestination(target.position - dirToTarget * stoppingDist);
                } else {
                    agent.SetDestination(target.position);
                }
            }
            yield return new WaitForSeconds(updateRate);
        }
    }

}
