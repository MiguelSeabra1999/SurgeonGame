using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsSimulator : MonoBehaviour
{
    public bool updatePhysics = false;
    public float cachedErrorSum = 0;
    List<Particle> Particles;
    List<DistanceConstraint> DistanceConstraints;

    void Awake()
    {
        Particles = new List<Particle>();
        DistanceConstraints = new List<DistanceConstraint>();
    }

    void Update()
    {
        if(updatePhysics)
        {
          cachedErrorSum = SolveConstraints();
        }
    }

    private float SolveConstraints()
    {
        float errorSum = 0;
        foreach(DistanceConstraint distanceConstraint in DistanceConstraints)
        {
            Vector3 error = distanceConstraint.Solve();
            errorSum += error.magnitude;
        }
        foreach(DistanceConstraint distanceConstraint in DistanceConstraints)
        {
            distanceConstraint.DebugDraw();
        }
        return errorSum;
    }

    public void AddParticle(Particle particle)
    {
        Particles.Add(particle);
    }
    public void AddDistanceConstraint(DistanceConstraint distanceConstraint)
    {
        DistanceConstraints.Add(distanceConstraint);
    }
}
