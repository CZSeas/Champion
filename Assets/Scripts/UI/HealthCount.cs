using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthCount : MonoBehaviour
{
    Player player;
    public int maxHealth = 6;
    public List<Image> hearts = new List<Image>(3);
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;
    int health;
    bool hasHalf;

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    void Update() {
        health = player.GetHealth();
        maxHealth = player.GetMaxHealth();
        if (health % 2 == 1) {
            hasHalf = true;
        } else {
            hasHalf = false;
        }
        for (int i = 0; i < hearts.Count; i++) {
            if (i * 2 < health) {
                hearts[i].sprite = fullHeart;
            } else {
                hearts[i].sprite = emptyHeart;
            }
        }
        if (hasHalf) {
            hearts[health / 2].sprite = halfHeart;
        }
    }
}
