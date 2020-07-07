using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 force, float hitSpeed);

    void TakeDamage(int damage);

}
