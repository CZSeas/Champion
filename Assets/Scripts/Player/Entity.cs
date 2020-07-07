using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    public int maxHealth;
    protected int health;
    protected bool dead;

    public event Action<Transform> OnDeath;

    protected virtual void Start() {
        health = maxHealth;
    }

    public virtual void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 force, float hitSpeed) {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(int damage) {
        health -= damage;
        if (health <= 0 && !dead) {
            Die();
        }
    }

    public int GetHealth() {
        return health;
    }

    public int GetMaxHealth() {
        return maxHealth;
    }

    public void SetHealth(int health) {
        this.health = health;
    }

    protected void Die() {
        dead = true;
        if (OnDeath != null) {
            OnDeath(transform);
        }
        GameObject.Destroy(gameObject);
    }
}
