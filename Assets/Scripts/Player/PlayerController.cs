using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum State {
        Normal,
        Invincible,
        Dead
    };

    State currentState;
    bool dashing = false;

    public State GetState() {
        return currentState;
    }

    public GameObject dashTrail;
    public float collisionRadius;

    Rigidbody playerRB;
    Vector3 velocity;
    public float moveSpeed = 10;
    float knockDuration = 0.1f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1.25f;
    float dashCooldownTime;
    public float dashModifier = 4.0f;
    GameObject trail;

    float invincibleDuration = 1f;
    Color initialColor;


    void Start() {
        collisionRadius = GetComponent<CapsuleCollider>().radius;
        playerRB = gameObject.GetComponent<Rigidbody>();
        initialColor = GetComponent<Renderer>().material.color;
        currentState = State.Normal;
    }

    public void setVelocity(Vector3 direction) {
        this.velocity = direction * moveSpeed;
    }

    //KNOCKBACK

    public IEnumerator KnockBack(Vector3 force) {
        if (currentState != State.Invincible) {
            StartCoroutine(StartInvincible());
            float knockTimer = 0;
            while (knockTimer < knockDuration) {
                playerRB.velocity = force;
                knockTimer += Time.deltaTime;
                yield return null;
            }
        } 
    }

    //DASH

    public IEnumerator Dash() {
        if (Time.time > dashCooldownTime) {
            State prevState = currentState;
            currentState = State.Invincible;
            dashing = true;
            SetTrail(4 * dashDuration);
            float dashTimer = 0;
            Vector3 prevVelocity = velocity;
            if (prevVelocity.magnitude == 0) {
                prevVelocity = transform.forward * moveSpeed;
            }
            while (dashTimer < dashDuration) {
                if (velocity.magnitude > 0) {
                    prevVelocity = velocity;
                }
                playerRB.velocity = dashModifier * prevVelocity;
                dashTimer += Time.deltaTime;
                yield return null;
            }
            dashCooldownTime = Time.time + dashCooldown;
            currentState = State.Normal;
            dashing = false;
        }
    }

    public bool IsDashing() {
        return dashing;
    }

    public void SetTrail(float duration) {
        trail = Instantiate(dashTrail, playerRB.position, Quaternion.identity) as GameObject;
        trail.transform.parent = playerRB.transform;
        Destroy(trail, duration);
    }

    public GameObject GetTrail() {
        return trail;
    }

    //INVINCIBLE

    public IEnumerator StartInvincible() {
        if (currentState != State.Invincible) {
            currentState = State.Invincible;
            //Physics.IgnoreLayerCollision(9, 11, true);
            //Physics.IgnoreLayerCollision(9, 16, true);
            Material playerMat = GetComponent<Renderer>().material;
            Color flashColor = Color.cyan;
            float invincibleTimer = 0;
            float flashSpeed = 4;
            while (invincibleTimer < invincibleDuration) {
                playerMat.SetColor("_BaseColor",
                    Color.Lerp(initialColor, flashColor, Mathf.PingPong(invincibleTimer * flashSpeed, 1)));
                invincibleTimer += Time.deltaTime;
                if (!dashing) {
                    playerRB.velocity = velocity;
                }
                yield return null;
            }
            playerMat.color = initialColor;
            //Physics.IgnoreLayerCollision(9, 11, false);
            //Physics.IgnoreLayerCollision(9, 16, false);
            currentState = State.Normal;
        }
    }

    public void lookAt(Vector3 point) {
        Vector3 correctedPoint = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(correctedPoint);
    }

    void FixedUpdate() {
        if (currentState == State.Normal && !dashing) {
            playerRB.velocity = velocity;
        }
    }

}
