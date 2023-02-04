using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlantedTree : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public partial struct GrowthSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        foreach (var tree in SystemAPI.Query<RefRW<GrowthComponents>>())
        {
            tree.ValueRW.timeTillFullyGrown -= dt * tree.ValueRO.growthSpeedMultiplier;
            // if timeTillFullyGrown == 0 then tree should become fully growns
        }
    }
}