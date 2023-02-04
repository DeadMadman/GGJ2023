using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlantedTree : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }

    public MeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            return meshRenderer;
        }
    }

    [SerializeField] public float exclusionRadius;
    [SerializeField] public float timeTillFullyGrown;
}

public class Baker : Baker<PlantedTree>
{
    public override void Bake(PlantedTree authoring)
    {
        Debug.Log("We got that dog in us");
        AddComponentObject(new Visuals { filter = authoring.MeshFilter, renderer = authoring.MeshRenderer });
        AddComponent(new LocalTransform { Position = authoring.transform.position, Rotation = authoring.transform.rotation, Scale = 1 });
        AddComponent(new GrowthComponent { exclusionRadius = authoring.exclusionRadius, growthSpeedMultiplier = 1, timeTillFullyGrown = authoring.timeTillFullyGrown });
        authoring.transform.localScale = new Vector3(0, 0, 0);
    }
}

public partial struct GrowthSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        foreach (var (tree, transform) in SystemAPI.Query<RefRW<GrowthComponent>, LocalTransform>())
        {
            if (tree.ValueRO.timeTillFullyGrown > 0)
                tree.ValueRW.timeTillFullyGrown -= dt * tree.ValueRO.growthSpeedMultiplier;
            float sizeMultiplier = 1 - tree.ValueRO.timeTillFullyGrown;
            transform.ApplyScale(10000000);
        }
    }
}