using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    Item itemA;
    Item itemB;
    bool usingItem;
    public Transform faceHold;

    public void IsUsingItem(bool usingItem) {
        this.usingItem = usingItem;
    }

    public void EquipItem(Item itemToEquip) {
        if (itemA == null) {
            itemA = itemToEquip;
            itemA = Instantiate(itemA, faceHold.position, transform.rotation) as Item;
            itemA.transform.parent = transform;
        } else if (itemB == null) {
            itemB = itemToEquip;
            itemB = Instantiate(itemB, faceHold.position, transform.rotation) as Item;
            itemB.transform.parent = transform;
        } else {
            float randomDir = Random.Range(0f, 1f);
            Vector3 randomDist = (new Vector3(randomDir, 2, 1 - randomDir)) 
                * GetComponent<PlayerController>().collisionRadius * 3;
            Destroy(Instantiate(itemA.itemDrop, transform.position + randomDist, Quaternion.identity).gameObject, 30);
            itemA = itemToEquip;
            itemA = Instantiate(itemA, faceHold.position, transform.rotation) as Item;
            itemA.transform.parent = transform;
        }
    }

    public void UseItemA() {
        if (itemA != null && !usingItem) {
            itemA.Use();
        }
    }

    public void UseItemB() {
        if (itemB != null && !usingItem) {
            itemB.Use();
        }
    }

    public void SwapItems() {
        Item temp = itemA;
        itemA = itemB;
        itemB = temp;
    }

    public Item GetItemA() {
        return itemA;
    }

    public Item GetItemB() {
        return itemB;
    }

}
