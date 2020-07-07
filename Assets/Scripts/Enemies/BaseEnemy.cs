using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class BaseEnemy : Enemy
{

    public float interpolationRate = 3;

    public override IEnumerator Attack() {
        if (player.GetPlayerController().GetState() == PlayerController.State.Invincible) {
            yield break;
        }
        currentState = State.Attacking;
        agent.enabled = false;

        Vector3 orgPos = transform.position;
        Vector3 targetPos = target.position;
        float percent = 0;

        while (percent < 1) {
            float interpolation = interpolationRate * Mathf.Pow(percent, 2);
            transform.position = Vector3.Lerp(orgPos, targetPos, interpolation);
            percent += Time.deltaTime * attackSpeed;
            yield return null;
        }
        nextAttackTime = Time.time + timeBetweenAttacks;
        currentState = State.Moving;
        agent.enabled = true;
    }

    public override IEnumerator UpdatePath() {
        //update rate in seconds
        float updateRate = 0.25f;

        while (hasTarget) {
            if (currentState == State.Moving) {
                if (!dead && agent.enabled) {
                    agent.SetDestination(target.position);
                }
            }
            yield return new WaitForSeconds(updateRate);
        }
    }

}
