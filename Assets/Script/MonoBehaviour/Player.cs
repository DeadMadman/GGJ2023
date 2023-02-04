using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;


    public MeshFilter MeshFilter
    {
        get 
        { 
            if(meshFilter == null) {
                meshFilter = GetComponent<MeshFilter>();
            }
            return meshFilter; 
        }
    }

    public MeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null) {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            return meshRenderer;
        }
    }

    [SerializeField] private float speed;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private float dodgeCooldown;

    

    public void BeGrey()
    {
        MeshRenderer.material.color = Color.grey;
    }
    public void BeRed()
    {
        MeshRenderer.material.color = Color.red;
    }
    public void BeBlue()
    {
        MeshRenderer.material.color = Color.blue;
    }
    public void BeGreen()
    {
        MeshRenderer.material.color = Color.green;
    }
    public void BeMagenta()
    {
        MeshRenderer.material.color = Color.magenta;
    }

    public void Walk()
    {
        
    }


    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            var animator = authoring.GetComponent<Animator>();
            AddComponentObject(new Visuals { filter = authoring.MeshFilter, renderer = authoring.MeshRenderer });
            AddComponentObject(new Anim { animator = animator });
            AddComponent(new Speed { value = authoring.speed });
            AddComponent<Input>();
            AddComponent<PreviousVelocity>();
            AddComponent<Velocity>();
            //AddComponentObject(new TransformContext { transform = authoring.transform });
            AddComponent(new WalkingVFX { vfxName = "Walking" });
            AddComponent(new Look { value = authoring.transform.forward });
            AddComponent(new Dodge { cooldown = authoring.dodgeCooldown, dodgeTime = authoring.dodgeTime, dodgeSpeed = authoring.dodgeSpeed });
        }
    }
}


public partial struct VelocityToPreviousVelocitySystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var (prev, curr) in SystemAPI.Query<RefRW<PreviousVelocity>, Velocity>()) {
            prev.ValueRW.value = curr.value;
        }
    }
}

public partial struct VelocityToAnimatorSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (visuals, input) in SystemAPI.Query<Anim, Input>()) {
            var anim = visuals.animator;
            anim.SetFloat("lookx", input.movement.x);
            anim.SetFloat("looky", input.movement.y);
        }
    }

}

public partial struct UpdateVisuals : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        foreach (var visuals in SystemAPI.Query<Anim>()) {
            var anim = visuals.animator;
            anim.Update(dt);
        }
    }
}


//public partial struct UpdateTransformContext : ISystem
//{
//    public void OnCreate(ref SystemState state) { }

//    public void OnDestroy(ref SystemState state) { }

//    public void OnUpdate(ref SystemState state)
//    {
//        foreach (var (context, transform) in SystemAPI.Query<TransformContext, LocalToWorld>()) {
//            context.transform.localPosition = transform.Position;
//            context.transform.localRotation = transform.Rotation;
//        }
//    }
//}


public partial struct WalkingEffectsSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var particles = ParticleSystemManager.Instance;


        foreach (var (transform, velocity, previousVelocity, vfx) in SystemAPI.Query<LocalToWorld, Velocity, PreviousVelocity, RefRW<WalkingVFX>>()) {
            bool isMoving = math.lengthsq(velocity.value) > 0.0f;
            bool wasMoving = math.lengthsq(previousVelocity.value) > 0.0f;
            ref WalkingVFX vfxRef = ref vfx.ValueRW;
            var key = vfxRef.vfxName.Value;
            if (isMoving && !wasMoving) {
                vfxRef.handle = particles.Play(key, transform.Position, transform.Rotation);
            }
            else if (!isMoving && wasMoving) {
                particles.Stop(key, vfxRef.handle);
            }

            if(isMoving) {
                particles.Transform(key, vfxRef.handle, transform.Position, transform.Rotation);
            }
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
        foreach (var (dodge, input, visuals, transform, entity) in SystemAPI.Query<RefRW<Dodge>, Input, Anim, LocalToWorld>().WithNone<Dodging>().WithEntityAccess()) {
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

        foreach (var (dodging, visuals, entity) in SystemAPI.Query<RefRW<Dodging>, Anim>().WithEntityAccess()) {
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


public partial struct PlantingSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (input, plantingPosition) in SystemAPI.Query<Input, RefRO<LocalTransform>>())
		{
            if (input.plantButton) // Need to somewhow also ask if the tree can be planted at the plantingPosition
			{
                //plant tree

			}
		}
    }
}
