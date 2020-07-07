using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    protected Transform target;
    float roomSizeX;
    float roomSizeY;
    protected float offsetY;
    protected float offsetHeight;
    Vector3 centerPos;
    public bool doorClosed;
    bool movingToCenter;
    bool movingToPlayer;
    public float height = 25;
    public Vector2 camMargin;
    public float camMoveSpeed = 3;

    void Start() {
        DontDestroyOnLoad(gameObject);
        target = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = new Vector3(target.position.x, height, target.position.z);
        float angleRad = Mathf.Deg2Rad * transform.eulerAngles.x;
        offsetY = Mathf.Cos(angleRad) * height;
        offsetHeight = Mathf.Sin(angleRad) * height;
        doorClosed = true;
    }

    public void ChangeRooms(float roomSizeX, float roomSizeY, Vector3 centerPos) {
        this.roomSizeX = roomSizeX;
        this.roomSizeY = roomSizeY;
        this.centerPos = centerPos;
        camMargin.x = Mathf.Clamp(camMargin.x, 0, roomSizeX / 2);
        camMargin.y = Mathf.Clamp(camMargin.y, 0, roomSizeY / 2);
        StartCoroutine(MoveToCenter());
    }

    IEnumerator MoveToCenter() {
        if (target != null) {
            movingToCenter = true;
            float percent = 0;
            while (percent <= 1 && target != null) {
                float xPos = Mathf.Clamp(target.position.x, centerPos.x - roomSizeX / 2 + camMargin.x,
                    centerPos.x + roomSizeX / 2 - camMargin.x);
                float yPos = Mathf.Clamp(target.position.z, centerPos.z - roomSizeY / 2 + camMargin.y,
                    centerPos.z + roomSizeY / 2 - camMargin.y);
                float xLerp = Mathf.Lerp(target.position.x, xPos, percent);
                float yLerp = Mathf.Lerp(target.position.z, yPos, percent);
                transform.position = new Vector3(xLerp, offsetHeight, yLerp - offsetY);
                percent += Time.deltaTime * camMoveSpeed;
                yield return null;
            }
            movingToCenter = false ;
            doorClosed = true;
        }
    }

    public IEnumerator MoveToPlayer() {
        if (target != null) {
            movingToPlayer = true;
            float percent = 0;
            Vector3 startPos = transform.position;
            while (percent <= 1 && !movingToCenter && target != null) {
                float xLerp = Mathf.Lerp(startPos.x, target.position.x, percent);
                float yLerp = Mathf.Lerp(startPos.z + offsetY, target.position.z, percent);
                transform.position = new Vector3(xLerp, offsetHeight, yLerp - offsetY);
                percent += Time.deltaTime * camMoveSpeed;
                yield return null;
            }
            movingToPlayer = false ;
        }
    }

    void Update() {
        TrackPlayer();
    }

    protected virtual void TrackPlayer() {
        if (target != null) {
            if (doorClosed) {
                float xPos = Mathf.Clamp(target.position.x, centerPos.x - roomSizeX / 2 + camMargin.x,
                    centerPos.x + roomSizeX / 2 - camMargin.x);
                float yPos = Mathf.Clamp(target.position.z, centerPos.z - roomSizeY / 2 + camMargin.y,
                    centerPos.z + roomSizeY / 2 - camMargin.y);
                transform.position = new Vector3(xPos, offsetHeight, yPos - offsetY);
            }
            else if (!movingToCenter && !movingToPlayer) {
                transform.position = new Vector3(target.position.x, offsetHeight, target.position.z - offsetY);
            }
        }
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(centerPos, new Vector3(roomSizeX - 2 * camMargin.x, 1, roomSizeY - 2 * camMargin.y));
    }
}
