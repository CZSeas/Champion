using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : Drop
{
    public Item item;

    void OnCollisionEnter(Collision collision) {
        Collider c = collision.collider;
        if (c.tag != "Player") {
            return;
        }
        ItemController itemController = c.GetComponent<ItemController>();
        itemController.EquipItem(item);
        Destroy(gameObject);
    }
}
