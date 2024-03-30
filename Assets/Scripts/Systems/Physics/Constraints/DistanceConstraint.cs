using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct DistanceConstraint 
{
    public Particle particleA;
    public Particle particleB;

    public float targetDistance;


    public DistanceConstraint(Particle particleA, Particle particleB, float distance) : this()
    {
        this.particleA = particleA;
        this.particleB = particleB;
        this.targetDistance = distance;
    }

    public Vector3 Solve()
    {
        Vector3 error = GetError();
        if(error.magnitude == 0)
        {
            return error;
        }


        Vector3 halfError = error * .5f;

        if(particleA.isLocked)
        {
                particleB.gameObject.transform.position += -1*error;
                particleB.OnPositionUpdated();
                return error;
        }
        if(particleB.isLocked)
        {
                particleA.gameObject.transform.position += error;
                particleA.OnPositionUpdated();
                return error;
        }

        particleA.gameObject.transform.position +=  halfError;
        particleB.gameObject.transform.position += -1 * halfError;
        
        particleA.OnPositionUpdated();
        particleB.OnPositionUpdated();

         return error;
    
    }

    public Vector3 GetError()
    {
        if(particleA.isLocked && particleB.isLocked)
            return Vector3.zero;

        Vector3 positionA = particleA.gameObject.transform.position;
        Vector3 positionB = particleB.gameObject.transform.position;
        float distance = Vector3.Distance(positionA,positionB);
        Vector3 offset = positionB - positionA;
        Vector3 direction = offset.normalized;
        if(distance <= targetDistance)
        {
            return  Vector3.zero;
        }
        float error = distance - targetDistance;
        return direction * error;
    }
    
    public bool ContainsParticle(int meshIndex)
    {
        return particleA.meshIndex == meshIndex || particleB.meshIndex == meshIndex;
    }

    public void DebugDraw()
    {

        Color color = Color.Lerp(Color.white, Color.red, GetError().magnitude/1f); 
        UnityEngine.Debug.DrawLine(particleA.gameObject.transform.position + Vector3.up*0.5f,  particleB.gameObject.transform.position + Vector3.up*0.5f, color,  0.0f,  true);
    }

    public Particle GetOtherParticle(Particle particle)
    {
        if(particleA == particle)
            return particleB;
        if(particleB == particle)
            return particleA;
        
        Debug.LogError("Missing particle");
        return null;
    }
    
    public void ReplaceParticle(Particle particle, Particle replacement)
    {
        if(particleA == particle)
            particleA = replacement;
        else if(particleB == particle)
            particleB = replacement;
    }
}
