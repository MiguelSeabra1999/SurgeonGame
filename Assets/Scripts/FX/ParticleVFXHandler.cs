using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Particle))]
public class ParticleVFXHandler : MonoBehaviour
{
    public GameObject cutVFX;
    public Vector3 offset;
    void Start()
    {
        Particle particle = GetComponent<Particle>();
        particle.onCut.AddListener(OnCut);
    }

    private void OnCut(Vector3 cutOffset)
    {
        float angleEuler = Vector3.SignedAngle(Vector3.forward, cutOffset, Vector3.up);
        Quaternion Rotation = Quaternion.Euler(0,angleEuler, 0);
        GameObject NewObject = Instantiate(cutVFX, transform.position + offset, Rotation);
    }
}