using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

public struct Velocity : IComponentData
{
    public float3 value;
}

public struct Look : IComponentData
{
    public float3 value;
}


public struct Movement : IComponentData
{

}

public struct Walking : IComponentData
{

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
}

public class Visuals : IComponentData
{
    public Mesh mesh;
    public Material material;
    public Animator animator;
}


