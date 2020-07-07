using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spawn))]
public class RoomGen : MonoBehaviour
{
    [Header("References")]
    public Transform tilePrefab;
    public Transform ceilPrefab;
    public Transform obstaclePrefab;
    public Transform outerWallPrefab;
    public Teleporter teleporterPrefab;
    public Teleporter sceneTeleporterPrefab;
    public Door doorPrefab;

    [Header("External Access")]
    public Door nextDoor;
    public Door prevDoor;
    public Teleporter nextTeleporter;
    public Spawn spawner;

    List<Coord> tileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    [Header("Parameters")]
    public Vector2 roomSize;
    public float tileSize = 1;

    public Dictionary<int, Coord> Door;
    public Coord nextDoorCoord;
    public Coord prevDoorCoord;

    public float obstaclePercent;
    public float minObstacleHeight;
    public float maxObstacleHeight;
    public float obstacleSize = 1;

    public float wallThickness = 2;
    public Vector2 ceilSize;

    
    public int seed = 100;
    

    public bool isStart = false;
    public bool isEnd = false;

    void Awake() {
        spawner = GetComponent<Spawn>();
    }

    public void FindDoors() {
        Door = new Dictionary<int, Coord> {
            {4, new Coord((int) (roomSize.x / 2), (int) roomSize.y - 1)},
            {1, new Coord(0, (int) (roomSize.y / 2))},
            {3, new Coord((int) roomSize.x - 1, (int) (roomSize.y / 2))},
            {2, new Coord((int) (roomSize.x / 2), 0)},
        };
    }

    public void GenerateRoom() {
        System.Random rng = new System.Random(seed);
        
        float roomHueMin = Random.Range(0, 0.85f);
        float roomHueMax = roomHueMin + 0.15f;

        //SET ROOM COLLIDERS

        GetComponent<BoxCollider>().size = new Vector3(roomSize.x * tileSize, 0.05f, roomSize.y * tileSize);

        Vector3 lrSize = new Vector3(wallThickness, maxObstacleHeight, roomSize.y * tileSize + 2 * wallThickness);
        Vector3 tdSize = new Vector3(roomSize.x * tileSize + 2 * wallThickness, maxObstacleHeight, wallThickness);

        for (int i = -1; i <= 1; i += 2) {
            Vector3 lrWallPos = new Vector3((transform.position.x + i * (roomSize.x * tileSize + wallThickness) / 2),
                maxObstacleHeight / 2, transform.position.z);
            Transform lrWall = Instantiate(outerWallPrefab, lrWallPos, Quaternion.identity) as Transform;
            lrWall.localScale = lrSize;
            lrWall.parent = transform;
            Vector3 lrCeilPos = new Vector3(lrWallPos.x + i * (ceilSize.x + wallThickness) / 2, maxObstacleHeight, lrWallPos.z);
            Transform lrCeil = Instantiate(ceilPrefab, lrCeilPos, Quaternion.Euler(Vector3.right * 90)) as Transform;
            lrCeil.localScale = new Vector3(ceilSize.x, roomSize.y * tileSize + 2 * (ceilSize.y + wallThickness), 1);
            lrCeil.parent = lrWall.transform;
        }
        for (int i = -1; i <= 1; i += 2) {
            Vector3 tdWallPos = new Vector3(transform.position.x, maxObstacleHeight / 2,
            transform.position.z + i * (roomSize.y * tileSize + wallThickness) / 2);
            Transform tdWall = Instantiate(outerWallPrefab, tdWallPos, Quaternion.identity) as Transform;
            tdWall.localScale = tdSize;
            tdWall.parent = transform;
            Vector3 tdCeilPos = new Vector3(tdWallPos.x, maxObstacleHeight, tdWallPos.z + i * (ceilSize.y + wallThickness) / 2);
            Transform tdCeil = Instantiate(ceilPrefab, tdCeilPos, Quaternion.Euler(Vector3.right * 90)) as Transform;
            tdCeil.localScale = new Vector3(roomSize.x * tileSize + 2 * wallThickness, ceilSize.y, 1);
            tdCeil.parent = tdWall.transform;
        }


        tileMap = new Transform[(int) roomSize.x, (int) roomSize.y];
        tileCoords = new List<Coord>();
        for (int x = 0; x < roomSize.x; x++) {
            for (int y = 0; y < roomSize.y; y++) {
                tileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(tileCoords.ToArray(), seed));

        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform roomHolder = new GameObject(holderName).transform;
        roomHolder.parent = transform;

        //TILE GEN

        MaterialPropertyBlock tileBlock = new MaterialPropertyBlock();

        for (int x = 0; x < roomSize.x; x++) {
            for (int y = 0; y < roomSize.y; y++) {
                Vector3 tilePos = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePos,
                    Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * tileSize;
                newTile.parent = roomHolder;
                tileBlock.SetColor("_BaseColor",
                    Random.ColorHSV(roomHueMin, roomHueMax - 0.05f, 0.35f, 0.45f, 0.75f, 0.9f));
                newTile.GetComponent<Renderer>().SetPropertyBlock(tileBlock);
                tileMap[x, y] = newTile;
            }
        }


        //OBSTACLE GEN

        int obstacleCount = (int) (roomSize.x * roomSize.y * obstaclePercent);

        int obstaclesGenerated = 0;
        bool[,] obstacleMap = new bool[(int)roomSize.x, (int)roomSize.y];
        List<Coord> openCoordsList = new List<Coord>(tileCoords);
        openCoordsList.Remove(prevDoorCoord);

        MaterialPropertyBlock obstacleBlock = new MaterialPropertyBlock();

        for (int i = 0; i < obstacleCount; i++) {
            Coord randCoord = GetRandomCoord();
            obstacleMap[randCoord.x, randCoord.y] = true;
            obstaclesGenerated++;

            if (randCoord != nextDoorCoord && randCoord != prevDoorCoord && MapIsTree(obstacleMap, obstaclesGenerated)) {
                float obstacleHeight = Mathf.Lerp(minObstacleHeight, maxObstacleHeight, (float)rng.NextDouble());
                Vector3 obstaclePos = CoordToPosition(randCoord.x, randCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePos + Vector3.up * obstacleHeight/2,
                    Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3(obstacleSize, obstacleHeight, obstacleSize);
                newObstacle.parent = roomHolder;
                obstacleBlock.SetColor("_BaseColor",
                    Random.ColorHSV(roomHueMin, roomHueMax, 0.5f, 0.8f, 0.8f, 1));
                newObstacle.GetComponent<Renderer>().SetPropertyBlock(obstacleBlock);
                openCoordsList.Remove(randCoord);   
            } else {
                obstacleMap[randCoord.x, randCoord.y] = false;
                obstaclesGenerated--;
            }
        }

        //SPAWN DOORS

        prevDoor = SpawnDoors(prevDoorCoord);
        nextDoor = SpawnDoors(nextDoorCoord);

        //PROTECT SPAWN
        ProtectSpawn(openCoordsList);

        //RANDOMIZE LIST OF OPEN COORDS
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(openCoordsList.ToArray(), seed));
    }

    //SPAWN DOOR METHOD

    Door SpawnDoors(Coord doorCoord) {
        Vector3 doorPos = CoordToPosition(doorCoord.x, doorCoord.y);
        Vector3 difVector = (doorPos - transform.position).normalized;
        Transform doorChild = doorPrefab.transform.GetChild(0);
        Vector3 offset = new Vector3(difVector.x * (tileSize + doorChild.localScale.x),
            doorChild.localScale.y, difVector.z * (tileSize + doorChild.localScale.x)) / 2;
        Door door = Instantiate(doorPrefab, doorPos + offset,
            Quaternion.Euler(0, Mathf.Max(0, difVector.x * 180) + difVector.z * 90, 0)) as Door;
        door.transform.parent = transform;
        Vector3 teleporterOffset = new Vector3(difVector.x * tileSize, 0, difVector.z * tileSize) / 2;
        if (doorCoord == nextDoorCoord) {
            if (isEnd) {
                nextTeleporter = Instantiate(sceneTeleporterPrefab, doorPos + teleporterOffset,
                    Quaternion.identity) as Teleporter;
                AuxRoom.yRot = Mathf.Max(0, -difVector.z * 180) + difVector.x * 90;
            } else {
                nextTeleporter = Instantiate(teleporterPrefab, doorPos + teleporterOffset,
                    Quaternion.identity) as Teleporter;
            }
            nextTeleporter.transform.parent = transform;
            nextTeleporter.gameObject.SetActive(false);
        }
        return door;
    }

    //CHECK CONNECTIVITY

    bool MapIsTree(bool[,] obstacleMap, int obstaclesGenerated) {
        int gridX = obstacleMap.GetLength(0);
        int gridY = obstacleMap.GetLength(1);
        bool[,] discovered = new bool[gridX, gridY]; 

        Queue<Coord> q = new Queue<Coord>();
        Coord door = prevDoorCoord;
        q.Enqueue(door);
        discovered[door.x, door.y] = true;
        int tilesDiscovered = 1;

        while (q.Count > 0) {
            Coord v = q.Dequeue();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int checkX = v.x + x;
                    int checkY = v.y + y;
                    if (x == 0 || y == 0) {
                        if (checkX >= 0 && checkX < gridX && checkY >= 0 && checkY < gridY) {
                            if (!discovered[checkX, checkY] && !obstacleMap[checkX, checkY]) {
                                discovered[checkX, checkY] = true;
                                tilesDiscovered++;
                                q.Enqueue(new Coord(checkX, checkY));
                            }
                        }
                    }
                }
            }
        }
        return tilesDiscovered == gridX * gridY - obstaclesGenerated;
    }


    //UTILITIES

    public Vector3 CoordToPosition(int x, int y) {
        return new Vector3(transform.position.x + (-roomSize.x / 2 + 0.5f + x ) * tileSize, 0,
            transform.position.z + (-roomSize.y / 2 + 0.5f + y) * tileSize);
    }

    public Coord GetRandomCoord() {
        Coord randCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randCoord);
        return randCoord;
    }

    public Transform GetRandomOpenTile() {
        Coord randCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randCoord);
        return tileMap[randCoord.x, randCoord.y];
    }

    void ProtectSpawn(List<Coord> openCoordsList) {
        int doorX = prevDoorCoord.x;
        int doorY = prevDoorCoord.y;
        int gridX = tileMap.GetLength(0);
        int gridY = tileMap.GetLength(1);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) {
                    continue;
                }
                int checkX = doorX + i;
                int checkY = doorY + j;
                if (checkX >= 0 && checkX < gridX && checkY >= 0 && checkY < gridY) {
                    Coord spawnNeighbour = new Coord(checkX, checkY);
                    openCoordsList.Remove(spawnNeighbour);
                }
            }
        }
    }


    //COORD STRUCTURE

    public struct Coord {
        public int x;
        public int y;

        public Coord(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1 == c2);
        }

    }
}
