using System;
using System.Collections;
using System.Collections.Generic;
using Libraries;
using Systems.Physics;
using UnityEngine;

[RequireComponent(typeof(DynamicMesh))]
public class MeshParticleInterface : MonoBehaviour
{



    private List<Particle> particles;
    private DynamicMesh mesh;
   

    public bool bUpdateEveryFrame = false;
    private bool bHaveParticlesMoved = false;

    
    void Awake()
    {
        mesh = GetComponent<DynamicMesh>();
        mesh.OnMeshFinishedGenerating.AddListener(AttachToMesh);
    }

    void Update()
    {
        if(bHaveParticlesMoved || bUpdateEveryFrame)
        {
            SnapMeshIntoParticlePositions();
            bHaveParticlesMoved = false;
        }
    }

    public void OnParticlePositionUpdated(Particle particle)
    {
        bHaveParticlesMoved = true;
    }

    public Particle CutParticle(Particle particle, Axis axis)
    {
        Tuple<int, List<int>> cutConnections = mesh.CutVertexPair(particle.meshIndex, axis);
        int replacementVertexIndex = cutConnections.Item1;
        
        Particle newParticle = CreateNewParticle(mesh.verticesArr[replacementVertexIndex], replacementVertexIndex, mesh.GetVertexGridCoordinates(particle.meshIndex));
        return newParticle;
    }    



    public void SnapMeshIntoParticlePositions()
    {
        PhysicsSimulator physicsSimulator = GameManager.Instance.physicsSimulator;
        foreach(Particle particle in particles)
        {
            Vector3 particleWorldPos = physicsSimulator.GetParticle(particle.meshIndex).position;
            Vector3 localPos = mesh.transform.InverseTransformPoint(particleWorldPos);
            mesh.UpdateVertexPosition(particle.meshIndex, localPos);
        }
        mesh.UpdateVertexes();
    }

    [ContextMenu("AttachToMesh")]
    protected void AttachToMesh()
    {
        particles = new List<Particle>();
        
        if(mesh == null)
            mesh = GetComponent<DynamicMesh>();
        
        CreateParticles();
        MakeDistanceConstraints();
    }

    void CreateParticles()
    {
        Vector3[] attachPoints = mesh.GetAttachPoints();
        for(int i = 0; i < attachPoints.Length; ++i)
        {
            CreateNewParticle(attachPoints[i], i, mesh.GetVertexGridCoordinates(i));
        
        }
    }

    private Particle CreateNewParticle(Vector3 attachPoint, int vertexIndex, Vector2Int meshCoords)
    {
        Vector3 worldPosition = mesh.transform.TransformPoint(attachPoint);
        Particle newParticle = new Particle(worldPosition,vertexIndex, meshCoords);
        particles.Add(newParticle);
        GameManager.Instance.physicsSimulator.AddParticle(newParticle, this);
        return newParticle;
    }

    void MakeDistanceConstraints()
    {

        int index = 0;
        for(int y = 0; y < mesh.resolution.y ; y++)
        {
            for(int x = 0; x < mesh.resolution.x; x++)
            {
                int bottomRowIndex = index + mesh.resolution.x;

                if(x <  mesh.resolution.x-1)
                {
                    MakeDistanceConstraints( particles[index],  particles[index+1]);
                }
                if(bottomRowIndex < particles.Count)
                {
                    MakeDistanceConstraints( particles[index],  particles[bottomRowIndex]);
                }
                if(bottomRowIndex + 1 < particles.Count && x < mesh.resolution.x - 1)
                {
                    MakeDistanceConstraints(particles[index],  particles[bottomRowIndex + 1]);
                }
                if(bottomRowIndex < particles.Count && x < mesh.resolution.x - 1)
                {
                    MakeDistanceConstraints(particles[index+1],  particles[bottomRowIndex]);
                }

                if(x == 0 || x == mesh.resolution.x - 1 || y == 0 || y == mesh.resolution.y - 1)
                {
                    GameManager.Instance.physicsSimulator.LockParticle(index);
                }

                index++;
            }
        }
    }

    void MakeDistanceConstraints(Particle mainParticle, Particle secondParticle)
    {
        float distance = Vector3.Distance(mainParticle.position,secondParticle.position);
        DistanceConstraint distanceConstraint = new(mainParticle,secondParticle, distance);
        GameManager.Instance.physicsSimulator.AddDistanceConstraint(distanceConstraint);
    }


    

}
