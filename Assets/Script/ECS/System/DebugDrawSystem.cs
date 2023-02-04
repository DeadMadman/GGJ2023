using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct DrawSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        {
            foreach (var (transform, visuals) in SystemAPI.Query<LocalToWorld, Visuals>()) {
                if(visuals != null) {
                    var renderer = visuals.renderer;

                    if (visuals.filter != null && renderer != null) {
                        var mesh = visuals.filter.mesh;
                        for(var index = 0; index < mesh.subMeshCount; index++) {
                            var material = renderer.materials[index];
                            RenderParams renderParams = new(material);

                            Graphics.RenderMesh(renderParams, mesh, index, transform.Value);
                        }

                    }
                }
            }
        }
    }
}
