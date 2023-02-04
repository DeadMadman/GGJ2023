using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial class DrawSystem : SystemBase
{
    protected override void OnUpdate()
    {
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

        foreach (var (transform, visuals) in SystemAPI.Query<LocalToWorld, Visuals>().WithAll<Instanced, Ground>()) {
            if (visuals != null) {
                var renderer = visuals.renderer;

                var mesh = visuals.filter.sharedMesh;
                for (var index = 0; index < mesh.subMeshCount; index++) {
                    var material = renderer.sharedMaterials[index];
                    RenderParams renderParams = new(material);

                    Graphics.RenderMesh(renderParams, mesh, index, transform.Value);
                }
            }
        }
    }
}
