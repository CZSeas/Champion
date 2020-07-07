using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    Transform target;
    public float height;

    void Start()
    {
        StartCoroutine(FindPlayer());
    }

    IEnumerator FindPlayer() {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (target != null) {
            transform.position = new Vector3(target.position.x, height, target.position.z);
        }
    }
}
