using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;


public class Brian : Enemy
{
    [Header("References")]
    public Material hitMat;
    public GameObject circleAttackPrefab;
    public GunController[] gunControllers;
    public Animator animator;

    [Header("Parameters")]
    public float sevenAttackDuration = 7f;
    public float lSpamAttackDuration = 7f;
    public float circleAttackDuration = 7f;
    public float circleAttackInterval = 1.3f;
    public float timeToWake = 2.5f;

    [Header("Name")]
    public string name = "Brian";

    bool awake = false;

    protected override void Start() {
        health = maxHealth;
        agent = gameObject.GetComponent<NavMeshAgent>();
        collisionRadius = 2.5f;
        thisMat = hitMat;
        if (UIController.instance != null) {
            UIController.instance.EnableHealthBar(name, maxHealth);
        }
        StartCoroutine(FindPlayer());
    }

    IEnumerator WakeUp() {
        yield return new WaitForSeconds(timeToWake);
        awake = true;
        StartCoroutine(UpdatePath());
    }

    IEnumerator FindPlayer() {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        currentState = State.Moving;
        hasTarget = true;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        player = target.GetComponent<Player>();
        player.OnDeath += OnTargetDeath;
        StartCoroutine(WakeUp());
    }

    protected override void FixedUpdate() {
        if (hasTarget && awake) {
            base.FixedUpdate();
            foreach (GunController gunController in gunControllers) {
                gunController.Aim(target.position);
            }
        }
    }

    public override IEnumerator Attack() {
        currentState = State.Attacking;
        animator.SetBool("LSpam", true);
        float attackTimer = 0;
        while (attackTimer < sevenAttackDuration) {
            foreach (GunController gunController in gunControllers) {
                gunController.Shoot();
            }
            attackTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(timeBetweenAttacks);
        animator.SetBool("LSpam", false);
        StartCoroutine(CircleAttack());
    }

    IEnumerator CircleAttack() {
        currentState = State.Attacking;
        float attackTimer = 0;
        float circleAttackTime = 0;
        while (attackTimer < circleAttackDuration) {
            if (attackTimer >= circleAttackTime) {
                Destroy(Instantiate(circleAttackPrefab,
                    new Vector3(transform.position.x, 1, transform.position.z), Quaternion.Euler(-90, 0, 0)), 10);
                circleAttackTime += circleAttackInterval;
            }
            attackTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(timeBetweenAttacks);
        StartCoroutine(LAttack());
    }

    IEnumerator LAttack() {
        currentState = State.Attacking;
        animator.SetBool("LSpam", true);
        foreach (GunController gunController in gunControllers) {
            Gun gun = gunController.GetEquipped();
            gun.msBetweenShots = 25;
            gun.bulletSpeed = 12;
            gun.spread = 5;
        }
        float attackTimer = 0;
        Vector3 maxAngle = new Vector3(0, 60, 0);
        Vector3 minAngle = new Vector3(0, -60, 0);
        while (attackTimer < lSpamAttackDuration) {
            for (int i = 0; i < gunControllers.Length; i++) {
                Gun gun = gunControllers[i].GetEquipped();
                if (i == 1) {
                    gun.muzzle[0].localEulerAngles = Vector3.Lerp(minAngle, maxAngle,
                        Mathf.Abs(Mathf.Sin(Mathf.PI * attackTimer / 3f)));
                } else {
                    gun.muzzle[0].localEulerAngles = Vector3.Lerp(maxAngle, minAngle,
                        Mathf.Abs(Mathf.Sin(Mathf.PI * attackTimer / 3f)));
                }
                gunControllers[i].Shoot();
            }
            foreach (GunController gunController in gunControllers) {
                gunController.Shoot();
            }
            attackTimer += Time.deltaTime;
            yield return null;
        }
        foreach (GunController gunController in gunControllers) {
            Gun gun = gunController.GetEquipped();
            gun.msBetweenShots = 100;
            gun.bulletSpeed = 40;
            gun.spread = 10;
        }
        nextAttackTime = Time.time + 3 * timeBetweenAttacks;
        animator.SetBool("LSpam", false);
        currentState = State.Moving;
    }

    public override IEnumerator UpdatePath() {
        float updateRate = 0.25f;
        while (hasTarget) {
            if (!dead && agent.enabled) {
                if ((target.position - transform.position).sqrMagnitude < Mathf.Pow(stoppingDist, 2)) {
                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    agent.SetDestination(target.position - dirToTarget * stoppingDist);
                } else {
                    agent.SetDestination(target.position);
                }
            }
            yield return new WaitForSeconds(updateRate);
        }
    }

    public override void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 force, float hitSpeed) {
        if (damage >= health) {
            deathEffect.startSpeed = hitSpeed * 0.5f;
            deathEffect.gameObject.GetComponent<ParticleSystemRenderer>().material = thisMat;
            UIController.instance.DisableHealthBar();
            Destroy(Instantiate(deathEffect.gameObject, hitPoint,
                Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.startLifetime);
        }
        health -= damage;
        UIController.instance.SetHealth(health);
        if (health <= 0 && !dead) {
            Die();
        }
    }

}
