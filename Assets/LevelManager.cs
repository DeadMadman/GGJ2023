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

public class LevelManager : MonoBehaviour, IComponentData
{
    [SerializeField] private Collectable log;
    [SerializeField] private Collectable acorn;

    HashSet<Vector3Int> blocked = new();

    private static EntityQuery query;
    public static EntityQuery Query => query;
    public static LevelManager Instance => query.GetSingleton<LevelManager>();

    private Bounds bounds = new(Vector3.zero, Vector3.zero);
    public Bounds Bounds => bounds;

    [SerializeField] private List<BlockData> blockData;

    private Dictionary<string, BlockData> resources = new();

    [SerializeField] public Vector3 cubeSize;
   
    public Bounds GetBoundsWith(LocalToWorld transform)
    {
        return new Bounds(transform.Position, cubeSize);
    }

    public Vector3 GridToWorld(int x, int y, int z)
    {
        var half = (cubeSize * 0.5f);
        return Vector3.Scale(new Vector3(x, y, z), cubeSize) - half;
    }

    public Vector3Int WorldToGrid(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / cubeSize.x);
        int y = Mathf.RoundToInt(pos.y / cubeSize.y);
        int z = Mathf.RoundToInt(pos.z / cubeSize.z);
        return new(x, z, z);
    }

    public void SetBlocked(int x, int y, int z)
    {
        var that = new Vector3Int(x, y, z);
        if (blocked.Contains(that)) {
            return;
        }
        blocked.Add(that);
    }
    public void Unblock(Vector3Int that)
    {
        if (!blocked.Contains(that)) {
            return;
        }
        blocked.Remove(that);
    }

    public void Block(Vector3Int that)
    {
        if (blocked.Contains(that)) {
            return;
        }
        blocked.Add(that);
    }
    public bool IsBLocked(int x, int y, int z)
    {
        var that = new Vector3Int(x, y, z);
        return blocked.Contains(that);
    }

    public bool IsBLocked(Vector3Int that)
    {
        return blocked.Contains(that);
    }

    public Bounds GetBoundsWith(Vector3 pos)
    {
        return new Bounds(pos, cubeSize);
    }


    public BlockData Get(string name)
    {
        return resources[name];
    }


    private EntityArchetype cubeArchetype;

    private void Awake()
    {
        foreach(var block in blockData) {
            resources.Add(block.name, block);
            foreach(var materials in block.prefab.GetComponent<MeshRenderer>().sharedMaterials) {
                materials.enableInstancing = true;
            }
        }

        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        manager.CreateSingleton(this, gameObject.name);
        query = manager.CreateEntityQuery(typeof(LevelManager));

        cubeArchetype = manager.CreateArchetype(typeof(Instanced), typeof(LocalToWorld), typeof(LocalTransform), typeof(WorldTransform), typeof(ParentTransform));

        var ground = Create("Ground", GridToWorld(-8, 0, -8), Quaternion.identity, new Vector3Int(32, 1, 32));
        manager.AddComponent<Ground>(ground.AsArray());
        foreach (var entity in ground) {
            manager.AddComponentData(entity, new VisuallyCulled { distance = 18f, cutoffDistance = 2.5f });
        }
        
        var trees = Create("Tree", GridToWorld(0, 1, 0), Quaternion.identity, new Vector3Int(16, 1, 12));
        manager.AddComponent<Tree>(trees.AsArray());
        var mildSound = new FixedList512Bytes<FixedString128Bytes>();
        mildSound.Add("Hit");
        
        var strongSound = new FixedList512Bytes<FixedString128Bytes>();
        strongSound.Add("Explosion");

        foreach (var entity in trees) {
            manager.AddComponentData(entity, new VisuallyCulled { distance = 15.0f, cutoffDistance = 2.5f });
            manager.AddComponentData(entity, new HitFX { vfxName = "Explosion", mildSounds = mildSound, strongSounds = strongSound });
            manager.AddComponentData(entity, new Health { health = 1 });
            manager.AddComponentData(entity, new Dropping { acorn = acorn, log = log, chanceForAcorn = 0.08f, chanceForWood = 0.25f });
        }
        manager.AddComponent<Attackable>(trees);
    }

    public NativeList<Entity> Create(string name, Vector3 at, Quaternion rotation, Vector3Int count)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var resource = resources[name];
        var go = resource.prefab;

        NativeList<Entity> list = new NativeList<Entity>(Allocator.Temp);
        for (int z = 0; z < count.z; z++) {
            for (int y = 0; y < count.y; y++) {
                for (int x = 0; x < count.x; x++) {
                    var position = at + Vector3.Scale(new Vector3(x, y, z), cubeSize * 1.0f);

                    blocked.Add(new Vector3Int(x, y, z));

                    var entity = manager.CreateEntity(cubeArchetype);

                    list.Add(entity);
                    LocalTransform transform = new();
                    transform.Position = position;
                    transform.Rotation = rotation;
                    transform.Scale = 1.0f;

                    bounds.Encapsulate(new Bounds(position, cubeSize));

                    //manager.SetComponentData(entity, visuals);
                    manager.SetComponentData(entity, transform);
                }
            }
        }
        return list;
        //manager.AddComponentData(entity, new TransformContext { transform = go.transform });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void OnDestroy()
    {
        
    }

}
