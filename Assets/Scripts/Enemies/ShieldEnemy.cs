using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class ShieldEnemy : BaseEnemy
{

    protected override void FixedUpdate() {
        if (hasTarget) {
            if (Time.time > nextAttackTime && currentState != State.Attacking) {
                float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistToTarget < Mathf.Pow(attackRange, 2)) {
                    StartCoroutine(Attack());
                }
            }
        }
    }

}
