using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
    public static class MathFunctions 
    {
        public static float RaySphereIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 spherePosition,
            float sphereRadius)
        {
            
            Vector3 rayToSphere = spherePosition - rayOrigin;

            float a = Vector3.Dot(rayDirection, rayDirection);
            float b = 2.0f * Vector3.Dot(rayToSphere, rayDirection);
            float c = Vector3.Dot(rayToSphere,rayToSphere) - sphereRadius*sphereRadius;
            float discriminant = b*b - 4*a*c;
            if(discriminant < 0)
                return -1;
            
            float result = (-b - Mathf.Sqrt(discriminant)) / (2.0f*a);
            return Mathf.Abs(result);
        }

        public static Vector3 RayHorizontalPlaneIntersection(Vector3 rayOrigin, Vector3 rayDirection, float positionY)
        {
            float t = (positionY - rayOrigin.y) / rayDirection.y;
            return rayOrigin + t * rayDirection;
        }
        
        public static Vector3 DeterminePredominantDirection(List<Vector3> positions, float minDistance = 0)
        {
            if(positions.Count < 2)
                return Vector3.zero;

            Vector3 movement = Vector3.zero;
            Vector3 lastPosition = positions[positions.Count - 1];
            for(int i = positions.Count - 2; i >= 0; i--)
            {
                Vector3 currentPosition = positions[i];
                Vector3 offset = lastPosition - currentPosition;
                if (offset.magnitude >= minDistance)
                {
                    movement = offset;
                    break;
                }
            }

            return movement;
        }
        
        public static Axis DeterminePredominantAxis(Vector3 movement)
        {
            float angleWithForward =  Vector3.Angle(Vector3.forward, movement);        
            float angleWithBack = Vector3.Angle(Vector3.back, movement);
            Axis cutAxis = angleWithBack < 45 || angleWithForward < 45 ? Axis.Vertical : Axis.Horizontal;
            return cutAxis;
        }

    }
}