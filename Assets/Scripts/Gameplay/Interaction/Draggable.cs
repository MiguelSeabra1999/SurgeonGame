using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

//DEPRECATED
public class Draggable : MonoBehaviour
{

  /*  [HideInInspector] public UnityEvent onGrabbed;
    [HideInInspector] public UnityEvent<List<Vector3>> onContextAction;
    [HideInInspector] public UnityEvent onReleased;
    private Vector3 _mousePositionOffset;
    private List<Vector3> _hoveredBuffer;

    void Awake()
    {
        _hoveredBuffer = new List<Vector3>();
    }

    void OnMouseOver() 
    {
        if(Input.GetKey(KeyCode.Mouse1) == false)
            return;

        _hoveredBuffer.Add(GetMouseWorldPosition());
        onContextAction.Invoke(_hoveredBuffer);
        
    }
    void OnMouseDrag()
    {
        Vector3 newPosition =  GetMouseWorldPosition() + _mousePositionOffset;
        //Debug.Log(GetMouseWorldPosition());

        Transform myTransform = transform;
        Vector3 clampedPosition = new Vector3(newPosition.x, myTransform.position.y,newPosition.z);
        myTransform.position = clampedPosition;
        
        GetComponent<Particle>().OnPositionUpdated();
    }
    void OnMouseDown()
    {        
        Vector3 position = gameObject.transform.position - GetMouseWorldPosition();
        Vector3 clampedPosition = new Vector3(position.x,0,position.z);
        _mousePositionOffset = clampedPosition;
        onGrabbed.Invoke();
    
    }   
    
    void OnMouseUp()
    {
        onReleased.Invoke();
        _hoveredBuffer.Clear();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 viewportPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }*/
}
