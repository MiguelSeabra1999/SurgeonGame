using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Draggable : MonoBehaviour
{
    public UnityEvent OnGrabbed;
    public UnityEvent OnContextAction;
    public UnityEvent OnReleased;
    private Vector3 mousePositionOffset;



    void OnMouseOver() 
    {
        if(Input.GetKey(KeyCode.Mouse1))
        {
             OnContextAction.Invoke();
        }
    }
    void OnMouseDrag()
    {
        Vector3 newPosition =  GetMouseWorldPosition() + mousePositionOffset;
        //Debug.Log(GetMouseWorldPosition());

        Vector3 clampedPosition = new Vector3(newPosition.x, transform.position.y,newPosition.z);
        transform.position = clampedPosition;



    }
    void OnMouseDown()
    {        
        Vector3 position = gameObject.transform.position - GetMouseWorldPosition();
        Vector3 clampedPosition = new Vector3(position.x,0,position.z);
        mousePositionOffset = clampedPosition;
        OnGrabbed.Invoke();
    
    }   

    


    void OnMouseUp()
    {
        OnReleased.Invoke();
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
    }
}
