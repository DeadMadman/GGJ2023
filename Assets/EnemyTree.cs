using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemyTree : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;

    private Entity entity;

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var archetype = manager.CreateArchetype(typeof(LocalTransform), typeof(WorldTransform), typeof(LocalToWorld));
        entity = manager.CreateEntity(archetype);
        manager.AddComponentData(entity, new LocalTransform { Position = transform.position, Rotation = transform.rotation, Scale = 1.0f });
        
        manager.AddComponentData(entity, new Speed { value = speed });
        manager.AddComponentData(entity, new Look { value = transform.forward });
        manager.AddComponent<Attackable>(entity);
        manager.AddComponent<PreviousVelocity>(entity);
        manager.AddComponent<Velocity>(entity);
        manager.AddComponentData(entity, new WalkingVFX { vfxName = "Walking" });
        manager.AddComponentData(entity, new HitVFX { vfxName = "Explosion" });
    }
}
