using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial class DrawSystem : SystemBase
{

    private void Draw(NativeList<LocalToWorld> list, Mesh mesh, Material[] materials)
    {
        if(list.IsEmpty) {
            return;
        }

        var transforms = list.AsArray().Reinterpret<Matrix4x4>();
        for (int i = 0; i < mesh.subMeshCount; i++) {
            Graphics.RenderMeshInstanced(new RenderParams(materials[i]), mesh, i, transforms, transforms.Length);
        }
    }


    protected override void OnUpdate()
    {
        var target = CameraTarget.Instance;
        var cam = MainCamera.Instance;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam.camera);
        var level = LevelManager.Instance;

        foreach (var (transform, visuals) in SystemAPI.Query<LocalToWorld, Visuals>().WithNone<Instanced>()) {
            if (visuals != null) {
                var renderer = visuals.renderer;

                var mesh = visuals.filter.mesh;
                for (var index = 0; index < mesh.subMeshCount; index++) {
                    var material = renderer.materials[index];
                    RenderParams renderParams = new(material);

                    Graphics.RenderMesh(renderParams, mesh, index, transform.Value);
                }
            }
        }
        {
            var groundResource = LevelManager.Instance.Get("Ground");
            var go = groundResource.prefab;
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;

            NativeList<LocalToWorld> list = new NativeList<LocalToWorld>(Allocator.Temp);
            if (sharedMaterials != null && mesh != null) {
                foreach (var transform in SystemAPI.Query<LocalToWorld>().WithAll<Instanced, Ground>()) {
                    var bounds = level.GetBoundsWith(transform);
                    bounds.center = Vector3.LerpUnclamped(target.transform.position, bounds.center, 1.25f);
                    if (GeometryUtility.TestPlanesAABB(planes, bounds)) {
                        list.Add(transform);
                    }
                }
            }
            Draw(list, mesh, sharedMaterials);
        }

        {
            var groundResource = LevelManager.Instance.Get("Tree");
            var go = groundResource.prefab;
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;

            NativeList<LocalToWorld> list = new NativeList<LocalToWorld>(Allocator.Temp);
            if (sharedMaterials != null && mesh != null) {
                foreach (var transform in SystemAPI.Query<LocalToWorld>().WithAll<Instanced, Tree>()) {

                    var bounds = level.GetBoundsWith(transform);
                    bounds.center = Vector3.LerpUnclamped(target.transform.position, bounds.center, 1.5f);
                    if (GeometryUtility.TestPlanesAABB(planes, bounds)) {
                        list.Add(transform);
                    }
                }
            }
            Draw(list, mesh, sharedMaterials);
        }
        //var groundResource = LevelManager.Instance.Get("Ground");
        //var go = groundResource.prefab;
        //var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        //var sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;

        //NativeList<LocalToWorld> list = new NativeList<LocalToWorld>(Allocator.Temp);
        //if (sharedMaterials != null && mesh != null) {
        //    foreach (var transform in SystemAPI.Query<LocalToWorld>().WithAll<Instanced, Ground>()) {

        //        var bounds = mesh.bounds;
        //        bounds.center += (Vector3)transform.Position;

        //        if (GeometryUtility.TestPlanesAABB(planes, bounds)) {
        //            for (var index = 0; index < mesh.subMeshCount; index++) {
        //                list.Add(transform);
        //            }
        //        }
        //    }
        //}

        //var transforms = list.AsArray().Reinterpret<Matrix4x4>();
        //for (int i = 0; i < mesh.subMeshCount; i++) {
        //    Graphics.RenderMeshInstanced(new RenderParams(sharedMaterials[i]), mesh, i, transforms, transforms.Length);
        //}

    }
}
