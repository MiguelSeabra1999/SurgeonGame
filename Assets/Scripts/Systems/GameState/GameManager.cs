using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Interaction;
using Systems.Physics;
using UnityEngine;

[RequireComponent(typeof(PhysicsSimulator))]
[RequireComponent(typeof(PlayerInput))]
public class GameManager : MonoBehaviour
{
    [HideInInspector, SerializeField] public static GameManager Instance { get; private set; }

    [HideInInspector, SerializeField] public PhysicsSimulator physicsSimulator; 
    [HideInInspector, SerializeField] public PlayerInput playerInput; 

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
        playerInput = GetComponent<PlayerInput>();
    }
}
