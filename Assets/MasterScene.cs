using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var uiScene = SceneManager.GetSceneByBuildIndex(1);
        if (uiScene.isLoaded) {
            Debug.Log("Do nothing");
        }
        else {
            SceneManager.LoadScene("GameUI", LoadSceneMode.Additive);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
