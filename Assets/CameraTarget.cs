using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CameraTarget : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static CameraTarget Instance => query.GetSingleton<CameraTarget>();

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.CreateSingleton(this, "Camera Target");
        query = manager.CreateEntityQuery(typeof(CameraTarget));    
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetRotation(Quaternion quaternion)
    {
        transform.rotation = quaternion;
    }

}
