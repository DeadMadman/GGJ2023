using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;


    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private float dodgeCooldown;


    private Material instance;
    public Mesh Mesh => mesh;
    public Material MaterialInstance
    {
        get
        { 
            if(instance == null) {
                instance = Instantiate(material);
            }
            return instance;
        
        } 
    }


    public void BeGrey()
    {
        MaterialInstance.color = Color.grey;
    }
    public void BeRed()
    {
        MaterialInstance.color = Color.red;
    }
    public void BeBlue()
    {
        MaterialInstance.color = Color.blue;
    }
    public void BeGreen()
    {
        MaterialInstance.color = Color.green;
    }
    public void BeMagenta()
    {
        MaterialInstance.color = Color.magenta;
    }

    public void Walk()
    {
        
    }

    private void OnDestroy()
    {
        if(instance != null) {
            DestroyImmediate(instance);
        }
    }


    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            var animator = authoring.GetComponent<Animator>();
            AddComponent<Movement>();
            AddComponentObject(new Visuals { mesh = authoring.Mesh, material = authoring.MaterialInstance, animator = animator });
            AddComponent(new Speed { value = authoring.speed });
            AddComponent<Input>();
            AddComponent<Velocity>();
            AddComponent(new Look { value = authoring.transform.forward });
            AddComponent(new Dodge { cooldown = authoring.dodgeCooldown, dodgeTime = authoring.dodgeTime, dodgeSpeed = authoring.dodgeSpeed });
        }
    }
}

public partial struct VelocityToAnimatorSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var particles = ParticleSystemManager.Instance;

        EntityCommandBuffer cmd = new(Allocator.Temp);

        foreach (var (visuals, input, entity) in SystemAPI.Query<Visuals, Input>().WithEntityAccess()) {
            var anim = visuals.animator;
            anim.Update(dt);
            anim.SetFloat("lookx", input.movement.x);
            anim.SetFloat("looky", input.movement.y);

            if (math.lengthsq(input.movement) > 0.0f) {
                cmd.AddComponent<Walking>(entity);
            }
            else {
                cmd.RemoveComponent<Walking>(entity);
            }
        }
    }

}

public partial struct WalkingEffectsSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var particles = ParticleSystemManager.Instance;

        EntityCommandBuffer cmd = new(Allocator.Temp);

        foreach (var (visuals, input, entity) in SystemAPI.Query<Visuals, Input>().WithEntityAccess()) {


        }
    }

}




public partial struct TransformToLookDirection : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var (look, transform) in SystemAPI.Query<RefRW<Look>, LocalToWorld>()) {
            look.ValueRW.value = transform.Forward;
        }
        
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public class VelocitySystemGroup : ComponentSystemGroup
{
}

[UpdateInGroup(typeof(VelocitySystemGroup))]
[UpdateBefore(typeof(ApplyVelocitySystem))]
public partial struct InputToVelocitySystem : ISystem
{
    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var (input, velocity, transform) in SystemAPI.Query<Input, RefRW<Velocity>, RefRW<LocalTransform>>().WithNone<Dodging>()) {
            velocity.ValueRW.value = input.movement;
            if(math.lengthsq(input.movement) > 0.0f) {
                transform.ValueRW.Rotation = Quaternion.LookRotation(input.movement, Vector3.up);
            }
        }
    }
}

[UpdateInGroup(typeof(VelocitySystemGroup))]
public partial struct ApplyVelocitySystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, velocity, speed) in SystemAPI.Query<RefRW<LocalTransform>, Velocity, Speed>()) {
            transform.ValueRW.Position += velocity.value * speed.value * dt;
        }
    }
}


public partial struct FollowPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var target = CameraTarget.Instance;

        int count = 0;
        var total = float3.zero;
        var rotation = Quaternion.identity;
        foreach(var transform in SystemAPI.Query<LocalToWorld>().WithAll<Input>()) {
            total += transform.Position;
            count++;
            rotation = transform.Rotation;
        }

        if(count != 0) {
            target.SetPosition(total / count);
            target.SetRotation(rotation);
        }
    }
}

public partial struct InputToDodgeSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        var particle = ParticleSystemManager.Instance;

        EntityCommandBuffer cmd = new EntityCommandBuffer(Allocator.Temp, PlaybackPolicy.SinglePlayback);
        foreach (var (dodge, input, visuals, transform, entity) in SystemAPI.Query<RefRW<Dodge>, Input, Visuals, LocalToWorld>().WithNone<Dodging>().WithEntityAccess()) {
            var time = dodge.ValueRO.time - dt;
            if(time <= 0.0f && input.justDodged) {
                dodge.ValueRW.time += dodge.ValueRO.cooldown;
                cmd.AddComponent(entity, new Dodging { time = dodge.ValueRO.dodgeTime });

                visuals.animator.SetBool("dodge", true);
            }
            dodge.ValueRW.time = math.max(time, 0.0f);
        }
        cmd.Playback(state.EntityManager);
    }
}

public partial struct DodgingSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        NativeList<Entity> entities = new NativeList<Entity>(Allocator.Temp);
        foreach (var (dodge, look, velocity) in SystemAPI.Query<Dodge, Look, RefRW<Velocity>>().WithAll<Dodging>()) {
            velocity.ValueRW.value = look.value * dodge.dodgeSpeed;
        }

        foreach (var (dodging, visuals, entity) in SystemAPI.Query<RefRW<Dodging>, Visuals>().WithEntityAccess()) {
            var time = dodging.ValueRO.time - dt;
            if (time <= 0.0f) {
                entities.Add(entity);


                visuals.animator.SetBool("dodge", false);
            }
            dodging.ValueRW.time = math.max(time, 0.0f);
        }

        state.EntityManager.RemoveComponent<Dodging>(entities.AsArray());
    }
}