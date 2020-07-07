using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : Drop
{
    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        Player player = c.GetComponent<Player>();
        if (player.GetHealth() < player.GetMaxHealth()) {
            player.SetHealth(player.GetHealth() + 1);
            Destroy(gameObject);
        }        
    }
    
}
