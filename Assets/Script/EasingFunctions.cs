using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasingFunctions : MonoBehaviour
{
    public void SpawnIn(GameObject obj)
    {
        StartCoroutine(RunSpawnIn(obj));
    }
    
    IEnumerator RunSpawnIn(GameObject obj)
    {
        float startScale = 0f;
        float endScale = 1f;
        float lerpValue = startScale;
        float timeElapsed = 0f;
        float scaleDuration = 1f;
   
        while (timeElapsed < scaleDuration)
        {
            lerpValue = Mathf.Lerp(startScale, endScale, timeElapsed / scaleDuration);
            timeElapsed += Time.unscaledDeltaTime;
            obj.transform.localScale = new Vector3(lerpValue, lerpValue, lerpValue);
            yield return null;
        }
        lerpValue = endScale;
        obj.transform.localScale = new Vector3(lerpValue, lerpValue, lerpValue);
    }
    
    public void LiftUp(GameObject obj)
    {
        StartCoroutine(RunLift(obj));
    }
    
    IEnumerator RunLift(GameObject obj)
    {
        float startScale = 0f;
        float endScale = 1f;
        float lerpValue = startScale;
        float timeElapsed = 0f;
        float scaleDuration = 1f;

        Vector3 position;
        while (timeElapsed < scaleDuration)
        {
            lerpValue = Mathf.Lerp(startScale, endScale, timeElapsed / scaleDuration);
            timeElapsed += Time.unscaledDeltaTime;
            position = obj.transform.position;
            position = new Vector3( position.x,  position.y, lerpValue);
            obj.transform.position = position;
            yield return null;
        }
        lerpValue = endScale;
        position = obj.transform.position;
        position = new Vector3( position.x,  position.y, lerpValue);
    }
}
