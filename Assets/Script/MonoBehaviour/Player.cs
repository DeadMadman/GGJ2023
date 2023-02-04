using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class Player : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private float dodgeCooldown;

    [SerializeField] private GameObject treePrefab;

    private Entity entity;



    private void Update()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var transform = manager.GetComponentData<LocalToWorld>(entity);
        var localData = manager.GetComponentData<LocalTransform>(entity);
        this.transform.localPosition = localData.Position;
        this.transform.localRotation = localData.Rotation;
        this.transform.localScale = localData.Scale * Vector3.one;

    }

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // Break down for re-usable bakers
        //var copy = authoring.Instance;
        var archetype = manager.CreateArchetype(typeof(LocalTransform), typeof(WorldTransform), typeof(LocalToWorld));
        entity = manager.CreateEntity(archetype);
        var animator = GetComponentInChildren<Animator>();

        manager.AddComponentData(entity, new LocalTransform { Position = transform.position, Rotation = transform.rotation, Scale = 1.0f }) ;
        manager.AddComponentObject(entity, new Anim { animator = animator });
        manager.AddComponentData(entity, new Speed { value = speed });
        manager.AddComponent<Input>(entity);
        manager.AddComponent<PreviousVelocity>(entity);
        manager.AddComponent<Velocity>(entity);
        manager.AddComponentData(entity, new Attack { attackTime = 1.0f, cooldown = 1.25f, range = 2.5f, angle = 180.0f });
        manager.AddComponentData(entity, new WalkingVFX { vfxName = "Walking" });
        manager.AddComponentData(entity, new AttackVFX { vfxName = "Axe Swing" });
        manager.AddComponentData(entity, new Look { value = transform.forward });
        manager.AddComponentData(entity, new Dodge { cooldown = dodgeCooldown, dodgeTime = dodgeTime, dodgeSpeed = dodgeSpeed });
        manager.AddComponentData(entity, new PlantableTree { entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity() });
    }

    //public class Baker : Baker<Player>
    //{
    //    public override void Bake(Player authoring)
    //    {
    //        // Break down for re-usable bakers
    //        //var copy = authoring.Instance;
    //        var animator = authoring.GetComponentInChildren<Animator>();

    //        AddComponentObject(new Anim { animator = animator });
    //        AddComponent(new Speed { value = authoring.speed });
    //        AddComponent<Input>();
    //        AddComponent<PreviousVelocity>();
    //        AddComponent<Velocity>();
    //        AddComponent(new Attack { attackTime = 1.0f, cooldown = 2.0f, range = 2.0f, angle = 90.0f });
    //        AddComponent(new WalkingVFX { vfxName = "Walking" });
    //        AddComponent(new AttackVFX { vfxName = "Axe Swing" });
    //        AddComponent(new Look { value = authoring.transform.forward });
    //        AddComponent(new Dodge { cooldown = authoring.dodgeCooldown, dodgeTime = authoring.dodgeTime, dodgeSpeed = authoring.dodgeSpeed });
    //        AddComponentObject(new PlantableTree { prefab = authoring.treePrefab });
    //    }
    //}
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
            anim.SetFloat("looky", input.movement.z);
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


public partial struct KeepInBoundsSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var level = LevelManager.Instance;
        var bounds = level.Bounds;

        foreach (var (input, transform) in SystemAPI.Query<Input, RefRW<LocalTransform>>()) {
            ref var transformRef = ref transform.ValueRW;
            transformRef.Position = Vector3.Max(bounds.min, transformRef.Position);
            transformRef.Position = Vector3.Min(bounds.max, transformRef.Position);
        }
    }
}


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
                particles.Stop(vfxRef.handle);
            }

            if(isMoving) {
                particles.Transform(vfxRef.handle, transform.Position, transform.Rotation);
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
        foreach(var (input, velocity, transform) in SystemAPI.Query<Input, RefRW<Velocity>, RefRW<LocalTransform>>().WithNone<Dodging, Attacking>()) {
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
public partial struct InputToAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        var particles = ParticleSystemManager.Instance;

        EntityCommandBuffer cmd = new EntityCommandBuffer(Allocator.Temp, PlaybackPolicy.SinglePlayback);
        foreach (var (velocity, attack, input, visuals, transform, vfx, entity) in SystemAPI.Query<RefRW<Velocity>, RefRW<Attack>, Input, Anim, LocalToWorld, RefRW<AttackVFX>>().WithNone<Dodging, Attacking>().WithEntityAccess()) {
            var time = attack.ValueRO.time - dt;
            var att = attack.ValueRO;
            ref var attackVfx = ref vfx.ValueRW;
            if (time <= 0.0f && input.justAttacked) {
                attack.ValueRW.time += attack.ValueRO.cooldown;
                cmd.AddComponent(entity, new Attacking { time = attack.ValueRO.attackTime, angle = att.angle, range = att.range });
                velocity.ValueRW.value = Vector3.zero;
                visuals.animator.SetBool("attack", true);
                particles.PlayOnce(attackVfx.vfxName.Value, transform.Position + math.float3(0.0f, 1.0f, 0.0f), transform.Rotation);
            }
            attack.ValueRW.time = math.max(time, 0.0f);
        }
        cmd.Playback(state.EntityManager);
    }
}

public partial struct OnHitSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var particles = ParticleSystemManager.Instance;
        foreach(var (transform, attackable, handle) in SystemAPI.Query<LocalToWorld, Attackable, RefRW<HitVFX>>()) {
            if(attackable.JustAttacked) {
     
                particles.PlayOnce(handle.ValueRW.vfxName.Value, transform.Position + math.float3(0.0f, 0.5f, 0.0f), transform.Rotation);

            }
            //else if(attackable.StoppedAttacked){
            //    Debug.Log("End");
            //    particles.Stop(handle.ValueRW.handle);
            //}

            //if(attackable.IsAttacked) {
            //    Debug.Log("Attack");
            //}
        }
    }
}


public partial struct AttackSystem : ISystem
{
    private bool InFOV(Vector3 lhs, Vector3 dir, Vector3 rhs, float minDistance, float maxDistance, float angle)
    {
        var offset = rhs - lhs;
        var distance = offset.magnitude;

        if (distance < minDistance + Mathf.Epsilon) {
            return false;
        }

        if (distance < maxDistance) {
            var unitOffset = offset / distance;
            var coneViewDot = Vector3.Dot(dir, unitOffset);
            if (coneViewDot > Mathf.Cos(angle)) {
                return true;
            }
        }
        return false;
    }

    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        foreach (var attackable in SystemAPI.Query<RefRW<Attackable>>()) {
            ref var att = ref attackable.ValueRW;
            att.prevState = att.currState;
            att.currState = false;
        }

        foreach (var (attackRef, look, lhs) in SystemAPI.Query<RefRW<Attacking>, Look, LocalToWorld>()) {
            ref var attack = ref attackRef.ValueRW;
            attack.prevHit = attack.currHit;
            attack.currHit = false;

            foreach(var (rhs, attackable) in SystemAPI.Query<LocalToWorld, RefRW<Attackable>>()) {
                ref var att = ref attackable.ValueRW;
                if(InFOV(lhs.Position, look.value, rhs.Position, 0.5f, attack.range, attack.angle)) {
                    att.currState = true;
                    attack.currHit = true;
                }
            }
        }

        NativeList<Entity> entities = new NativeList<Entity>(Allocator.Temp);
        foreach (var (attacking, visuals, entity) in SystemAPI.Query<RefRW<Attacking>, Anim>().WithEntityAccess()) {
            var time = attacking.ValueRO.time - dt;
            if (time <= 0.0f) {
                entities.Add(entity);

                visuals.animator.SetBool("attack", false);
            }

            attacking.ValueRW.time = math.max(time, 0.0f);
        }
        state.EntityManager.RemoveComponent<Attacking>(entities.AsArray());
    }
}

public partial struct InputToDodgeSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        var particles = ParticleSystemManager.Instance;

        EntityCommandBuffer cmd = new EntityCommandBuffer(Allocator.Temp, PlaybackPolicy.SinglePlayback);
        foreach (var (dodge, input, visuals, transform, entity) in SystemAPI.Query<RefRW<Dodge>, Input, Anim, LocalToWorld>().WithNone<Dodging, Attacking>().WithEntityAccess()) {
            var time = dodge.ValueRO.time - dt;
            if(time <= 0.0f && input.justDodged) {
                dodge.ValueRW.time += dodge.ValueRO.cooldown;
                cmd.AddComponent(entity, new Dodging { time = dodge.ValueRO.dodgeTime });
                particles.PlayOnce("Electro", transform.Position + math.float3(0.0f, 0.5f, 0.0f), transform.Rotation);
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
        var particles = ParticleSystemManager.Instance;

        foreach (var (transform, dodging, visuals, entity) in SystemAPI.Query<LocalToWorld, RefRW<Dodging>, Anim>().WithEntityAccess()) {
            var time = dodging.ValueRO.time - dt;
            if (time <= 0.0f) {
                entities.Add(entity);
                particles.PlayOnce("KABOOM", transform.Position + math.float3(0.0f, 0.5f, 0.0f), transform.Rotation);

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
        foreach (var (input, plantingPosition, tree) in SystemAPI.Query<Input, RefRO<LocalTransform>, PlantableTree>())
		{
            if (input.plantButton) // TODO: Make sure it's not too close to another tree, but that would require comparing distance with a ton of trees which sounds annoying.
            {
                float3 pos = plantingPosition.ValueRO.Position;
                pos.y = 0.5f;
                Entity newTree = state.EntityManager.Instantiate(tree.entity);
                // It says that the new entity doesn't have a LocalTransform, this was not an issue before.
                state.EntityManager.SetComponentData(newTree, LocalTransform.FromPosition(pos));
			}
		}
    }
}