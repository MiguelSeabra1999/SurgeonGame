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

        public RayCastResult(int particleIndex,Vector3 hitPoint)
        {
            bHit = particleIndex >= 0;
            this.particleIndex = particleIndex;
            this.hitPoint = hitPoint;
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
        
        [HideInInspector] public UnityEvent<Vector3> onParticleCut;

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

            return new(particleIndex, origin + direction * minDistance);
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
            if(particle.bWasCut) //TODO allow for multiple cutting
                return;
            if(mouseMovementBuffer.Count < 2)
                return;

            Vector3 movement = Vector3.zero;
            Vector3 lastPosition = mouseMovementBuffer[mouseMovementBuffer.Count - 1];
            for(int i = mouseMovementBuffer.Count - 2; i >= 0; i--)
            {
                Vector3 currentPosition = mouseMovementBuffer[i];
                Vector3 offset = lastPosition - currentPosition;
                if (offset.magnitude >= minDragDistanceToCut)
                {
                    movement = offset;
                    break;
                }
            }
        
            if(movement.magnitude == 0)
                return;
            Debug.DrawLine(lastPosition, lastPosition - movement, Color.red, 5f);
        
            onParticleCut.Invoke(movement);
        
            float angleWithForward =  Vector3.Angle(Vector3.forward, movement);        
            float angleWithBack = Vector3.Angle(Vector3.back, movement);
        
            particle.bWasCut = true;
            if (angleWithBack < 45 || angleWithForward < 45)
            {
                Debug.Log(particle.meshIndex + " cut vertically " + movement.normalized);
                
                if(_particlesToMeshInterface.ContainsKey(particle))
                {
                    Particle newParticle = _particlesToMeshInterface[particle].CutParticleVertical(particle);
                    CutParticleVertical(particle,newParticle);
                }
            }
            else
            {
                Debug.Log(particle.meshIndex + " cut horizontally " + movement.normalized);
                if(_particlesToMeshInterface.ContainsKey(particle))
                {
                    Particle newParticle = _particlesToMeshInterface[particle].CutParticleHorizontal(particle);
                    CutParticleHorizontal(particle,newParticle);
                }
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
    
        void MakeDistanceConstraints(Particle mainParticle, Particle secondParticle)
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


        public void CutParticleVertical(Particle cutParticle, Particle replacementParticle)
        {
            UpdateVerticalDistanceConstraints(cutParticle, replacementParticle);
            UpdateDiagonalDistanceConstraintsVertical(cutParticle, replacementParticle);
        }    
    
        public void CutParticleHorizontal(Particle cutParticle, Particle replacementParticle)
        {
            replacementParticle.bWasCut = true;
            UpdateHorizontalDistanceConstraints(cutParticle, replacementParticle);
            UpdateDiagonalDistanceConstraintsHorizontal(cutParticle, replacementParticle);
        }

        private void UpdateDiagonalDistanceConstraintsVertical(Particle cutParticle, Particle replacementParticle)
        {
            Vector2Int coords = cutParticle.meshCoords;
            Vector2Int topRight = new Vector2Int(coords.x + 1, coords.y + 1);
            Vector2Int right = new Vector2Int(coords.x + 1, coords.y);
            Vector2Int bottomRight = new Vector2Int(coords.x + 1 , coords.y - 1);
            Particle topRightParticle =  _particlesGrid[topRight]; 
            Particle rightParticle =  _particlesGrid[right];
            Particle bottomRightParticle = _particlesGrid[bottomRight];
        
            Particle[] particlesToMoveConstraint = { topRightParticle, rightParticle, bottomRightParticle};
            MigrateConstraintsToReplacementParticle(cutParticle, replacementParticle, particlesToMoveConstraint);
        }
    
        private void UpdateDiagonalDistanceConstraintsHorizontal(Particle cutParticle, Particle replacementParticle)
        {
            Vector2Int coords = cutParticle.meshCoords;
            Vector2Int topLeft = new Vector2Int(coords.x - 1, coords.y + 1);
            Vector2Int top = new Vector2Int(coords.x, coords.y + 1);
            Vector2Int topRight = new Vector2Int(coords.x + 1 , coords.y + 1);
            Particle topLeftParticle = _particlesGrid[topLeft]; 
            Particle topParticle = _particlesGrid[top];
            Particle topRightParticle = _particlesGrid[topRight];
        
            Particle[] particlesToMoveConstraint = { topLeftParticle, topParticle, topRightParticle};
            if (particlesToMoveConstraint == null) throw new ArgumentNullException(nameof(particlesToMoveConstraint));
            MigrateConstraintsToReplacementParticle(cutParticle, replacementParticle, particlesToMoveConstraint);
        }

        private void MigrateConstraintsToReplacementParticle(Particle cutParticle, Particle replacementParticle,
            Particle[] particlesToMoveConstraint)
        {
            foreach (Particle particle in particlesToMoveConstraint)
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
        }

        private void UpdateVerticalDistanceConstraints(Particle cutParticle, Particle replacementParticle)
        {
            Vector2Int coords = cutParticle.meshCoords;
        
            Vector2Int top = new Vector2Int(coords.x  , coords.y + 1);
            Vector2Int bottom = new Vector2Int(coords.x  , coords.y - 1);
        
            CreateOrUpdateDistanceConstraint(cutParticle, replacementParticle, top);
            CreateOrUpdateDistanceConstraint(cutParticle, replacementParticle, bottom);
        }    
    
        private void UpdateHorizontalDistanceConstraints(Particle cutParticle, Particle replacementParticle)
        {
            Vector2Int coords = cutParticle.meshCoords;
        
            Vector2Int left = new Vector2Int(coords.x - 1  , coords.y );
            Vector2Int right = new Vector2Int(coords.x + 1 , coords.y );
        
            CreateOrUpdateDistanceConstraint(cutParticle, replacementParticle, left);
            CreateOrUpdateDistanceConstraint(cutParticle, replacementParticle, right);
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