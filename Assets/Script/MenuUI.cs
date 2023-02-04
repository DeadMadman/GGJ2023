using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
	
	public void LoadMenuScene()
	{
		StartCoroutine(FadeIn(0));
	}
	
	public void LoadGameScene()
	{
		StartCoroutine(FadeIn(1));
	}
	
    public void LoadScene(int sceneIndex)
    {
	    StartCoroutine(FadeIn(sceneIndex));
    }
    
    public void Quit()
    {
    #if UNITY_EDITOR
	    UnityEditor.EditorApplication.isPlaying = false;
    #else
		Application.Quit();
    #endif
    }
    	
    private IEnumerator WaitAndLoad(int nextSceneIndex)
    {
	    yield return new WaitUntil(() => faded);
	    SceneManager.LoadScene(nextSceneIndex);
	    yield return null;
	}
    
    [SerializeField] private Image image;
    //private static readonly int Cut = Shader.PropertyToID("_Cut");
    private bool faded = false;
    
    private void Start()
    {
	    image.gameObject.SetActive(true);
    }

    private float startOpacity = -2f;
    private float endOpasity = 100f;
    private float duration = 1f;
    private float lerpValue;
    
    IEnumerator FadeIn(int nextSceneIndex)
    {
	    faded = false;
	    float timeElapsed = 0f;

	    while (timeElapsed < duration)
	    {
		    lerpValue = Mathf.Lerp(startOpacity, endOpasity, timeElapsed / duration);
		    timeElapsed += Time.deltaTime;
			//image.material.SetFloat(Cut, lerpValue);
			var imageColor = image.color;
			imageColor.a = lerpValue;
			
		    yield return null;
	    }
	    lerpValue = endOpasity;
	    SceneManager.LoadScene(nextSceneIndex);
    }
}
