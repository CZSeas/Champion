using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : Entity
{
    public enum State {
        Idle,
        Moving,
        Attacking,
        BeingKnocked
    };
    protected State currentState;


    public LayerMask collisionMask;
    public NavMeshAgent agent;

    protected float collisionRadius;
    public float stoppingDist;

    protected Player player;
    protected Transform target;
    protected bool hasTarget;

    public ParticleSystem deathEffect;
    protected Material thisMat;

    public float height;
    public float knockForce = 25;
    public float knockDuration = 0.1f;

    public int attackDamage = 1;
    public float attackSpeed = 3;
    public float attackRange = 3;
    public float timeBetweenAttacks = 0.5f;
    protected float nextAttackTime;

    public int tier = 1;

    public void SetNextAttackTime() {
        nextAttackTime = Time.time + timeBetweenAttacks;
    }

    protected override void Start() {
        base.Start();
        agent = gameObject.GetComponent<NavMeshAgent>();
        if (GetComponent<CapsuleCollider>() != null) {
            collisionRadius = GetComponent<CapsuleCollider>().radius;
        }
        thisMat = GetComponent<Renderer>().material;

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            currentState = State.Moving;
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            player = target.GetComponent<Player>();
            player.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
    }

    public void RestartPath() {
        if (GameObject.FindGameObjectWithTag("Player") != null) {
            currentState = State.Moving;
            hasTarget = true;
            StartCoroutine(UpdatePath());
        }
    }

    protected virtual void FixedUpdate() {
        if (hasTarget) {
            if (Time.time > nextAttackTime && currentState != State.Attacking) {
                float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistToTarget < Mathf.Pow(attackRange, 2)) {
                    StartCoroutine(Attack());
                }
            }
            transform.LookAt(target.position);
        }
    }

    protected void OnTargetDeath(Transform playerTransform) {
        hasTarget = false;
        currentState = State.Idle;
    }

    public override void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 force, float hitSpeed) {
        if (damage >= health) {
            deathEffect.startSpeed = hitSpeed * 0.5f;
            deathEffect.gameObject.GetComponent<ParticleSystemRenderer>().material = thisMat;
            Destroy(Instantiate(deathEffect.gameObject, hitPoint,
                Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection, force, hitSpeed);
        StartCoroutine(KnockBack(force));
    }

    protected Vector3 CheckCollisions(Vector3 velocity) {
        float moveDist = velocity.magnitude * Time.fixedDeltaTime;
        Vector3 xDir = new Vector3(velocity.normalized.x, 0, 0);
        Vector3 yDir = new Vector3(0, 0, velocity.normalized.z);
        Vector3 checkVelocity = velocity;
        Ray rayX = new Ray(transform.position + xDir.normalized * collisionRadius, xDir);
        Ray rayY = new Ray(transform.position + yDir.normalized * collisionRadius, yDir);
        RaycastHit hit;
        if (Physics.Raycast(rayX, out hit, moveDist,
            collisionMask, QueryTriggerInteraction.Collide)) {
            checkVelocity.x = 0;
        }
        if (Physics.Raycast(rayY, out hit, moveDist,
            collisionMask, QueryTriggerInteraction.Collide)) {
            checkVelocity.z = 0;
        }
        return checkVelocity;
    }

    void OnTriggerStay(Collider other) {
        if (other.tag != "Player") {
            return;
        }
        Vector3 pushDir = other.transform.position - transform.position;
        pushDir.y = 0;
        if (pushDir.magnitude == 0) {
            float randomPush = Random.Range(0f, 1f);
            pushDir = new Vector3(randomPush, 0, 1 - randomPush);
        }
        player.TakeHit(attackDamage, transform.position, pushDir,
            pushDir.normalized * knockForce, agent.velocity.magnitude);
    }

    public IEnumerator KnockBack(Vector3 force) {
        float knockTime = Time.time + knockDuration;
        currentState = State.BeingKnocked;
        agent.enabled = false;
        while (Time.time < knockTime) {
            Vector3 velocity = CheckCollisions(force);
            transform.position = transform.position + velocity * Time.deltaTime;
            yield return null;
        }
        currentState = State.Moving;
        agent.enabled = true;
    }


    public abstract IEnumerator UpdatePath();

    public abstract IEnumerator Attack();
}
