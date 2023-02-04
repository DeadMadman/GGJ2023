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
    public bool plantButton;
}

public struct Instanced : IComponentData
{

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
    public GameObject prefab;
}
public struct GrowthComponent :  IComponentData
{
    public float growthSpeedMultiplier; // Increases when fertilized default should be 1
    public float timeTillFullyGrown;
    //public int growthLevel;
}