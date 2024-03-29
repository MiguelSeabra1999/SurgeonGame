using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Draggable))]
public class Particle : MonoBehaviour
{
    public float heighDisplacementWhenGrabbed = 3;
    public int meshIndex = -1;

    private MeshParticleInterface meshParticleInterface;
    private Draggable draggable;

    public bool isLocked = false;

    void Awake()
    {
        draggable = GetComponent<Draggable>();
        draggable.OnGrabbed.AddListener(OnGrabbed);
        draggable.OnReleased.AddListener(OnReleased);
        draggable.OnContextAction.AddListener(OnCut);
    }

    public void Init(MeshParticleInterface inMeshParticleInterface, int inMeshIndex)
    {
        meshParticleInterface = inMeshParticleInterface;
        meshIndex = inMeshIndex;
    }

    [ContextMenu("OnPositionUpdated")]
    public void OnPositionUpdated()
    {
        meshParticleInterface.OnParticlePositionUpdated(this);
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

    void OnCut()
    {
        meshParticleInterface.CutParticleVertical(this);
    }
}
