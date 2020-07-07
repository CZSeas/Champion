using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject[] flashHolders;
    public Sprite[] muzzleFlashSprites;
    public SpriteRenderer[] spriteRenderers;

    public float flashTime = 0.07f;

    void Start() {
        Deactivate();
    }

    public void Activate() {
        foreach (GameObject flashHolder in flashHolders) {
            flashHolder.SetActive(true);
        }
        int randIdx = Random.Range(0, muzzleFlashSprites.Length);
        for (int i = 0; i < spriteRenderers.Length; i++) {
            spriteRenderers[i].sprite = muzzleFlashSprites[randIdx];
        }
        Invoke("Deactivate", flashTime);
    }

    public void Deactivate() {
        foreach (GameObject flashHolder in flashHolders) {
            flashHolder.SetActive(false);
        }
    }
}
