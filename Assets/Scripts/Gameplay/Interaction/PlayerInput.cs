using System;
using System.Collections.Generic;
using System.Numerics;
using Libraries;
using Systems.Physics;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;


namespace Gameplay.Interaction
{
    public class PlayerInput : MonoBehaviour
    {
        private Vector3 _mousePositionOffset;
        private List<Vector3> _hoveredBuffer;
        private RayCastResult _hitResult;
        private int _grabbedParticle;

        private bool _bIsMouseDown = false;
        
        [HideInInspector] public UnityEvent<int> onGrabbed;
        [HideInInspector] public UnityEvent<int, List<Vector3>> onContextAction;
        [HideInInspector] public UnityEvent<int> onReleased;
        
        private Camera _cachedCamera;
        private PhysicsSimulator _physicsSimulator;
        void Awake()
        {
            _hoveredBuffer = new List<Vector3>();
            _cachedCamera = Camera.main;
        }
        void Start()
        {
            _physicsSimulator = GameManager.Instance.physicsSimulator;
            _hitResult =  new(-1, Vector3.zero, Vector3.zero);
        }

        void Update()
        {
            Vector3 viewportPoint = _cachedCamera.ScreenToViewportPoint(Input.mousePosition);
            Ray ray = _cachedCamera.ViewportPointToRay(viewportPoint);
            
            RayCastResult hitResult = _physicsSimulator.RayCast(ray.origin, ray.direction);
            

            if (hitResult.particleIndex != _hitResult.particleIndex)
            {
                _hoveredBuffer.Clear();
            }
            
            _hitResult = hitResult;
            
            UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction*1000, Color.red, 0.1f);
            
 
            UpdateInput(ray);
 
        }

        private void UpdateInput( Ray ray)
        {
            if (_hitResult.bHit)
                _hoveredBuffer.Add(_hitResult.mousePointInParticlePlane);
            
            if (Input.GetKey(KeyCode.Mouse1))
            {
                
                if (_hitResult.bHit == false)
                    return;
                ContextAction();
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (_bIsMouseDown == false)
                {
                    if (_hitResult.bHit == false)
                        return;
                   // Debug.Log("Mouse Down");
                    MouseDown();
                }
                else
                {
                    MouseDrag(ray);
                }
            }
            else if (_bIsMouseDown)
            {
               // Debug.Log("Mouse Up");
                MouseUp();
            }
        }

        void ContextAction() 
        {
            

            onContextAction.Invoke(_hitResult.particleIndex, _hoveredBuffer);
        }
        void MouseDrag( Ray ray)
        {
            Particle particle = _physicsSimulator.GetParticle(_grabbedParticle);
            Vector3 mousePointInParticlePlane = MathFunctions.RayHorizontalPlaneIntersection(ray.origin, ray.direction, particle.position.y);
            Vector3 newPosition =  mousePointInParticlePlane + _mousePositionOffset;
            
            Vector3 clampedPosition = new Vector3(newPosition.x, particle.position.y,newPosition.z);
            _physicsSimulator.SetParticlePosition(_grabbedParticle, clampedPosition);
            
        }
        void MouseDown()
        {
            _bIsMouseDown = true;
            Particle particle = _physicsSimulator.GetParticle(_hitResult.particleIndex);
            Vector3 position = particle.position - _hitResult.hitPoint;
            Vector3 clampedPosition = new Vector3(position.x,0,position.z);
            _mousePositionOffset = clampedPosition;
            _grabbedParticle = _hitResult.particleIndex;
            onGrabbed.Invoke(_hitResult.particleIndex);
        }   
        
        void MouseUp()
        {
            _bIsMouseDown = false;
            onReleased.Invoke(_grabbedParticle);
            _hoveredBuffer.Clear();
            _grabbedParticle = -1;
        }
    }
}