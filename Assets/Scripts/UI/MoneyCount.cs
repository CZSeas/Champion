using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyCount : MonoBehaviour
{
    Player player;
    public Text money;

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    void Update() {
        money.text = "Δ " + player.GetCurrentMoney().ToString();
    }
}
