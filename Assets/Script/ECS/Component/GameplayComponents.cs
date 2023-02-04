using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

public struct PreviousVelocity : IComponentData
{
    public float3 value;
}

public struct Velocity : IComponentData
{
    public float3 value;
}

public struct Look : IComponentData
{
    public float3 value;
}

public struct WalkingVFX : IComponentData
{
    public FixedString128Bytes vfxName;
    public int handle;
}

public struct HitVFX : IComponentData
{
    public FixedString128Bytes vfxName;
    public int handle;
}

public struct AttackVFX : IComponentData
{
    public FixedString128Bytes vfxName;
    public int handle;
}

public struct Dodge : IComponentData
{
    public float dodgeTime;
    public float dodgeSpeed;
    public float cooldown;
    public float time;
}

public struct Dodging : IComponentData
{
    public float time;
}

public struct Speed : IComponentData
{
    public float value;
}

public struct Input : IComponentData
{
    public float3 movement;
    public bool justDodged;
    public bool justAttacked;
    public bool plantButton;
}

public struct Attack : IComponentData
{
    public float cooldown;
    public float attackTime;
    public float angle;
    public float range;
    public float time;
}

public struct Attacking : IComponentData
{
    public float time;
    public float angle;
    public float range;

    public bool prevHit;
    public bool currHit;

    public bool IsHitting => currHit;
    public bool JustHit => currHit && !prevHit;
    public bool StoppedHitting => !currHit && prevHit;
}

public struct Attackable : IComponentData
{
    public bool prevState;
    public bool currState;

    public bool IsAttacked => currState;
    public bool JustAttacked => currState && !prevState;
    public bool StoppedAttacked => !currState && prevState;
}

public struct Instanced : IComponentData
{

}

public struct Opening : IComponentData
{
    public float distance;
    public float cutoffDistance;
}

public struct InCameraView : IComponentData
{

}

public struct Ground : IComponentData
{

}

public struct Tree : IComponentData
{

}
public struct VisuallyCulled : IComponentData
{
    public float distance;
    public float cutoffDistance;
}

public class Anim : IComponentData
{
    public Animator animator;
}

//public class TransformContext : IComponentData
//{
//    public Transform transform;
//}

public class Visuals : IComponentData, IEquatable<Visuals>
{
    public MeshFilter filter;
    public MeshRenderer renderer;

    public bool Equals(Visuals other)
    {
        return filter == other.filter && renderer == other.renderer;
    }
}

public class PlantableTree : IComponentData
{
    public Entity prefab;
}
public struct GrowthComponent :  IComponentData
{
    public float growthSpeedMultiplier; // Increases when fertilized default should be 1
    public float timeTillFullyGrown;
    public float exclusionRadius;
}