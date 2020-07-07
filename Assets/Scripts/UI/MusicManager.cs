using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip gillette;
    public AudioClip[] mainThemes;
    public AudioClip wineGlass;

    public static MusicManager instance;

    void Start() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        AudioManager.instance.PlayMusic(mainThemes[Random.Range(0, mainThemes.Length)], 0.5f);
    }
}
