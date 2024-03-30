using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class DynamicMesh : MonoBehaviour
{
    [SerializeField] protected Mesh mesh;
    [SerializeField] public List<Vector3> vertices; //this should be double the amount actually needed to allow for cutting
    [SerializeField] protected List<int> triangles;
    [SerializeField] protected List<Vector2> uvs;

    [SerializeField] public Vector3[] verticesArr;
    [SerializeField] protected int[] trianglesArr;
    [SerializeField] protected Vector2[] uvsArr;

    public UnityEvent OnMeshFinishedGenerating;

    public Vector2Int resolution = new Vector2Int(100,500);
    protected int coreSize = 0; 
    protected int halfCoreSize;


    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    protected void FinalizeMeshInitialization()
    {
        verticesArr = vertices.ToArray();
        trianglesArr = triangles.ToArray();
        uvsArr = uvs.ToArray();

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public virtual void UpdateVertexPosition(int Index, Vector3 Position)
    {
        vertices[Index] = Position;
        UpdateMesh();
    }

    public virtual Vector3[] GetAttachPoints()
    {
        
        return verticesArr;
    }



    protected void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }
    

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verticesArr;
        mesh.triangles = trianglesArr;
        mesh.uv = uvsArr;
        mesh.RecalculateNormals();
    }

    public void UpdateVertexes()
    {
        mesh.vertices = verticesArr;
        mesh.RecalculateNormals();
    }
    public void UpdateTriangles()
    {
        mesh.triangles = trianglesArr;
        mesh.RecalculateNormals();
    }

    protected void InitMesh(int n)
    {
        vertices = new List<Vector3>(n);
        triangles = new List<int>(n*3);
        uvs = new List<Vector2>(n);
    }
    [ContextMenu("Clear")]
    protected void Clear()
    {
        mesh.Clear();
    }

    public Vector2Int GetVertexGridCoordinates(int vertexIndex)
    {
        int x = vertexIndex % resolution.x;
        int y = (vertexIndex / resolution.x) % resolution.y;
        return new(x,y);

    }

    int GetTriangleHorizontalDirectionInRelationToPoint(int vertexIndex,  Vector3Int triangleVertexIndexes)
    {
        Vector2Int coordinatesX = GetVertexGridCoordinates(triangleVertexIndexes.x);
        Vector2Int coordinatesY = GetVertexGridCoordinates(triangleVertexIndexes.y);
        Vector2Int coordinatesZ = GetVertexGridCoordinates(triangleVertexIndexes.z);

        Vector2Int coordinatesVertex = GetVertexGridCoordinates(vertexIndex);

        if(coordinatesX.x < coordinatesVertex.x || coordinatesY.x < coordinatesVertex.x || coordinatesZ.x < coordinatesVertex.x)
            return -1;
        if(coordinatesX.x > coordinatesVertex.x || coordinatesY.x > coordinatesVertex.x || coordinatesZ.x > coordinatesVertex.x)
            return 1;

        Debug.LogError("Invalid Triangle");
        return 0;
    }
    
    int GetTriangleVerticalDirectionInRelationToPoint(int vertexIndex,  Vector3Int triangleVertexIndexes)
    {
        Vector2Int coordinatesX = GetVertexGridCoordinates(triangleVertexIndexes.x);
        Vector2Int coordinatesY = GetVertexGridCoordinates(triangleVertexIndexes.y);
        Vector2Int coordinatesZ = GetVertexGridCoordinates(triangleVertexIndexes.z);

        Vector2Int coordinatesVertex = GetVertexGridCoordinates(vertexIndex);

        if(coordinatesX.y < coordinatesVertex.y || coordinatesY.y < coordinatesVertex.y || coordinatesZ.y < coordinatesVertex.y)
            return -1;
        if(coordinatesX.y > coordinatesVertex.y || coordinatesY.y > coordinatesVertex.y || coordinatesZ.y > coordinatesVertex.y)
            return 1;

        Debug.LogError("Invalid Triangle");
        return 0;
    }

    public Tuple<int,List<int>> CutVertexVertical(int inVertexIndex)
    {
        CutVertex(inVertexIndex + halfCoreSize, true);
        return CutVertex(inVertexIndex, true);
    }
    
    public Tuple<int,List<int>> CutVertexHorizontal(int inVertexIndex)
    {
        CutVertex(inVertexIndex + halfCoreSize, false);
        return CutVertex(inVertexIndex, false);
    }

    private Tuple<int,List<int>> CutVertex(int inVertexIndex, bool bIsVertical)
    {
        int replacementVertexIndex = inVertexIndex + coreSize;
        Tuple<int,List<int>> cutConnections = new(replacementVertexIndex, new List<int>());
        if(inVertexIndex > coreSize)
        {
            //TODO Handle this case
            Debug.Log("Recutting Point");
            return cutConnections;
        }

        for(int i = 0; i < trianglesArr.Length; i+=3)
        {
            Vector3Int triangle = new(trianglesArr[i], trianglesArr[i+1], trianglesArr[i+2]);
            bool bTriangleContainsVertex = triangle.x == inVertexIndex || triangle.y == inVertexIndex || triangle.z == inVertexIndex;
            if(bTriangleContainsVertex == false )
                continue;

            int direction = 0;
            if (bIsVertical)
            {
                direction = GetTriangleHorizontalDirectionInRelationToPoint(inVertexIndex, triangle);
            }
            else
            {
                direction = GetTriangleVerticalDirectionInRelationToPoint(inVertexIndex, triangle);
            }
            if(direction < 1)
                continue;

            for(int j = 0; j < 3; j++)
            {
                if(trianglesArr[i + j] == inVertexIndex)
                {
                    trianglesArr[i + j] = inVertexIndex + coreSize;
                   
                }
                else if(cutConnections.Item2.Contains(trianglesArr[i + j]) == false)
                {
                    cutConnections.Item2.Add(trianglesArr[i + j]);
                }
            }
        }

        UpdateTriangles();
        return cutConnections;
    }

}
