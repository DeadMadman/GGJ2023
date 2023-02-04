using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioManager manager;

    void PlayAudio()
    {
        StartCoroutine(PlayClip());
    }
    
    IEnumerator PlayClip()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        Destroy(audioSource);
    }


}
