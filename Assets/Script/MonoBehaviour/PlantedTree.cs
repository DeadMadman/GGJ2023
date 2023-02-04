using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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

    [SerializeField] private float exclusionRadius;
    [SerializeField] private float timeTillFullyGrown;

    public class Baker : Baker<PlantedTree>
	{
        public override void Bake(PlantedTree authoring)
		{
            AddComponentObject(new Visuals { filter = authoring.MeshFilter, renderer = authoring.MeshRenderer });
            AddComponent(new GrowthComponent { /*exclusionRadius = authoring.exclusionRadius,*/ growthSpeedMultiplier = 1, timeTillFullyGrown = authoring.timeTillFullyGrown });
        }
	}
}

public partial struct GrowthSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        foreach (var tree in SystemAPI.Query<RefRW<GrowthComponent>>())
        {
            tree.ValueRW.timeTillFullyGrown -= dt * tree.ValueRO.growthSpeedMultiplier;
            // if timeTillFullyGrown == 0 then tree should become fully growns
        }
    }
}