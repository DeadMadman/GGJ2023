using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour, IComponentData
{
    private void Awake()
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton(this, "Scorn Manager");
    }

    [SerializeField] private TMP_Text logText;
    [SerializeField] private TMP_Text acornText;

    [SerializeField] private Image logImg;
    [SerializeField] private Image acornImg;

    private int logCount = 0;
    private int acornCount = 0;

    public int AcornCount => acornCount;

    private float startScale = 1f;
    private float endScale = 1.6f;
    private float scaleLerpValue;
    float scaleDuration = 0.1f;

    public void AddLog(int num)
    {
        logCount += num;
        logText.text = logCount.ToString();
        StartCoroutine(AnimateIcon(logImg));
    }
    
    public void AddAcorn(int num)
    {
        acornCount += num;
        acornText.text = acornCount.ToString();
        StartCoroutine(AnimateIcon(acornImg));
    }

    private void Update()
    {
        if (Keyboard.current[Key.K].wasPressedThisFrame)
        {
            AddAcorn(1);
        }
        if (Keyboard.current[Key.L].wasPressedThisFrame)
        {
            AddLog(1);
        }
    }

    IEnumerator AnimateIcon(Image img)
    {
        float timeElapsed = 0f;
        scaleLerpValue = startScale;
        while (timeElapsed < scaleDuration)
        {
            scaleLerpValue = Mathf.Lerp(startScale, endScale, timeElapsed / scaleDuration);
            timeElapsed += Time.unscaledDeltaTime;
            img.rectTransform.localScale = new Vector2(scaleLerpValue, scaleLerpValue);
            yield return null;
        }
        scaleLerpValue = endScale;
        
        timeElapsed = 0f;
        while (timeElapsed < scaleDuration)
        {
            scaleLerpValue = Mathf.Lerp(endScale, startScale, timeElapsed / scaleDuration);
            timeElapsed += Time.unscaledDeltaTime;
            img.rectTransform.localScale = new Vector2(scaleLerpValue, scaleLerpValue);
            yield return null;
        }
        scaleLerpValue = startScale;
    }
}
