using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleAttack : MonoBehaviour
{
    ParticleSystem ps;
    List<ParticleCollisionEvent> collisionEvents;
    Player player;
    public float force;
    public int damage;

    void Start() {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        ParticleSystem.TriggerModule trigger = ps.trigger;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        trigger.SetCollider(0, player.gameObject.GetComponent<Collider>());
    }

    void OnParticleTrigger() {

        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        for (int i = 0; i < numEnter; i++) {
            ParticleSystem.Particle p = enter[i];
            if (player.GetPlayerController().GetState() != PlayerController.State.Invincible) {
                Vector3 direction = (player.transform.position - p.position).normalized;
                player.TakeHit(damage, p.position, direction,
                    direction * force, p.velocity.magnitude);
                p.remainingLifetime = 0f;
            }
            enter[i] = p;
        }
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    }
}
