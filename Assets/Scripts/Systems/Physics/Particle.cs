using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
/*
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Draggable))]
*/

public class Particle
{
    public Vector3 position;
    public int meshIndex;
    public Vector2Int meshCoords;
    public bool isLocked;
    public bool bWasCut;    

    public Particle(Vector3 position, int meshIndex,Vector2Int meshCoords)
    {
        this.position = position;
        this.meshIndex = meshIndex;
        this.meshCoords = meshCoords;
        isLocked = false;
        bWasCut = false;
    }
    
    public static bool operator ==(Particle a, Particle b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        return a.meshIndex == b.meshIndex;
    }
    public static bool operator !=(Particle a, Particle b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        return a.meshIndex != b.meshIndex;
    }


}
