using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private bool isLog = true;
    
    private ScoreManager scoreManager;
    
    Camera mainCamera;
    
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        mainCamera = Camera.main;
        transform.rotation = mainCamera.transform.rotation;
    }
    
}
