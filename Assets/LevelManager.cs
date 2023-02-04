using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine.InputSystem;

[System.Serializable]
public struct BlockData
{
    public string name;
    public GameObject prefab;
}

public struct Ground : IComponentData
{

}

public class LevelManager : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static LevelManager Instance => query.GetSingleton<LevelManager>();

    [SerializeField] private List<BlockData> blockData;

    private Dictionary<string, BlockData> resources = new();

    private EntityArchetype cubeArchetype;

    private void Awake()
    {
        foreach(var block in blockData) {
            resources.Add(block.name, block);
        }

        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        manager.CreateSingleton(this, gameObject.name);
        query = manager.CreateEntityQuery(typeof(LevelManager));

        cubeArchetype = manager.CreateArchetype(typeof(Instanced), typeof(Visuals), typeof(LocalToWorld), typeof(LocalTransform), typeof(WorldTransform), typeof(ParentTransform));

        CreateGround(Vector3.zero, Quaternion.identity, new Vector3Int(64, 1, 64));
    }

    public void CreateGround(Vector3 at, Quaternion rotation, Vector3Int count)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var resource = resources["Ground"];
        var go = resource.prefab;

        Visuals visuals = new Visuals();
        visuals.filter = go.GetComponent<MeshFilter>();
        visuals.renderer = go.GetComponent<MeshRenderer>();
        var offset = visuals.filter.sharedMesh.bounds.size;
        for (int z = 0; z < count.z; z++) {
            for (int y = 0; y < count.y; y++) {
                for (int x = 0; x < count.x; x++) {
                    var position = at + Vector3.Scale(new Vector3(x, y, z), offset * 1.0f);

                    var entity = manager.CreateEntity(cubeArchetype);

                    manager.AddComponent<Ground>(entity);

                    LocalTransform transform = new();
                    transform.Position = position;
                    transform.Rotation = rotation;
                    transform.Scale = 1.0f;


                    manager.SetComponentData(entity, visuals);
                    manager.SetComponentData(entity, transform);
                }
            }
        }
        //manager.AddComponentData(entity, new TransformContext { transform = go.transform });
    }

    private void OnDestroy()
    {
        
    }

}
