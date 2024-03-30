using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DynamicMesh))]
public class MeshParticleInterface : MonoBehaviour
{

    public GameObject particleObject;

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

    public void CutParticleVertical(Particle particle)
    {
        Tuple<int, List<int>> cutConnections = mesh.CutVertexVertical(particle.meshIndex);
        int replacementVertexIndex = cutConnections.Item1;
        
        Particle newParticle = CreateNewParticle(mesh.verticesArr[replacementVertexIndex], replacementVertexIndex, mesh.GetVertexGridCoordinates(particle.meshIndex));
        GameManager.Instance.physicsSimulator.CutParticleVertical(particle,newParticle);
    }    
    public void CutParticleHorizontal(Particle particle)
    {
        Tuple<int, List<int>> cutConnections = mesh.CutVertexHorizontal(particle.meshIndex);
        int replacementVertexIndex = cutConnections.Item1;
        
        Particle newParticle = CreateNewParticle(mesh.verticesArr[replacementVertexIndex], replacementVertexIndex, mesh.GetVertexGridCoordinates(particle.meshIndex));
        GameManager.Instance.physicsSimulator.CutParticleHorizontal(particle,newParticle);
        
    }


    public void SnapMeshIntoParticlePositions()
    {
        foreach(Particle particle in particles)
        {
            mesh.UpdateVertexPosition(particle.meshIndex, particle.transform.localPosition);
        }
        mesh.UpdateVertexes();
    }

    [ContextMenu("AttachToMesh")]
    protected void AttachToMesh()
    {
        particles = new List<Particle>();
        Clear();

        if(mesh == null)
            mesh = GetComponent<DynamicMesh>();


        CreateParticles();
        MakeDistanceConstraints();
    }

    [ContextMenu("Clear")]
    protected void Clear()
    {
        for(int i = 0; i < particles.Count; ++i)
        {
            DestroyImmediate(particles[i].gameObject);
        }
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
        GameObject newObject = Instantiate(particleObject, worldPosition, Quaternion.identity);
        newObject.transform.parent = transform;

        Particle newParticle = newObject.GetComponent<Particle>();
            
        newParticle.Init(this, vertexIndex, meshCoords);
        particles.Add(newParticle);
        GameManager.Instance.physicsSimulator.AddParticle(newParticle);

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
                    particles[index].isLocked = true;
                }

                index++;
            }
        }
    }

    void MakeDistanceConstraints(Particle mainParticle, Particle secondParticle)
    {
        float distance = Vector3.Distance(mainParticle.gameObject.transform.position,secondParticle.gameObject.transform.position);
        DistanceConstraint distanceConstraint = new(mainParticle,secondParticle, distance);
        GameManager.Instance.physicsSimulator.AddDistanceConstraint(distanceConstraint);
    }


    

}
