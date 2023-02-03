using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;

    private VisualElement start;
    private VisualElement options;
    private VisualElement exit;
    // Start is called before the first frame update

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        start = root.Q<Button>("Start");
        options = root.Q<Button>("Options");
        exit = root.Q<Button>("Exit");
    }

    void OnEnable()
    {
        start.RegisterCallback<ClickEvent>(OnStart);
        options.RegisterCallback<ClickEvent>(OnOptions);
        exit.RegisterCallback<ClickEvent>(OnExit);
    }

    private void OnDisable()
    {
        start.UnregisterCallback<ClickEvent>(OnStart);
        options.UnregisterCallback<ClickEvent>(OnOptions);
        exit.UnregisterCallback<ClickEvent>(OnExit);
    }

    private void OnStart(ClickEvent e)
    {
        Debug.Log("Start");
    }

    private void OnOptions(ClickEvent e)
    {
        Debug.Log("Options");
    }

    private void OnExit(ClickEvent e)
    {
        Debug.Log("Exit");
    }
}
