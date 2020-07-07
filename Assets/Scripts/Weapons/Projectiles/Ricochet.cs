using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ricochet : Projectile
{
    int numBounces = 2;

    protected override void CheckCollisions(float moveDist) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, moveDist + hitDistCompensator,
            collisionMask, QueryTriggerInteraction.Collide)) {
            //DIFFERENCE
            OnHitRicochet(hit.collider, hit.point, hit.normal);
        }
    }

    void OnHitRicochet(Collider c, Vector3 hitPoint, Vector3 hitNormal) {
        if (numBounces == 0 || c.GetComponent<IDamageable>() != null) {
            OnHitObject(c, hitPoint, hitNormal);
        } else {
            Vector3 reflectVector = Vector3.Reflect(transform.forward, hitNormal);
            transform.forward = reflectVector;
            numBounces--;
        }
    }
}
