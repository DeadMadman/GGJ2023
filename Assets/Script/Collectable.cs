using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public class Collectable : MonoBehaviour
{
    [SerializeField] private bool isLog = true;
    
    private ScoreManager scoreManager;
    
    Camera mainCamera;

    private Entity entity;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        mainCamera = Camera.main;
        transform.rotation = mainCamera.transform.rotation;

        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var archetype = manager.CreateArchetype(typeof(Collectible), typeof(LocalTransform), typeof(WorldTransform), typeof(LocalToWorld));
        entity = manager.CreateEntity(archetype);
        manager.AddComponentData(entity, new Collectible { context = this });
        manager.AddComponentData(entity, new LocalTransform { Position = transform.position, Rotation = transform.rotation, Scale = 1.0f });
    }

    public void Collect()
    {
        if(isLog) {
            scoreManager.AddLog(1);
        }
        else {
            scoreManager.AddAcorn(1);
        }

    }

    private void Update()
    {
        if(entity == Entity.Null) {
            return;
        }
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var transform = manager.GetComponentData<LocalToWorld>(entity);
        var localData = manager.GetComponentData<LocalTransform>(entity);
        this.transform.localPosition = localData.Position;
        this.transform.localRotation = localData.Rotation;
        this.transform.localScale = localData.Scale * Vector3.one;
    }

}
