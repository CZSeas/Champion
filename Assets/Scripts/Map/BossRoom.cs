using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : AuxRoom
{
    public Enemy bossPrefab;
    public GameObject teleporter;
    public Transform bossSpawnPos;

    public Vector2 roomSize = Vector3.zero;

    protected override void Start() {
        teleporter.SetActive(false);
        base.Start();
        Enemy boss = Instantiate(bossPrefab, bossSpawnPos.position,
            Quaternion.Euler(0, yRot + Mathf.Sign(yRot) * -180, 0)) as Enemy;
        boss.OnDeath += OnBossDeath;
        StartCoroutine(SetupCam());
    }

    IEnumerator SetupCam() {
        //DIRTY CHECK FeelsBadMan
        if (CameraController.instance != null) {
            yield return new WaitUntil(() => CameraController.instance.GetCurrentCam().gameObject.name == "BossCam");
            CameraTracker tracker = CameraController.instance.GetCurrentCam().GetComponent<CameraTracker>();
            if (yRot == 90 || yRot == -90) {
                tracker.ChangeRooms(roomSize.y, roomSize.x, bossSpawnPos.position);
            } else {
                tracker.ChangeRooms(roomSize.x, roomSize.y, bossSpawnPos.position);
            }
        }
    }

    //NEED TO DOUBLE CHECK THIS
    protected override void AnimateDoors() {
        prevDoor.CloseDoor();
    }

    void OnBossDeath(Transform transform) {
        teleporter.SetActive(true);
        nextDoor.OpenDoor();

        //DROP SOME SHIT HERE
    }
}
