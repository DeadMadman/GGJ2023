using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
	//Fade
	[SerializeField] private Image fadingImage;
	//private static readonly int Cut = Shader.PropertyToID("_Cut");
    
	private float startOpacity = 0f;
	private float endOpasity;
	private float duration = 0.5f;
	private float fadeLerpValue;
    
	//Animated icon
	[SerializeField] private Image animatedImage;
    
	private float startScale = 0.9f;
	private float endScale = 1f;
	private float scaleLerpValue;
	float scaleDuration = 1f;
	
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private GameObject pauseMenu;

	private void Start()
    {
	    PauseGame();
	    mainMenu.SetActive(true);
	    endOpasity = fadingImage.color.a;
	    fadingImage.gameObject.SetActive(true);
	    StartCoroutine(FadeOut());
	   
	    Animate();
    }
	
    public void Animate()
    {
	    StartCoroutine(AnimateIcon());
    }
    
    public void LoadMenuScene()
    {
	    PauseGame();
	    StartCoroutine(FadeIn(mainMenu));
    }
	
    public void LoadPlayScreen()
    {
	    UnpauseGame();
	    pauseMenu.SetActive(false);
	    StartCoroutine(FadeTransition(mainMenu));
    }
    
    public void ResumeGamecreen()
    {
	    UnpauseGame();
	    pauseMenu.SetActive(false);
	    //StartCoroutine(FadeTransition(mainMenu));
    }

    public void LoadPauseScreen()
    {
	    PauseGame();
	    pauseMenu.SetActive(true);
    }

    private void Update()
    {
	    if (Keyboard.current[Key.P].wasPressedThisFrame)
	    {
		    if (!pauseMenu.activeSelf)
		    {
			    LoadPauseScreen();
		    }
	    }
	    if (Keyboard.current[Key.Escape].wasPressedThisFrame)
	    {
		    if (!pauseMenu.activeSelf)
		    {
				LoadPauseScreen();
		    }
	    }
    }

    void PauseGame()
    {
	    Time.timeScale = 0f;
    }

    void UnpauseGame()
    {
	    Time.timeScale = 1f;
    }
    
    public void Quit()
    {
#if UNITY_EDITOR
	    UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
    
    IEnumerator AnimateIcon()
    {
	    float timeElapsed = 0f;
	    scaleLerpValue = startScale;
	    while (timeElapsed < scaleDuration)
	    {
		    scaleLerpValue = Mathf.Lerp(startScale, endScale, timeElapsed / scaleDuration);
		    timeElapsed += Time.unscaledDeltaTime;
		    animatedImage.rectTransform.localScale = new Vector2(scaleLerpValue, scaleLerpValue);
		    yield return null;
	    }
	    scaleLerpValue = endScale;
	    
	    timeElapsed = 0f;
	    scaleLerpValue = endScale;
	    while (timeElapsed < scaleDuration)
	    {
		    scaleLerpValue = Mathf.Lerp(endScale, startScale, timeElapsed / scaleDuration);
		    timeElapsed += Time.unscaledDeltaTime;
		    animatedImage.rectTransform.localScale = new Vector2(scaleLerpValue, scaleLerpValue);
		    yield return null;
	    }
	    scaleLerpValue = startScale;
	    StartCoroutine(AnimateIcon());
    }
    
    IEnumerator FadeTransition(GameObject obj)
    {
	    float timeElapsed = 0f;
	    fadeLerpValue = startOpacity;
	    fadingImage.color = new Color(fadingImage.color .r, fadingImage.color .g, fadingImage.color.b, fadeLerpValue);
	    fadingImage.gameObject.SetActive(true);
	    
	    while (timeElapsed < duration)
	    {
		    fadeLerpValue = Mathf.Lerp(startOpacity, endOpasity, timeElapsed / duration);
		    timeElapsed += Time.unscaledDeltaTime;
		    //image.material.SetFloat(Cut, lerpValue);
		    fadingImage.color = new Color(fadingImage.color .r, fadingImage.color .g, fadingImage.color.b, fadeLerpValue);
			
		    yield return null;
	    }
	    fadeLerpValue = endOpasity;
	    
	    fadingImage.gameObject.SetActive(false);
	    obj.SetActive(false);

	    StartCoroutine(FadeOut());
    }
    
    IEnumerator FadeIn(GameObject obj)
    {
	    float timeElapsed = 0f;
	    fadeLerpValue = startOpacity;
	    fadingImage.color = new Color(fadingImage.color .r, fadingImage.color .g, fadingImage.color.b, fadeLerpValue);
	    fadingImage.gameObject.SetActive(true);
	    
	    while (timeElapsed < duration)
	    {
		    fadeLerpValue = Mathf.Lerp(startOpacity, endOpasity, timeElapsed / duration);
		    timeElapsed += Time.unscaledDeltaTime;
			//image.material.SetFloat(Cut, lerpValue);
			fadingImage.color = new Color(fadingImage.color .r, fadingImage.color .g, fadingImage.color.b, fadeLerpValue);
			
		    yield return null;
	    }
	    fadeLerpValue = endOpasity;
	    
	    fadingImage.gameObject.SetActive(false);
	    obj.SetActive(true);
	    
	    StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
	    float timeElapsed = 0f;
	    fadeLerpValue = endOpasity;
	    fadingImage.color = new Color(fadingImage.color .r, fadingImage.color .g, fadingImage.color.b, fadeLerpValue);
	    fadingImage.gameObject.SetActive(true);
	    
	    while (timeElapsed < duration)
	    {
		    fadeLerpValue = Mathf.Lerp(endOpasity, startOpacity, timeElapsed / duration);
		    timeElapsed += Time.unscaledDeltaTime;
		    //image.material.SetFloat(Cut, lerpValue);
		    fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, fadeLerpValue);

		    yield return null;
	    }
	    fadeLerpValue = startOpacity;
	    
	    fadingImage.gameObject.SetActive(false);
    }
}
