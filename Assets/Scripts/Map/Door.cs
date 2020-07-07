using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator animator;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        if (animator == null) {
            animator = GetComponent<Animator>();
        }
    }

    public void OpenDoor() {
        if (animator != null) {
            animator.Play("Door");
        }
    }

    public void CloseDoor() {
        if (animator != null) {
            animator.Play("CloseDoor");
        }
    }

}
