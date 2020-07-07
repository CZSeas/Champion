using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float masterVolumePercent = 1;
    [Range(0f, 1f)]
    public float sfxVolumePercent = 1;
    [Range(0f, 1f)]
    public float musicVolumePercent = 1;

    AudioSource[] musicSources;
    int activeMusicIdx;

    SoundLibrary library;
    Transform audioListener;
    Transform playerTransform;
    AudioSource audioSource;

    public static AudioManager instance;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            library = GetComponent<SoundLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        musicSources = new AudioSource[2];
        for (int i = 0; i < musicSources.Length; i++) {
            GameObject newMusicSource = new GameObject("Music Source" + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            newMusicSource.transform.parent = transform;
        }
        audioListener = transform.Find("AudioListener");
        audioSource = audioListener.gameObject.GetComponent<AudioSource>();
        audioSource.spread = 0;
        StartCoroutine(FindPlayer());
    }

    IEnumerator FindPlayer() {
        yield return new WaitUntil(() => GameObject.FindWithTag("Player") != null);
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Update() {
        if (playerTransform != null) {
            audioListener.position = playerTransform.position;
        }
        if (!musicSources[activeMusicIdx].isPlaying) {
            PlayMusic(musicSources[activeMusicIdx].clip, 0.1f);
        };
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        if (musicSources[activeMusicIdx].clip != clip || !musicSources[activeMusicIdx].isPlaying) {
            activeMusicIdx = 1 - activeMusicIdx;
            if (musicSources[activeMusicIdx].clip != clip || !musicSources[activeMusicIdx].isPlaying) {
                musicSources[activeMusicIdx].clip = clip;
                musicSources[activeMusicIdx].Play();
            }
            StartCoroutine(MusicCrossfade(fadeDuration));
        }
        
    }

    public void PlaySound(AudioClip clip) {
        if (clip != null) {
            audioSource.PlayOneShot(clip, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlaySound(string name) {
        PlaySound(library.GetClipFromName(name));
    }

    public AudioClip GetPlaying() {
        return musicSources[activeMusicIdx].clip;
    }

    IEnumerator MusicCrossfade(float duration) {
        float percent = 0;
        while (percent < 1) {
            musicSources[activeMusicIdx].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicIdx].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            percent += Time.deltaTime * 1 / duration;
            yield return null;
        }
    }

}
