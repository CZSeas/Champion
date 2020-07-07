using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Gun
{
    Animator animator;
    public ParticleSystem hitEffect;
    public ParticleSystem swordTrail;
    public Collider collider;
    public GameObject dashTrail;
    Player player;

    protected override void Start() {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        collider.enabled = false;
        maxAmmo = totalAmmo;
    }

    protected override void LateUpdate() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("colliderOff")) {
            collider.enabled = false;
        }
    }

    public override IEnumerator Reload() {
        yield break;
    }

    public override void Aim(Vector3 aimPoint) {
        return;
    }

    public override void Shoot() {
        if (Time.time > nextShotTime && (bulletsRemaining > 0 || bulletsRemaining == -1) && !reloading) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            animator.SetTrigger("Attack");
            collider.enabled = true;
        }
    }

    public void OnChildTriggerEnter(Collider c) {
        Vector3 contactPoint = c.ClosestPointOnBounds(player.transform.position);
        Vector3 dirToTarget = (contactPoint - player.transform.position).normalized;
        IDamageable objectHit = c.GetComponent<IDamageable>();
        if (objectHit != null) {
            objectHit.TakeHit(damage, contactPoint, dirToTarget,
                dirToTarget * knockBackForce, bulletSpeed);
        }
        Renderer hitRenderer = c.GetComponent<Renderer>();
        if (hitRenderer != null) {
            MaterialPropertyBlock hitBlock = new MaterialPropertyBlock();
            ParticleSystem.MainModule main = hitEffect.main;
            hitRenderer.GetPropertyBlock(hitBlock);
            main.startColor = hitBlock.GetColor("_BaseColor");
        }
        Instantiate(hitEffect, contactPoint, Quaternion.FromToRotation(Vector3.forward,
            dirToTarget));
    }

    public override IEnumerator Ult() {
        //CHANGE COOLDOWN TIME TO INCREASE RATE
        if (currentState != State.Ulting && Time.time > ultCooldownTime && !reloading) {
            currentState = State.Ulting;

            ParticleSystem.EmissionModule emission = swordTrail.emission;
            emission.rateOverTime = 40;
            emission.rateOverDistance = 40;
            ParticleSystem.TrailModule particleTrail = swordTrail.trails;
            ParticleSystem.MinMaxGradient prevColorOverTrail = particleTrail.colorOverTrail;
            particleTrail.colorOverTrail = particleTrail.colorOverLifetime;

            animator.SetTrigger("Ult");
            animator.SetLayerWeight(animator.GetLayerIndex("Ult"), 1);
            transform.localPosition = Vector3.zero;
            recoilAngle = 0;
            float initialMSBetweenShots = msBetweenShots;
            float prevMoveSpeed = player.GetPlayerController().moveSpeed;
            float prevDashCooldown = player.GetPlayerController().dashCooldown;
            float prevDashModifier = player.GetPlayerController().dashModifier;
            int prevDamage = damage;
            player.GetPlayerController().moveSpeed *= 1.25f;
            player.GetPlayerController().dashModifier *= 1.5f;
            player.GetPlayerController().dashCooldown /= 3;
            msBetweenShots /= 2;
            damage = 5;

            player.GetPlayerController().SetTrail(ultDuration);
            yield return new WaitForSeconds(ultDuration);

            emission.rateOverTime = 0;
            emission.rateOverDistance = 10;
            particleTrail.colorOverTrail = prevColorOverTrail;

            msBetweenShots = initialMSBetweenShots;
            damage = prevDamage;
            player.GetPlayerController().moveSpeed = prevMoveSpeed;
            player.GetPlayerController().dashCooldown = prevDashCooldown;
            player.GetPlayerController().dashModifier = prevDashModifier;
            currentState = State.Normal;
            animator.SetTrigger("StopUlt");
            animator.SetLayerWeight(animator.GetLayerIndex("Ult"), 0);
            ultCooldownTime = Time.time + ultCooldown;
        }

    }
}
