using System.Collections;
using System.Collections.Generic;
using Systems.Physics;
using UnityEngine;

[RequireComponent(typeof(PhysicsSimulator))]
public class ParticleVFXHandler : MonoBehaviour
{
    public GameObject cutVFX;
    public Vector3 offset;
    void Start()
    {
        PhysicsSimulator physicsSimulator = GetComponent<PhysicsSimulator>();
        physicsSimulator.onParticleCut.AddListener(OnCut);
    }

    private void OnCut(Particle particle, Vector3 cutOffset)
    {
        float angleEuler = Vector3.SignedAngle(Vector3.forward, cutOffset, Vector3.up);
        Quaternion Rotation = Quaternion.Euler(0,angleEuler, 0);
        GameObject NewObject = Instantiate(cutVFX, particle.position + offset, Rotation);
    }
}