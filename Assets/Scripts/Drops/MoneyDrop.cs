using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyDrop : Drop
{
    int amount = 5;

    public void SetAmount(int amount) {
        this.amount = amount;
    }

    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        Player player = c.GetComponent<Player>();
        player.IncreaseMoney(amount);
        Destroy(gameObject);
    }
    
}
