using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering.VirtualTexturing;

[System.Serializable]
public struct NamedParticleSystem
{
    [SerializeField] public string name;
    [SerializeField] public ParticleSystem particleSystem;
}

public class ParticleSystemManager : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static ParticleSystemManager Instance => query.GetSingleton<ParticleSystemManager>();

    [SerializeField] private List<NamedParticleSystem> particleSystems;
    private Dictionary<string, ParticleSystem> particleSystemsLookup = new();

    private Dictionary<string, Dictionary<int, ParticleSystem>> instancesParticleSystems = new();
    private List<int> freeList = new();

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.CreateSingleton(this, "Particle System Manager");

        foreach(var item in particleSystems) {
            particleSystemsLookup.Add(item.name, item.particleSystem);
            instancesParticleSystems.Add(item.name, new());
        }

        query = manager.CreateEntityQuery(typeof(ParticleSystemManager));
    }

    public int Play(string name, Vector3 at, Quaternion rot)
    {
        var system = Instantiate(particleSystemsLookup[name]);

        var instances = instancesParticleSystems[name];
        
        int index;
        if(freeList.Count == 0) {
            index = instances.Count;
        }
        else {
            index = freeList[freeList.Count - 1];
            freeList.RemoveAt(index);
        }
        instances[index] = system;

        system.Play(true);
        system.transform.position = at;
        system.transform.rotation = rot;
        return index;
    }

    public void Transform(string name, int index, Vector3 at, Quaternion rot)
    {
        var instances = instancesParticleSystems[name];
        if (instances.ContainsKey(index)) {
            var system = instances[index];
            system.transform.position = at;
            system.transform.rotation = rot;
        }
    }


    public void Stop(string name, int index)
    {
        var instances = instancesParticleSystems[name];
        if(instances.ContainsKey(index)) {
            Destroy(instances[index]);
            instances.Remove(index);
            freeList.Add(index);
        }
    }
}
