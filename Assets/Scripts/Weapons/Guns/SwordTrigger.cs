using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrigger : MonoBehaviour
{
    Sword parent;

    void Start() {
        parent = transform.parent.GetComponent<Sword>();
    }

    void OnTriggerEnter(Collider c) {
        if (parent != null) {
            parent.OnChildTriggerEnter(c);
        }
    }
}
