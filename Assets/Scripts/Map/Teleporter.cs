using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public static event Action Teleporting;

    Vector3 destination;

    public void SetDestination(Vector3 position) {
        destination = new Vector3(position.x, 1, position.z);
    }

    protected virtual void OnTriggerEnter(Collider player) {
        if (player.tag != "Player") {
            return;
        }
        if (destination != null) {

            //PROBABLY FINE?
            TrailRenderer[] allTrails = GameObject.FindObjectsOfType<TrailRenderer>();
            foreach (TrailRenderer trail in allTrails) {
                trail.Clear();
            }
            ParticleSystem[] allParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>();
            foreach (ParticleSystem particleSystem in allParticleSystems) {
                particleSystem.Clear();
            }

            player.GetComponent<Player>().cape.SetActive(false);
            player.transform.position = destination;
            Teleporting();
            player.GetComponent<Player>().cape.SetActive(true);
        }
    }
}
