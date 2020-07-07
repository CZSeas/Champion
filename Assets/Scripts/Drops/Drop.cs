using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Drop : MonoBehaviour
{
    public float percentage;
    Rigidbody RB;
    float turnSpeed = 5;
    public int price;

    void Awake() {
        RB = GetComponent<Rigidbody>();
    }

    public void InitialForce() {
        float randomPush = Random.Range(-1f, 1f);
        float randomPushB = Random.Range(-1f, 1f);
        Vector3 pushDir = new Vector3(randomPush, 0, randomPushB).normalized;
        RB.velocity = pushDir * 5;
    }

    void Update() {
        RB.angularVelocity = Vector3.up * turnSpeed;
    }
}
