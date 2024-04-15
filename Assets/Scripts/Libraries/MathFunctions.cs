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
    }
}