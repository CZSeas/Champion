using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    protected override void CheckCollisions(float moveDist) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, moveDist + hitDistCompensator,
            collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitStick(hit.collider, hit.point, hit.normal);
        }
    }

    void OnHitStick(Collider c, Vector3 hitPoint, Vector3 hitNormal) {
        enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        if (c.GetComponent<IDamageable>() != null || c.gameObject.tag == "PhysicalWeapon") {
            transform.parent = c.transform;
        }
        OnHitObject(c, hitPoint, hitNormal);
        Destroy(gameObject, 2);
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            Renderer hitRenderer = c.GetComponent<Renderer>();
            if (hitRenderer != null) {
                MaterialPropertyBlock hitBlock = new MaterialPropertyBlock();
                ParticleSystem.MainModule main = hitEffect.main;
                hitRenderer.GetPropertyBlock(hitBlock);
                main.startColor = hitBlock.GetColor("_BaseColor");
            }
            Instantiate(hitEffect, transform.position, Quaternion.FromToRotation(Vector3.forward, -transform.forward));
            enabled = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Destroy(gameObject, 2);
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
    }
}
