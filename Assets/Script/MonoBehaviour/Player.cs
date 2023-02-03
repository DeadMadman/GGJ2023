using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Player : MonoBehaviour
{
    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            AddComponent<Movement>();
            AddComponent<LocalTransform>();
            AddComponent<ParentTransform>();
            AddComponent<WorldTransform>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial struct PlayerBakingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {   
        
    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {

    }
}