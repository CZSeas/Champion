using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public Image fadePlane;
    public bool fullDark = false;

    public HealthBar healthBar;

    public BottomText bottomText;

    public static UIController instance;

    void Start() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        DisableHealthBar();
        HideBottomText();
    }

    //FADE

    public void FadeToNext() {
        StartCoroutine(FadeTransition(Color.clear, Color.black, 0.5f));
    }

    IEnumerator FadeTransition(Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;
        fullDark = false;
        while (percent < 1) {
            float interpolation = 4 * (-Mathf.Pow(percent, 2) + percent);
            if (percent > 0.5) {
                fullDark = true;
            }
            fadePlane.color = Color.Lerp(from, to, interpolation);
            percent += Time.deltaTime * speed;
            yield return null;
        }
    }

    //HEALTHBAR

    public void EnableHealthBar(string name, int maxHealth) {
        healthBar.gameObject.SetActive(true);
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetName(name);
    }

    public void DisableHealthBar() {
        healthBar.gameObject.SetActive(false);
    }

    public void SetHealth(int health) {
        healthBar.SetHealth(health);
    }

    //BOTTOM TEXT

    public void DisplayBottomText(string text) {
        bottomText.DisplayBottomText(text);
    }

    public void HideBottomText() {
        bottomText.HideBottomText();
    }
}
