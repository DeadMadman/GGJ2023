using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering.VirtualTexturing;
using System;

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

    private Dictionary<int, ParticleSystem> instancesParticleSystems = new();
    private List<int> freeList = new();

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.CreateSingleton(this, gameObject.name);

        foreach(var item in particleSystems) {
            particleSystemsLookup.Add(item.name, item.particleSystem);
            //instancesParticleSystems.Add(item.name, new());
        }

        query = manager.CreateEntityQuery(typeof(ParticleSystemManager));
    }

    private IEnumerator PlayAndCleanup(string name, Vector3 at, Quaternion rot)
    {
        var system = Instantiate(particleSystemsLookup[name]);
        system.Play();
        system.transform.position = at;
        system.transform.rotation = rot;
        yield return new WaitWhile(() => system.isPlaying);
        DestroyImmediate(system.gameObject);
    }

    public void PlayOnce(string name, Vector3 at, Quaternion rot)
    {
        StartCoroutine(PlayAndCleanup(name, at, rot));
    }

    public int Play(string name, Vector3 at, Quaternion rot)
    {
        var system = Instantiate(particleSystemsLookup[name]);

        int index;
        if(freeList.Count == 0) {
            index = instancesParticleSystems.Count;
        }
        else {
            index = freeList[freeList.Count - 1];
            freeList.RemoveAt(freeList.Count - 1);
        }
        instancesParticleSystems[index] = system;

        system.Play(true);
        system.transform.position = at;
        system.transform.rotation = rot;
        return index;
    }



    public void Transform(int index, Vector3 at, Quaternion rot)
    {
        if (instancesParticleSystems.ContainsKey(index)) {
            var system = instancesParticleSystems[index];
            system.transform.position = at;
            system.transform.rotation = rot;
        }
    }


    public void Stop(int index)
    {
        if(instancesParticleSystems.ContainsKey(index)) {
            DestroyImmediate(instancesParticleSystems[index].gameObject);
            instancesParticleSystems.Remove(index);
            freeList.Add(index);
        }
    }
}
