using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SkinMesh : DynamicMesh
{
    [Header("Dimensions")]
    public float length = 10;
    public float width = 2;

    private int halfCoreSize;


    void Start()
    {
        GeneratePlaneMesh();
        OnMeshFinishedGenerating.Invoke();
    }


    public override Vector3[] GetAttachPoints()
    {
        List<Vector3> Result = new List<Vector3>();
        //dived by for because we have vertexes for the bottom facing and top facing + a copy of every vertex for cutting
        for(int i = 0; i < verticesArr.Length/4; i++)
        {
            Result.Add(verticesArr[i]);
        }
        return Result.ToArray();
    }
    public override void UpdateVertexPosition(int Index, Vector3 Position)
    {
        verticesArr[Index] = Position;
        verticesArr[Index + halfCoreSize] = Position;
    }

    [ContextMenu("Generate plane mesh")]
    public void GeneratePlaneMesh()
    {
        if(mesh == null)
        {
            int n = resolution.x * resolution.y * 2;
            InitMesh(n);
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        
        float x_step = width/resolution.x;
        float y_step = length/resolution.y;
        int index = 0;


        for(int y = 0; y < resolution.y; y++)
        {
            for(int x = 0; x < resolution.x; x++)
            {
                vertices.Add(new Vector3(x*x_step,y*y_step,0));
                Vector2 uv = new Vector2((float)x/(resolution.x-1), (float)y/(resolution.y-1));
                uvs.Add(uv);

                if(y > 0)
                {
                    int bottomRowIndex = index - resolution.x;
                    if(x < resolution.x-1)
                    {
                       AddTriangle(index+1, bottomRowIndex,index);
                    }
                    if(x > 0)
                    {
                       AddTriangle(bottomRowIndex,bottomRowIndex-1,index);
                    }
                }

                index++;
            }
        }

        for(int y = 0; y < resolution.y; y++)
        {
            for(int x = 0; x < resolution.x; x++)
            {
                vertices.Add(new Vector3(x*x_step,y*y_step,0));
                Vector2 uv = new Vector2((float)x/(resolution.x-1), (float)y/(resolution.y-1));
                uvs.Add(uv);

                if(y > 0)
                {
                    int bottomRowIndex = index - resolution.x;
                    if(x < resolution.x-1)
                    {
                        AddTriangle(index,bottomRowIndex,index+1);
                    }
                    if(x > 0)
                    {
                        AddTriangle(index,bottomRowIndex-1,bottomRowIndex);
                    }
                }
                index ++;
            }
        }

        coreSize = vertices.Count;
        halfCoreSize = coreSize/2;

        //creating a backup of every vertex to reattach triangle to on cutting
        for(int i = 0; i < coreSize; ++i)
        {
            vertices.Add(vertices[i]);
            uvs.Add(uvs[i]);
        }

        FinalizeMeshInitialization();
        UpdateMesh();
    }



}


