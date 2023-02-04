using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[System.Serializable]
public struct BlockData
{
    public string name;
    public Mesh mesh;
    public Material material;
}

public class LevelManager : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static LevelManager Instance => query.GetSingleton<LevelManager>();

    [SerializeField] private List<BlockData> blockData;

    private Dictionary<string, Mesh> meshes = new();
    private Dictionary<string, Material> materials = new();

    private EntityArchetype cubeArchetype;

    private void Awake()
    {
        foreach(var block in blockData) {
            meshes.Add(block.name, block.mesh);
            materials.Add(block.name, block.material);
        }

        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        manager.CreateSingleton(this, gameObject.name);
        query = manager.CreateEntityQuery(typeof(LevelManager));

        cubeArchetype = manager.CreateArchetype(typeof(Visuals), typeof(LocalToWorld), typeof(LocalTransform), typeof(WorldTransform), typeof(ParentTransform));

        CreateCube("Ground", Vector3.zero, Quaternion.identity);
    }

    public void CreateCube(string name, Vector3 at, Quaternion rotation)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = manager.CreateEntity(cubeArchetype);

        LocalTransform transform = new();
        transform.Position = at;
        transform.Rotation = rotation;
        transform.Scale = 1.0f;

        //Visuals visuals = new Visuals();
        //visuals.material = materials[name];
        //visuals.mesh = meshes[name];

        //manager.SetComponentData(entity, visuals);
        manager.SetComponentData(entity, transform);

    }

    private void OnDestroy()
    {
        
    }

}
