using System.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.AI;

public class LevelGen : MonoBehaviour
{
    [Header("References")]
    public RoomGen roomPrefab;
    public NavMeshSurface navMesh;
    public static List<Enemy> enemies = new List<Enemy>();

    [Header("Parameters")]
    public int numRooms = 1;
    int roomsGenerated;
    int currentRoomIdx;
    int gridSize;
    Vector3 playerStartPos = new Vector3(0, 1, 0);
    List<RoomGen> rooms = new List<RoomGen>();
    bool[,] roomGrid;

    public static float yRot = 0;

    public Vector2 minMaxObstaclePercent = new Vector2(0.05f, 0.3f);
    public Vector2 minMaxRoomSize = new Vector2(8, 18);
    int minRoomSize, maxRoomSize;

    public static bool enemiesEnabled = true;
    public static Vector2 minMaxEnemyPercent = new Vector2(0.07f, 0.11f);

    public void Play()
    {
        gridSize = 2 * numRooms + 1;
        roomGrid = new bool[gridSize, gridSize];
        minRoomSize = (int) ((minMaxRoomSize.x - 1) / 2);
        maxRoomSize = (int) ((minMaxRoomSize.y - 1) / 2);
        Vector2 size = new Vector2(UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1,
            UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1);
        Spawn.SetEnemies(enemies.ToArray());
        GenerateLevel(Vector3.zero, size, size, numRooms, numRooms, 0);
        rooms.Reverse();
        currentRoomIdx = 0;
        rooms[rooms.Count - 1].isEnd = true;
        Spawn.GoToNextRoom += SpawnNextTeleporter;
        Teleporter.Teleporting += MoveToNextRoom;
        BuildLevel();
        navMesh.BuildNavMesh();
        MoveToNextRoom();
        if (GameObject.FindWithTag("Player") != null) {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<Player>().cape.SetActive(false);
            player.transform.position = playerStartPos;
            player.GetComponent<Player>().cape.SetActive(true);
        }
    }

    public bool GenerateLevel(Vector3 centerPos, Vector2 size, Vector2 prevSize, int x, int y, int doorNum) {
        if (roomsGenerated == numRooms) {
            return false;
        }
        roomsGenerated++;
        roomGrid[x, y] = true;
        if (doorNum == 0) {
            roomGrid[x - (int)Mathf.Sin(Mathf.Deg2Rad * yRot), y - (int)Mathf.Cos(Mathf.Deg2Rad * yRot)] = true;
        }
        RoomGen room = Instantiate(roomPrefab, centerPos, Quaternion.identity) as RoomGen;
        room.transform.parent = transform;
        room.obstaclePercent = UnityEngine.Random.Range(minMaxObstaclePercent.x, minMaxObstaclePercent.y);
        room.roomSize = size;
        room.FindDoors();
        room.seed = UnityEngine.Random.Range(0, 1000);
        if (!GenerateRandomDoor(room, size, prevSize, x, y)) {
            roomsGenerated--;
            Destroy(room.gameObject);
            return GenerateLevel(centerPos, size, prevSize, x, y, doorNum);
        } else {
            if (doorNum != 0) {
                room.prevDoorCoord = room.Door[doorNum];;
            } else {
                room.isStart = true;
                switch (yRot) {
                    case 90:
                        room.prevDoorCoord = room.Door[1];
                        break;
                    case -90:
                        room.prevDoorCoord = room.Door[3];
                        break;
                    case 0:
                        room.prevDoorCoord = room.Door[2];
                        break;
                    case 180:
                        room.prevDoorCoord = room.Door[4];
                        break;
                }
                Vector3 startDoorPos = room.CoordToPosition(room.prevDoorCoord.x, room.prevDoorCoord.y);
                playerStartPos = new Vector3(startDoorPos.x, 1, startDoorPos.z);
            }
            rooms.Add(room);
            return true;
        }
    }

    bool GenerateRandomDoor(RoomGen room, Vector2 size, Vector2 prevSize, int x, int y) {
        float rng;
        bool generated = false;
        //Top Bot
        for (int i = -1; i <= 1; i += 2) {
            rng = UnityEngine.Random.Range(0.0f, 1.0f);
            if (rng < 0.25f && CheckAvailable(x, y + i)) {
                Vector2 newSize = new Vector2(UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1,
                    UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1);
                Vector3 doorPos = room.CoordToPosition(room.Door[i + 3].x, room.Door[i + 3].y);
                Vector3 newPos = new Vector3(doorPos.x, 0,
                    doorPos.z + i * (newSize.y / 2 + prevSize.y + 0.5f) * room.tileSize);
                generated = true;
                GenerateLevel(newPos, newSize, size, x, y + i, -i + 3);
                room.nextDoorCoord = room.Door[i + 3];
                return generated;
            }
        }
        //Left Right
        for (int i = -1; i <= 1; i += 2) {
            rng = UnityEngine.Random.Range(0.0f, 1.0f);
            if (rng < 0.25f && CheckAvailable(x + i, y)) {
                Vector2 newSize = new Vector2(UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1, 
                    UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1) * 2 + 1);
                Vector3 doorPos = room.CoordToPosition(room.Door[i + 2].x, room.Door[i + 2].y);
                Vector3 newPos = new Vector3(doorPos.x + i * (newSize.x / 2 + prevSize.x + 0.5f)
                    * room.tileSize, 0, doorPos.z);
                generated = true;
                GenerateLevel(newPos, newSize, size, x + i, y, -i + 2);
                room.nextDoorCoord = room.Door[i + 2];
                return generated;
            }
        }
        return generated;
    }

    public void BuildLevel() {
        for (int i = 0; i < rooms.Count; i++) {
            RoomGen room = rooms[i];
            room.GenerateRoom();
            if (i + 1 < rooms.Count) {
                RoomGen nextRoom = rooms[i + 1];
                room.nextTeleporter.SetDestination(nextRoom.CoordToPosition(nextRoom.prevDoorCoord.x,
                    nextRoom.prevDoorCoord.y));
            }
        }
    }

    void MoveToNextRoom() {
        RoomGen currentRoom = rooms[currentRoomIdx];
        if (currentRoomIdx - 1 >= 0) {
            rooms[currentRoomIdx - 1].spawner.ClearCurrentDrops();
        }

        CameraTracker tracker = CameraController.instance.GetCurrentCam().GetComponent<CameraTracker>();
        tracker.ChangeRooms(currentRoom.roomSize.x * currentRoom.tileSize,
            currentRoom.roomSize.y * currentRoom.tileSize, currentRoom.transform.position);

        if (enemiesEnabled) {
            currentRoom.spawner.minEnemies = 1 + (int)(minMaxEnemyPercent.x * currentRoom.roomSize.x * currentRoom.roomSize.y);
            currentRoom.spawner.maxEnemies = (int)(minMaxEnemyPercent.y * currentRoom.roomSize.x * currentRoom.roomSize.y);
            currentRoom.spawner.SpawnEnemies();
        }
        if (currentRoom.prevDoor != null) {
            currentRoom.prevDoor.CloseDoor();
        }
        currentRoomIdx++;
    }

    void SpawnNextTeleporter() {
        if (currentRoomIdx > 0 && currentRoomIdx <= rooms.Count) {
            RoomGen currentRoom = rooms[currentRoomIdx - 1];
            currentRoom.nextDoor.OpenDoor();
            currentRoom.nextTeleporter.gameObject.SetActive(true);
            if (currentRoomIdx <= rooms.Count - 1) {
                RoomGen nextRoom = rooms[currentRoomIdx];
                nextRoom.prevDoor.OpenDoor();
            }
            CameraTracker tracker = CameraController.instance.GetCurrentCam().GetComponent<CameraTracker>();
            tracker.doorClosed = false;
            tracker.StartCoroutine(tracker.MoveToPlayer());
        }
    }



    bool CheckAvailable(int x, int y) {
        int closedNeighbours = 0;
        if (!(x >= 0 && x < gridSize && y >= 0 && y < gridSize)) {
            return false;
        }
        if (roomGrid[x, y]) {
            return false;
        }
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (x + i >= 0 && x + i < gridSize && y + j >= 0 && y + j < gridSize) {
                    if (i * i + j * j == 2) {
                        continue;
                    }
                    if (roomGrid[x + i, y + j]) {
                        closedNeighbours++;
                    }
                }
            }
        }
        if (closedNeighbours <= 1) {
            return true;
        }
        return false;
    }

    void OnDestroy() {
        Spawn.GoToNextRoom -= SpawnNextTeleporter;
        Teleporter.Teleporting -= MoveToNextRoom;
    }

}
