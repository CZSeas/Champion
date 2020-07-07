using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public float itemDuration;
    public float itemCooldown;
    protected float itemCooldownTime;
    protected ItemController itemController;
    public ItemDrop itemDrop;

    protected virtual void Awake() {
        itemController = GameObject.FindWithTag("Player").GetComponent<ItemController>();
    }

    public void Use() {
        StartCoroutine(UseItem());
    }

    public abstract IEnumerator UseItem();
}
