using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MainCamera : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static MainCamera Instance => query.GetSingleton<MainCamera>();

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.CreateSingleton(this, gameObject.name);
        query = manager.CreateEntityQuery(typeof(MainCamera));
    }
}
