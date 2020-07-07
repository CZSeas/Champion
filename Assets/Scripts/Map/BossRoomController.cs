using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BossRoomController : MonoBehaviour 
{
    public BossRoom[] bosses;
    int seed;

    public static BossRoomController instance;
    public Queue<BossRoom> shuffledBosses;

    void Start() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            seed = Random.Range(0, 1000);
            shuffledBosses = new Queue<BossRoom>(Utility.ShuffleArray(bosses, seed));
            DontDestroyOnLoad(gameObject);
        }
        StartCoroutine(SpawnBossRoom());
    }

    IEnumerator SpawnBossRoom() {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == 2);
        if (shuffledBosses.Count > 0) {
            BossRoom bossRoom = Instantiate(shuffledBosses.Dequeue(), transform.position, Quaternion.identity) as BossRoom;
        } 
    }
}