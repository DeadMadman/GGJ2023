using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

[System.Serializable]
public struct NamedSound
{
    [SerializeField] public string name;
    [SerializeField] public AudioClip sound;
}


public class SoundManager : MonoBehaviour, IComponentData
{
    private static EntityQuery query;
    public static EntityQuery Query => query;

    public static SoundManager Instance => query.GetSingleton<SoundManager>();

    [SerializeField] private List<NamedSound> sounds;
    private Dictionary<string, AudioClip> audioClipLookup = new();

    private void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        query = manager.CreateEntityQuery(typeof(SoundManager));
        manager.CreateSingleton(this, gameObject.name);

        foreach (var item in sounds) {
            audioClipLookup.Add(item.name, item.sound);
            //instancesParticleSystems.Add(item.name, new());
        }

    }

    private IEnumerator PlayAndCleanup(string name, Vector3 at, Quaternion rot, float volume)
    {
        if(audioClipLookup.ContainsKey(name)) {
            var source = new GameObject(name).AddComponent<AudioSource>();
            source.clip = audioClipLookup[name];
            source.loop = false;
            source.volume = volume;
            
            source.transform.position = at;
            source.transform.rotation = rot;
            
            source.Play();

            var wait = new WaitWhile(() => source != null && source.isPlaying);
            yield return wait;

            if (source != null) {
                DestroyImmediate(source.gameObject);
            }



        }
    }

    public void PlayOnce(string name, Vector3 at, Quaternion rot, float volume = 1.0f)
    {
        StartCoroutine(PlayAndCleanup(name, at, rot, volume));
    }
}
