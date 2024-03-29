using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsSimulator))]
public class GameManager : MonoBehaviour
{
    [HideInInspector, SerializeField] public static GameManager Instance { get; private set; }

    [HideInInspector, SerializeField] public PhysicsSimulator physicsSimulator; 

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 

        physicsSimulator = GetComponent<PhysicsSimulator>();
    }
}
