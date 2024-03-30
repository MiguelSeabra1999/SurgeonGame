using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsSimulator : MonoBehaviour
{
    public bool updatePhysics = false;
    public bool drawConstraints = true;
    public float cachedErrorSum = 0;
    private List<Particle> _particles;
    private Dictionary<Vector2Int, Particle> _particlesGrid;
    private Dictionary<Vector2Int, Particle> _secondaryParticlesGrid;
    private Dictionary<Particle, List<int>> _particlesConstraints;
    private List<DistanceConstraint> _distanceConstraints;


    void Awake()
    {
        _particles = new List<Particle>();
        _distanceConstraints = new List<DistanceConstraint>();
        _particlesGrid = new Dictionary<Vector2Int, Particle>();
        _secondaryParticlesGrid = new Dictionary<Vector2Int, Particle>();
        _particlesConstraints = new Dictionary<Particle, List<int>>();
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

    private float SolveConstraints()
    {
        float errorSum = 0;
        foreach (DistanceConstraint distanceConstraint in _distanceConstraints)
        {
            Vector3 error = distanceConstraint.Solve();
            errorSum += error.magnitude;
        }


        return errorSum;
    }

    public void AddParticle(Particle particle)
    {
        _particles.Add(particle);
        _particlesConstraints.Add(particle, new List<int>());
        if (!_particlesGrid.TryAdd(particle.meshCoords, particle))
        {
            _secondaryParticlesGrid.Add(particle.meshCoords, particle);
        }
    }

    public void AddDistanceConstraint(DistanceConstraint distanceConstraint)
    {
        _distanceConstraints.Add(distanceConstraint);
        _particlesConstraints[distanceConstraint.particleA].Add(_distanceConstraints.Count - 1);
        _particlesConstraints[distanceConstraint.particleB].Add(_distanceConstraints.Count - 1);
    }
    
    void MakeDistanceConstraints(Particle mainParticle, Particle secondParticle)
    {
        float distance = Vector3.Distance(mainParticle.gameObject.transform.position,secondParticle.gameObject.transform.position);
        DistanceConstraint distanceConstraint = new(mainParticle,secondParticle, distance);
        AddDistanceConstraint(distanceConstraint);
    }

    protected int FindConstraintBetweenTwoParticles(Particle particleA, Particle particleB)
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
        Vector2Int Right = new Vector2Int(coords.x + 1, coords.y);
        Vector2Int bottomRight = new Vector2Int(coords.x + 1 , coords.y - 1);
        Particle topRightParticle = _particlesGrid[topRight]; 
        Particle rightParticle = _particlesGrid[Right];
        Particle bottomRightParticle = _particlesGrid[bottomRight];
        
        Particle[] ParticlesToMoveConstraint = new Particle[]{ topRightParticle, rightParticle, bottomRightParticle};
        MigrateConstraintsToReplacementParticle(cutParticle, replacementParticle, ParticlesToMoveConstraint);
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
        
        Particle[] ParticlesToMoveConstraint = new Particle[]{ topLeftParticle, topParticle, topRightParticle};
        MigrateConstraintsToReplacementParticle(cutParticle, replacementParticle, ParticlesToMoveConstraint);
    }

    private void MigrateConstraintsToReplacementParticle(Particle cutParticle, Particle replacementParticle,
        Particle[] ParticlesToMoveConstraint)
    {
        foreach (Particle particle in ParticlesToMoveConstraint)
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


}