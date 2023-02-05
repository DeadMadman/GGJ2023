using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public class EnemyTree : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private Collectable log;
    [SerializeField] private Collectable acorn;

    private Entity entity;

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var archetype = manager.CreateArchetype(typeof(LocalTransform), typeof(WorldTransform), typeof(LocalToWorld));
        entity = manager.CreateEntity(archetype);
        manager.AddComponentData(entity, new Anim { animator = GetComponent<Animator>() }); 
        manager.AddComponentData(entity, new LocalTransform { Position = transform.position, Rotation = transform.rotation, Scale = transform.localScale.x });
        manager.AddComponent<WalkingEnemy>(entity);
        manager.AddComponentData(entity, new Speed { value = speed });
        manager.AddComponentData(entity, new Look { value = transform.forward });
        manager.AddComponent<Attackable>(entity);
        manager.AddComponent<PreviousVelocity>(entity);
        manager.AddComponent<Velocity>(entity);
        manager.AddComponentData(entity, new WalkingFX { vfxName = "Walking" });
        manager.AddComponentData(entity, new Dropping { acorn = acorn, log = log, chanceForAcorn = 0.08f, chanceForWood = 0.25f });

        var mildSound = new FixedList512Bytes<FixedString128Bytes>();
        mildSound.Add("Hit");

        var strongSound = new FixedList512Bytes<FixedString128Bytes>();
        strongSound.Add("Explosion");
        manager.AddComponentData(entity, new HitFX { vfxName = "Explosion", mildSounds = mildSound, strongSounds = strongSound });
        manager.AddComponentData(entity, new Health { health = 2 });
    }

    private void Update()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var transform = manager.GetComponentData<LocalToWorld>(entity);
        var localData = manager.GetComponentData<LocalTransform>(entity);
        this.transform.localPosition = localData.Position;
        this.transform.localRotation = localData.Rotation;
        this.transform.localScale = localData.Scale * Vector3.one;

    }
}
