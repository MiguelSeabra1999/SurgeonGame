using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Draggable))]
public class Particle : MonoBehaviour
{
    public float heighDisplacementWhenGrabbed = 3;

    public float minDragDistanceToCut = 0.01f;

    public int meshIndex = -1;
    public Vector2Int meshCoords = Vector2Int.zero;
    private MeshParticleInterface _meshParticleInterface;
    private Draggable _draggable;

    public bool isLocked = false;
    public bool bWasCut = false;
    
    [HideInInspector] public UnityEvent<Vector3> onCut;
    void Awake()
    {
        _draggable = GetComponent<Draggable>();
        _draggable.onGrabbed.AddListener(OnGrabbed);
        _draggable.onReleased.AddListener(OnReleased);
        _draggable.onContextAction.AddListener(OnCut);
    }


    public void Init(MeshParticleInterface inMeshParticleInterface, int inMeshIndex, Vector2Int inMeshCoords)
    {
        _meshParticleInterface = inMeshParticleInterface;
        meshIndex = inMeshIndex;
        meshCoords = inMeshCoords;
    }

    [ContextMenu("OnPositionUpdated")]
    public void OnPositionUpdated()
    {
        _meshParticleInterface.OnParticlePositionUpdated(this);
    }

    void OnGrabbed()
    {
        transform.position += Vector3.up * heighDisplacementWhenGrabbed;
        isLocked = true;
    }
    void OnReleased()
    {
        transform.position -= Vector3.up * heighDisplacementWhenGrabbed;
        isLocked = false;
    }

    void OnCut(List<Vector3> mouseMovementBuffer)
    {
        if(bWasCut) //TODO allow for multiple cutting
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
        
        onCut.Invoke(movement);
        
        float angleWithForward =  Vector3.Angle(Vector3.forward, movement);        
        float angleWithBack = Vector3.Angle(Vector3.back, movement);
        
        bWasCut = true;
        if (angleWithBack < 45 || angleWithForward < 45)
        {
            Debug.Log(meshIndex + " cut vertically " + movement.normalized);
            _meshParticleInterface.CutParticleVertical(this);
        }
        else
        {
            Debug.Log(meshIndex + " cut horizontally " + movement.normalized);
           _meshParticleInterface.CutParticleHorizontal(this);
        }
    }
    void DebugDraw()
    {
        
    }
}
