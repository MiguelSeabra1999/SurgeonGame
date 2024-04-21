using System;
using Libraries;
using UnityEngine;

namespace Systems.Physics
{
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
        public byte cutDirections; //collects Directions
        
        public Particle(Vector3 position, int meshIndex,Vector2Int meshCoords)
        {
            this.position = position;
            this.meshIndex = meshIndex;
            this.meshCoords = meshCoords;
            isLocked = false;
            cutDirections = 0;
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


        public void UpdateCutDirections(Axis axis)
        {
            Directions[] axisDirections = GridFunctions.GetDirectionsFromAxis(axis);
            foreach (Directions direction in axisDirections)
            {
                cutDirections |= (byte)direction;
            }
        }

        public bool WasCut()
        {
            return cutDirections != 0;
        }
    }
}