using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial struct OpeningSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var target = CameraTarget.Instance;
        foreach (var (transform, open) in SystemAPI.Query<RefRW<LocalTransform>, Opening>()) {
            ref var transformRef = ref transform.ValueRW;

            var distance = math.distance(target.transform.position, transformRef.Position) - open.cutoffDistance;
            var fraction = distance / open.distance;
            fraction = math.smoothstep(0.0f, 1.0f, fraction);

            transformRef.Scale = math.clamp(fraction, 0.0f, 1.0f);
        }
    }
}

public partial struct VisualCullingSystem : ISystem
{

    // LMAO, NEW YORK; EH?!
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float Manhattan(float3 a, float3 b)
    {
        var x = math.abs(a.x - b.x);
        var y = math.abs(a.y - b.y);
        var z = math.abs(a.z - b.z);
        return x + y + z;
    }

    private NativeList<Entity> removeList;
    public void OnCreate(ref SystemState state) 
    {
        removeList = new NativeList<Entity>(Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state) {
        removeList.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var level = LevelManager.Instance;
        var cam = MainCamera.Instance;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam.camera);

        var target = CameraTarget.Instance;
        removeList.Clear();
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess()) {
            ref var transformRef = ref transform.ValueRW;

            var bounds = level.GetBoundsWith(transformRef.Position);
            if (GeometryUtility.TestPlanesAABB(planes, bounds)) {
                removeList.Add(entity);
            }
        }
        state.EntityManager.AddComponent<InCameraView>(removeList);

        foreach (var (transform, vs) in SystemAPI.Query<RefRW<LocalTransform>, VisuallyCulled>().WithAll<InCameraView>()) {
            ref var transformRef = ref transform.ValueRW;

            var distance = Manhattan(target.transform.position, transformRef.Position) - vs.cutoffDistance;
            var fraction = distance / vs.distance;
            fraction = 1.0f - math.smoothstep(0.0f, 1.0f, fraction);
            if(fraction > 0.60) {
                transformRef.Scale = vs.scale * 1.0f;
            }
            else if(fraction > 0.5f) {
                transformRef.Scale = vs.scale * 0.75f;
            }
            else if (fraction > 0.25f) {
                transformRef.Scale = vs.scale * 0.5f;
            }
            else {
                transformRef.Scale = 0;
            }
        }

    }
}


public partial class DrawSystem : SystemBase
{

    private void Draw(NativeArray<LocalToWorld> list, Mesh mesh, Material[] materials)
    {
        if(list.Length == 0) {
            return;
        }

        var transforms = list.Reinterpret<Matrix4x4>();
        for (int i = 0; i < mesh.subMeshCount; i++) {
            var rp = new RenderParams(materials[i]);
            rp.layer = -1000;
    
            Graphics.RenderMeshInstanced(new RenderParams(materials[i]), mesh, i, transforms, transforms.Length);
        }
    }


    protected override void OnUpdate()
    {
        var target = CameraTarget.Instance;
        var cam = MainCamera.Instance;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam.camera);
        var level = LevelManager.Instance;

        {
            var groundResource = LevelManager.Instance.Get("Ground");
            var go = groundResource.prefab;
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;
            
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp);
            var query = builder.WithAll<Instanced, Ground, InCameraView>().WithAll<LocalToWorld>().Build(this);
            //var query = GetEntityQuery(builder.WithAll<Instanced, Ground, InView>().WithAll<LocalToWorld>());
            var transforms = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            Draw(transforms, mesh, sharedMaterials);
            
            EntityManager.RemoveComponent<InCameraView>(query.ToEntityArray(Allocator.Temp));
        }

        {
            var groundResource = LevelManager.Instance.Get("Tree");
            var go = groundResource.prefab;
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;

            var sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp);
            var query = builder.WithAll<Instanced, Tree, InCameraView>().WithAll<LocalToWorld>().Build(this);
            //var query = GetEntityQuery(builder.WithAll<Instanced, Tree, InView>().WithAll<LocalToWorld>());
            var transforms = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            Draw(transforms, mesh, sharedMaterials);

            EntityManager.RemoveComponent<InCameraView>(query.ToEntityArray(Allocator.Temp));
        }

        foreach (var (transform, visuals) in SystemAPI.Query<LocalToWorld, Visuals>().WithNone<Instanced>()) {
            if (visuals != null) {
                var renderer = visuals.renderer;

                var filter = visuals.filter;
                for (var index = 0; index < filter.mesh.subMeshCount; index++) {
                    var material = renderer.materials[index];
                    RenderParams renderParams = new(material);

                    Graphics.RenderMesh(renderParams, filter.mesh, index, transform.Value);
                }
            }
        }

        //foreach (var (transform, skin) in SystemAPI.Query<LocalToWorld, SkinnedMesh>().WithNone<Instanced>()) {
        //    if (skin != null) {
        //        var renderer = skin.skinnedMeshRenderer;

        //        for (var index = 0; index < renderer.sharedMesh.subMeshCount; index++) {
        //            var material = renderer.materials[index];
        //            RenderParams renderParams = new(material);

        //            renderer.BakeMesh(renderer.sharedMesh);
        //            Graphics.RenderMesh(renderParams, renderer.sharedMesh, index, transform.Value);
        //        }
        //    }
        //}
    }
}
