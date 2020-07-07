using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Shell : MonoBehaviour
{
    Rigidbody myRB;
    public float forceMin;
    public float forceMax;

    float lifetime = 1;
    float fadetime = 0.5f;

    void Awake() {
        myRB = GetComponent<Rigidbody>();
        float force = Random.Range(forceMin, forceMax);
        myRB.AddForce(transform.right * force);
        myRB.AddTorque(Random.insideUnitSphere * force);
        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1) {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }
        Destroy(gameObject);
    }
}
