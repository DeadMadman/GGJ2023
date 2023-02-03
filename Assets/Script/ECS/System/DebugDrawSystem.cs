using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
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
            foreach(var (transform, visuals) in SystemAPI.Query<LocalToWorld, Visuals>()) {
                var material = visuals.material;
                if (visuals.mesh != null && material != null) {
                    RenderParams renderParams = new(material);
                    Graphics.RenderMesh(renderParams, visuals.mesh, 0, transform.Value);
                }
            }
        }
    }
}
