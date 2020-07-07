using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CokeDrop : Drop
{
    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        PlayerController playerController = c.GetComponent<PlayerController>();
        playerController.moveSpeed *= 1.1f;
        Destroy(gameObject);      
    }
    
}
