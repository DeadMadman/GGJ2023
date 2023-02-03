using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Movement : IComponentData
{

}

public class Visuals : IComponentData
{
    public Mesh mesh;
    public Material material;
}

