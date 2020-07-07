using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGen))]
public class Spawn : MonoBehaviour
{
    public static event Action GoToNextRoom;

    public int maxEnemies;
    public int minEnemies;
    int numEnemies;
    int enemiesSpawned;
    int enemiesAlive;
    static Enemy[] enemies;
    RoomGen room;
    public Drop[] drops;
    public MoneyDrop moneyDrop;
    List<Drop> currentDrops = new List<Drop>();
    float totalPercent;
    bool roomStarted = false;
    bool roomCleared = false;

    void Awake() {
        room = GetComponent<RoomGen>();
        foreach (Drop drop in drops) {
            totalPercent += drop.percentage;
        }
    }

    void Update() {
        if (enemiesAlive <= 0 && enemiesSpawned > 0 && !roomCleared && roomStarted) {
            GoToNextRoom();
            roomCleared = true;
        }
    }

    public static void SetEnemies(Enemy[] enemyList) {
        enemies = enemyList;
    }

    public void SpawnEnemies() {
        roomStarted = true;
        numEnemies = UnityEngine.Random.Range(minEnemies, maxEnemies + 1);
        while (numEnemies > 0) {
            SpawnEnemy();
            enemiesSpawned++;
        }
    }

    void SpawnEnemy() {
        Transform randomTile = room.GetRandomOpenTile();
        Enemy enemyToSpawn = GetRandomEnemy();
        numEnemies -= enemyToSpawn.tier;
        Enemy newEnemy = Instantiate(enemyToSpawn, randomTile.position + Vector3.up * enemyToSpawn.height
            + UnityEngine.Random.Range(-0.5f, 0.5f) * randomTile.lossyScale, Quaternion.identity) as Enemy;
        newEnemy.OnDeath += OnEnemyDeath;
        newEnemy.transform.parent = transform;
        enemiesAlive++;
    }

    void OnEnemyDeath(Transform enemyTransform) {
        enemiesAlive--;
        if (enemiesAlive <= 0) {
            GoToNextRoom();
            roomCleared = true;

            //DROPS

            MoneyDrop currentMoneyDrop = Instantiate(moneyDrop,
                enemyTransform.position + Vector3.up * 2, Quaternion.identity) as MoneyDrop;
            currentMoneyDrop.SetAmount(UnityEngine.Random.Range(enemiesSpawned * 2, enemiesSpawned * 4));
            currentMoneyDrop.InitialForce();
            currentDrops.Add(currentMoneyDrop);

            //CHANGE TO VARIABLE SPAWN PERCENT
            if (UnityEngine.Random.Range(0f, 1f) < 0.2f) {
                Drop drop = GetRandomDrop();
                Drop currentDrop = Instantiate(drop, enemyTransform.position + Vector3.up * 2, Quaternion.identity) as Drop;
                currentDrop.InitialForce();
                currentDrops.Add(currentDrop);
            }

        }
    }

    public void ClearCurrentDrops() {
        if (currentDrops.Count > 0) {
            foreach (Drop drop in currentDrops) {
                if (drop != null) {
                    Destroy(drop.gameObject);
                }
            }
        }
        currentDrops.Clear();
    }

    Drop GetRandomDrop() {
        int idx = 0;
        float randomPercent = UnityEngine.Random.Range(0f, totalPercent);
        while (randomPercent > 0) {
            randomPercent -= drops[idx].percentage;
            idx++;
        }
        idx--;
        return drops[idx];
    }

    Enemy GetRandomEnemy() {
        return enemies[UnityEngine.Random.Range(0, enemies.Length)];
    }
}
