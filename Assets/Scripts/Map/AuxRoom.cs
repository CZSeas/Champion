using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuxRoom : MonoBehaviour
{
    public Player playerPrefab;
    public Door nextDoor;
    public Door prevDoor;
    public Transform startPos;
    public Transform room;

    public static float yRot;

    protected virtual void Start() {
        if (GameObject.FindWithTag("Player") != null) {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<Player>().cape.SetActive(false);
            player.transform.position = startPos.position;
            player.GetComponent<Player>().cape.SetActive(true);
        } else {
            Instantiate(playerPrefab, startPos.position, Quaternion.identity);
        }
        Rotate();
        AnimateDoors();
        LevelGen.yRot = yRot;
    }

    protected virtual void AnimateDoors() {
        prevDoor.CloseDoor();
        nextDoor.OpenDoor();
    }

    void Rotate() {
        room.eulerAngles = new Vector3(0, yRot, 0);
    }
}
