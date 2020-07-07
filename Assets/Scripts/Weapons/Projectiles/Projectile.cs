using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    public ParticleSystem hitEffect;
    protected float speed;
    protected int damage;
    protected float hitDistCompensator = 0.1f;
    protected float force;

    void Start() {
        Collider[] initialCollisons = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
        if (initialCollisons.Length > 0) {
            OnHitObject(initialCollisons[0], transform.position, transform.forward);
        }
    }

    public void SetSpeed(float speed) {
        this.speed = speed;
    }

    public void SetDamage(int damage) {
        this.damage = damage;
    }

    public void SetKnockForce(float force) {
        this.force = force;
    }

    void Update()
    {
        float moveDist = speed * Time.deltaTime;
        CheckCollisions(moveDist);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    protected virtual void CheckCollisions(float moveDist) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, moveDist + hitDistCompensator, 
            collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit.collider, hit.point, hit.normal);
        }
    }

    protected virtual void OnHitObject(Collider c, Vector3 hitPoint, Vector3 hitNormal) {
        IDamageable objectHit = c.GetComponent<IDamageable>();
        if (objectHit != null) {
            objectHit.TakeHit(damage, hitPoint, transform.forward, transform.forward.normalized * force, speed);
        }
        if (c.gameObject.tag == "Player") {
            if (c.gameObject.GetComponent<PlayerController>().GetState() == PlayerController.State.Invincible) {
                return;
            }
        }
        Renderer hitRenderer = c.GetComponent<Renderer>();
        if (hitRenderer != null) {
            MaterialPropertyBlock hitBlock = new MaterialPropertyBlock();
            ParticleSystem.MainModule main = hitEffect.main;
            hitRenderer.GetPropertyBlock(hitBlock);
            main.startColor = hitBlock.GetColor("_BaseColor");
        }
        Instantiate(hitEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, -transform.forward));
        if (c.gameObject.tag == "PhysicalWeapon") {
            int enemyLayerIdx = 1 << LayerMask.NameToLayer("Enemies");
            int playerLayerIdx = 1 << LayerMask.NameToLayer("Player");
            collisionMask = collisionMask | enemyLayerIdx;
            collisionMask = collisionMask & ~playerLayerIdx;
            speed *= 1.5f;
            transform.forward = Vector3.Reflect(transform.forward, hitNormal);
            transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        } else {
            GameObject.Destroy(gameObject);
        }
    }
}
