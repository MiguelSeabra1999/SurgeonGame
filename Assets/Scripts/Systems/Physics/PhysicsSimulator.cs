using System;
using System.Collections.Generic;
using Libraries;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Systems.Physics
{

    public struct RayCastResult
    {
        public bool bHit;
        public int particleIndex;
        public Vector3 hitPoint;
        public Vector3 mousePointInParticlePlane;

        public RayCastResult(int particleIndex,Vector3 hitPoint, Vector3 mousePointInParticlePlane)
        {
            bHit = particleIndex >= 0;
            this.particleIndex = particleIndex;
            this.hitPoint = hitPoint;
            this.mousePointInParticlePlane = mousePointInParticlePlane;
        }

    }
    public class PhysicsSimulator : MonoBehaviour
    {
        public float particleRadius = 1;
        public bool updatePhysics = false;
        public bool drawConstraints = true;
        public bool drawParticles = true;
        public float cachedErrorSum = 0;
        public float particleHeightDisplacementWhenGrabbed = 0.1f;
        public float minDragDistanceToCut = 0.1f;
        private  Dictionary<int, Particle> _particles;
        private Dictionary<Vector2Int, Particle> _particlesGrid;
        private Dictionary<Vector2Int, Particle> _secondaryParticlesGrid;
        private Dictionary<Particle, List<int>> _particlesConstraints;
        private Dictionary<Particle, MeshParticleInterface> _particlesToMeshInterface;
        private List<DistanceConstraint> _distanceConstraints;
        
        [HideInInspector] public UnityEvent<Particle, Vector3> onParticleCut;

        void Awake()
        {
            _particles = new Dictionary<int, Particle>();
            _distanceConstraints = new List<DistanceConstraint>();
            _particlesGrid = new Dictionary<Vector2Int, Particle>();
            _secondaryParticlesGrid = new Dictionary<Vector2Int, Particle>();
            _particlesConstraints = new Dictionary<Particle, List<int>>();
            _particlesToMeshInterface = new Dictionary<Particle, MeshParticleInterface>();
        }

        private void Start()
        {
            GameManager.Instance.playerInput.onContextAction.AddListener(OnCut);
            GameManager.Instance.playerInput.onGrabbed.AddListener(OnGrabbed);
            GameManager.Instance.playerInput.onReleased.AddListener(OnReleased);
        }

        void Update()
        {
            if (updatePhysics)
            {
                cachedErrorSum = SolveConstraints();
            }

            if (drawConstraints)
            {
                foreach (DistanceConstraint distanceConstraint in _distanceConstraints)
                {
                    distanceConstraint.DebugDraw();
                }
            }
        }

        void OnDrawGizmos()
        {
            if (drawParticles && Application.isPlaying)
            {
                foreach (var it in _particles)
                {
                    Gizmos.DrawWireSphere(it.Value.position, particleRadius);
                }
            }
            
        }
        
        public Particle GetParticle(int meshIndex)
        {
            return _particles[meshIndex];
        }
        
        public void SetParticlePosition(int meshIndex, Vector3 position)
        {
            Particle particle = _particles[meshIndex];
            particle.position = position;

            
            if(_particlesToMeshInterface.ContainsKey(particle))
                _particlesToMeshInterface[particle].OnParticlePositionUpdated(particle);
        }

        public RayCastResult RayCast(Vector3 origin, Vector3 direction)
        {
        
            direction = direction.normalized;
            float minDistance = float.MaxValue;
            int particleIndex = -1;

            foreach (Particle particle in _particles.Values)
            {
                float distance = MathFunctions.RaySphereIntersection(origin, direction, particle.position, particleRadius);
                if(distance < 0)
                    continue;
                
                if (distance < minDistance)
                {
                    particleIndex = particle.meshIndex;
                    minDistance = distance;
                }
            }

            Vector3 mousePointInParticlePlane = Vector3.zero;
            if (particleIndex != -1)
            {
                mousePointInParticlePlane = MathFunctions.RayHorizontalPlaneIntersection(origin, direction, GetParticle(particleIndex).position.y);
            }
            return new(particleIndex, origin + direction * minDistance, mousePointInParticlePlane);
        }
        
        private void OnGrabbed(int particleIndex)
        {
            Particle particle = _particles[particleIndex];
            particle.isLocked = true;
            
            if(_particlesToMeshInterface.ContainsKey(particle) == false)
                return;
            MeshParticleInterface meshInterface = _particlesToMeshInterface[particle];
            particle.position.y =  meshInterface.transform.position.y + particleHeightDisplacementWhenGrabbed;


        }

        private void OnReleased(int particleIndex)
        {
            Particle particle = _particles[particleIndex];
            particle.isLocked = false;
            
            if(_particlesToMeshInterface.ContainsKey(particle) == false)
                return;
            MeshParticleInterface meshInterface = _particlesToMeshInterface[particle];
            particle.position.y = meshInterface.transform.position.y;
        }
        
        void OnCut(int particleIndex, List<Vector3> mouseMovementBuffer)
        {
            Particle particle = _particles[particleIndex];
            if(particle.WasCut()) //TODO allow for multiple cutting
                return;
            
            Vector3 movement = MathFunctions.DeterminePredominantDirection(mouseMovementBuffer, minDragDistanceToCut);
            if(movement.magnitude == 0)
                return;
            
            Vector3 lastPosition = mouseMovementBuffer[mouseMovementBuffer.Count - 1];
            Debug.DrawLine(lastPosition, lastPosition - movement, Color.red, 5f);
        
            onParticleCut.Invoke(particle,movement);
        
            Axis cutAxis = MathFunctions.DeterminePredominantAxis(movement);

            Debug.Log(particle.meshIndex + " cut " + GridFunctions.AxisToString(cutAxis) + " " + movement.normalized);
     
            if(_particlesToMeshInterface.ContainsKey(particle))
            {
                Particle newParticle = _particlesToMeshInterface[particle].CutParticle(particle, cutAxis);
                CutParticle(particle,newParticle, cutAxis);
            }
        }



        private float SolveConstraints()
        {
            float errorSum = 0;
            foreach (DistanceConstraint distanceConstraint in _distanceConstraints)
            {
                Vector3 error = distanceConstraint.Solve();
                SetParticlePosition(distanceConstraint.particleA.meshIndex, distanceConstraint.particleA.position);
                SetParticlePosition(distanceConstraint.particleB.meshIndex, distanceConstraint.particleB.position);
                errorSum += error.magnitude;
            }
            
            return errorSum;
        }

        public void AddParticle(Particle particle, MeshParticleInterface meshParticleInterface)
        {
            _particles.Add(particle.meshIndex, particle);
            _particlesConstraints.Add(particle, new List<int>());
            if (!_particlesGrid.TryAdd(particle.meshCoords, particle))
            {
                _secondaryParticlesGrid.Add(particle.meshCoords, particle);
            }
            if(meshParticleInterface != null)
                _particlesToMeshInterface.Add(particle, meshParticleInterface);
        }

        public void AddDistanceConstraint(DistanceConstraint distanceConstraint)
        {
            _distanceConstraints.Add(distanceConstraint);
            _particlesConstraints[distanceConstraint.particleA].Add(_distanceConstraints.Count - 1);
            _particlesConstraints[distanceConstraint.particleB].Add(_distanceConstraints.Count - 1);
        }

        private void MakeDistanceConstraints(Particle mainParticle, Particle secondParticle)
        {
            float distance = Vector3.Distance(mainParticle.position,secondParticle.position);
            DistanceConstraint distanceConstraint = new(mainParticle,secondParticle, distance);
            AddDistanceConstraint(distanceConstraint);
        }

        private int FindConstraintBetweenTwoParticles(Particle particleA, Particle particleB)
        {
            for (int i = 0; i < _particlesConstraints[particleA].Count; i++)
            {
                int distanceConstraintIndex = _particlesConstraints[particleA][i];
                DistanceConstraint constraint = _distanceConstraints[distanceConstraintIndex];
                if (constraint.ContainsParticle(particleB.meshIndex))
                {
                    return distanceConstraintIndex;
                }
            }

            return -1;
        }


        public void CutParticle(Particle cutParticle, Particle replacementParticle, Axis axis)
        {
            cutParticle.UpdateCutDirections(axis);
            replacementParticle.UpdateCutDirections(axis);
            
            UpdateAxisDistanceConstraints(cutParticle, replacementParticle, axis);
            UpdateDiagonalDistanceConstraints(cutParticle, replacementParticle, axis);
        }    
        
        private void UpdateDiagonalDistanceConstraints(Particle cutParticle, Particle replacementParticle, Axis axis)
        {
            Vector2Int coords = cutParticle.meshCoords;
            Directions axisDirection = GridFunctions.GetDirectionsFromAxis(GridFunctions.GetOtherAxis(axis))[0];
            Directions[] directions = GridFunctions.GetComplexDirectionsFromBasic((BasicDirections)axisDirection);
            
            foreach(Directions complexDirection in directions)
            {
                Vector2Int neighbour = GridFunctions.GetCoordNeighbour(coords, complexDirection);
                Particle particle = _particlesGrid[neighbour];
                MigrateConstraintsToReplacementParticle(cutParticle, replacementParticle, particle);
            }
        }

        private void MigrateConstraintsToReplacementParticle(Particle cutParticle, Particle replacementParticle,
            Particle particle)
        {

            for (int i = 0; i < _particlesConstraints[particle].Count; i++)
            {
                int distanceConstraintIndex = _particlesConstraints[particle][i];
                DistanceConstraint constraint = _distanceConstraints[distanceConstraintIndex];
            
                if(constraint.ContainsParticle(cutParticle.meshIndex) == false)
                    continue;
            
                constraint.ReplaceParticle(cutParticle, replacementParticle);
                _distanceConstraints[distanceConstraintIndex] = constraint;
            }
            
        }

        private void UpdateAxisDistanceConstraints(Particle cutParticle, Particle replacementParticle, Axis axis)
        {
            Vector2Int coords = cutParticle.meshCoords;
            foreach(Directions direction in GridFunctions.GetDirectionsFromAxis(axis))
            {
                Vector2Int neighbour = GridFunctions.GetCoordNeighbour(coords, direction);
                CreateOrUpdateDistanceConstraint(cutParticle, replacementParticle, neighbour);
            }
        
        }   
        
        private void CreateOrUpdateDistanceConstraint(Particle cutParticle, Particle replacementParticle,
            Vector2Int otherParticleCoords)
        {
            bool bWasOtherParticleReplaced = _secondaryParticlesGrid.ContainsKey(otherParticleCoords);
            Particle otherParticle = bWasOtherParticleReplaced
                ? _secondaryParticlesGrid[otherParticleCoords]
                : _particlesGrid[otherParticleCoords];
            
            if(bWasOtherParticleReplaced)
            {
                int foundConstraintIndex = FindConstraintBetweenTwoParticles(cutParticle, otherParticle);
                DistanceConstraint constraint = _distanceConstraints[foundConstraintIndex];
                constraint.ReplaceParticle(cutParticle, replacementParticle);
                _distanceConstraints[foundConstraintIndex] = constraint;
            }
            else
            {
                MakeDistanceConstraints(otherParticle, replacementParticle);
            }
        }

        public void LockParticle(int index)
        {
            Particle particle = _particles[index];
            particle.isLocked = true;
            _particles[index] = particle;
        }
    }
}