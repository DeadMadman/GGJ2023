using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    public Mesh Mesh => mesh;
    public Material Material => material;



    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            AddComponent<Movement>();
            AddComponentObject(new Visuals { mesh = authoring.Mesh, material = authoring.Material });
            //AddComponent<LocalTransform>();
            //AddComponent<ParentTransform>();
            //AddComponent<WorldTransform>();
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

