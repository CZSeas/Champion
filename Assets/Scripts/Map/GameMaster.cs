using System.Collections;
using System;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [Header("References")]
    public LevelGen levelGenPrefab;
    public Player playerPrefab;
    Player player;
    public List<EnemyList> enemyLevels;

    [Header("Parameters")]
    public int numLevels = 5;
    public int startingNumRooms = 8;
    public bool enemiesEnabled = true;
    public Vector2 minMaxEnemyPercent = new Vector2(0.07f, 0.11f);


    bool sectionFinished = false;
    bool sceneLoaded = false;
    AsyncOperation loadedScene;
    bool isBoss = false;

    [Serializable]  
    public struct EnemyList {
        public List<Enemy> list;
    }

    /* 0 = main
     * 1 = shop
     * 2 = boss
     */
    int sceneToLoad;

    void Start() {
        DontDestroyOnLoad(gameObject);
        SceneTeleporter.EndScene += TriggerNext;
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity) as Player;
        StartCoroutine(RunGame());
    }

    IEnumerator RunGame() {
        for (int i = 1; i <= numLevels; i++) {
            int enemyListIdx = Math.Min(i - 1, enemyLevels.Count - 1);
            LevelGen.enemies = enemyLevels[enemyListIdx].list;
            //CHANGE THIS UP LATER
            LevelGen.minMaxEnemyPercent = minMaxEnemyPercent;
            LevelGen.enemiesEnabled = enemiesEnabled;

            int numRooms = startingNumRooms + (int)(5 * (Mathf.Log(i)));
            Vector2 minMaxNumShops = new Vector2(numRooms / 4, numRooms / 3);
            int numShops = Math.Max(1, UnityEngine.Random.Range((int)minMaxNumShops.x, (int)minMaxNumShops.y + 1));
            int shopInterval = numRooms / numShops;
            int intervalOffset = numRooms % numRooms;
            int minIdx = 2;
            int maxIdx = shopInterval + intervalOffset;
            int prevShopIdx = 0;
            LevelGen levelGen;
            sceneToLoad = 0;
            StartCoroutine(LoadScene());
            StartCoroutine(StartScene());
            yield return new WaitUntil(() => sceneLoaded);
            sceneLoaded = false;
            while (maxIdx <= numRooms) {
                
                int currentShopIdx = UnityEngine.Random.Range(minIdx, maxIdx);
                int currentNumRooms = currentShopIdx - prevShopIdx - 1;
                levelGen = Instantiate(levelGenPrefab, Vector3.zero, Quaternion.identity) as LevelGen;
                levelGen.numRooms = currentNumRooms;
                levelGen.Play();

                ////SHITTY SPOT FOR THIS
                //UIController.instance.gameObject.SetActive(true);

                //WAIT FOR GAME
                sectionFinished = false;
                sceneToLoad = 1;
                StartCoroutine(LoadScene());
                yield return new WaitUntil(() => sectionFinished);

                //WAIT FOR SHOP
                sectionFinished = false;
                sceneToLoad = 0;
                StartCoroutine(LoadScene());
                yield return new WaitUntil(() => sectionFinished);

                prevShopIdx = currentShopIdx;
                minIdx = maxIdx + 1;
                maxIdx += shopInterval;
            }
            sectionFinished = false;
            levelGen = Instantiate(levelGenPrefab, Vector3.zero, Quaternion.identity) as LevelGen;
            levelGen.numRooms = numRooms - prevShopIdx;
            levelGen.Play();

            //SPAWN BOSS
            sceneToLoad = 2;
            StartCoroutine(LoadScene());
            yield return new WaitUntil(() => sectionFinished);

            //QUEUE NEXT
            sectionFinished = false;
            isBoss = true;
            yield return new WaitUntil(() => sectionFinished);
            isBoss = false;
        }
    }
    
    void TriggerNext() {
        UIController.instance.FadeToNext();
        if (isBoss) {
            StartCoroutine(SwapLevel());
        } else {
            StartCoroutine(SwapScene());
        }
    }

    IEnumerator LoadScene() {
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        scene.allowSceneActivation = false;
        loadedScene = scene;
        while (scene.progress < 0.9f) {
            yield return null;
        }
    }

    IEnumerator SwapScene() {
        Scene currentScene = SceneManager.GetActiveScene();
        yield return new WaitUntil(() => UIController.instance.fullDark);
        TrailRenderer[] allTrails = GameObject.FindObjectsOfType<TrailRenderer>();
        foreach (TrailRenderer trail in allTrails) {
            trail.emitting = false;
            trail.Clear();
        }
        ParticleSystem[] allParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem particleSystem in allParticleSystems) {
            particleSystem.Pause();
            particleSystem.Clear();
        }
        loadedScene.allowSceneActivation = true;
        yield return new WaitUntil(() => loadedScene.isDone);
        Scene nextScene = SceneManager.GetSceneByBuildIndex(sceneToLoad);
        SceneManager.SetActiveScene(nextScene);

        //DIRTY DIRTY SWITCH OMEGALUL
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("MainCamera");
        CameraController.instance.SetNextCam(goArr[goArr.Length - 1].GetComponent<Camera>());
        CameraController.instance.NextCam();

        AsyncOperation clearScene = SceneManager.UnloadSceneAsync(currentScene);
        if (clearScene != null) {
            yield return new WaitUntil(() => clearScene.isDone);
        }

        player.GetComponent<Player>().cam = CameraController.instance.GetCurrentCam();
        foreach (ParticleSystem particleSystem in allParticleSystems) {
            if (particleSystem != null) {
                particleSystem.Play();
            }
        }
        sectionFinished = true;
    }

    IEnumerator SwapLevel() {
        Scene currentScene = SceneManager.GetActiveScene();
        yield return new WaitUntil(() => UIController.instance.fullDark);
        CameraController.instance.NextCam();
        AsyncOperation clearScene = SceneManager.UnloadSceneAsync(currentScene);
        if (clearScene != null) {
            yield return new WaitUntil(() => clearScene.isDone);
        }
        sectionFinished = true;
    }

    IEnumerator StartScene() {
        loadedScene.allowSceneActivation = true;
        yield return new WaitUntil(() => loadedScene.isDone);
        Scene nextScene = SceneManager.GetSceneByBuildIndex(sceneToLoad);
        SceneManager.SetActiveScene(nextScene);

        //DIRTY CAM SET PepeLaugh
        CameraController.instance.SetCurrentCam(GameObject.FindWithTag("MainCamera").GetComponent<Camera>());
        player.GetComponent<Player>().cam = CameraController.instance.GetCurrentCam();
        CameraController.instance.GetCurrentCam().enabled = true;
        sceneLoaded = true;
    }
}
