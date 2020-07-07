using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomText : MonoBehaviour
{
    public GameObject textPanel;
    public Text centerText;

    public void DisplayBottomText(string text) {
        textPanel.SetActive(true);
        centerText.gameObject.SetActive(true);
        centerText.text = text;
    }

    public void HideBottomText() {
        textPanel.SetActive(false);
        centerText.gameObject.SetActive(false);
    }
}
