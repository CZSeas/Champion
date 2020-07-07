using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int price;
    public enum Type {
        Item,
        Gun,
        Drop
    };
    public Type type;
    public GameObject item;

    public string PriceString() {
        return String.Format("Press F to Buy (Δ{0})", price);
    }

    void OnDisable() {
        UIController.instance.HideBottomText();
    }
}
