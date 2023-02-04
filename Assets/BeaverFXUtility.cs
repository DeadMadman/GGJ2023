using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeaverFXUtility : MonoBehaviour
{
    [SerializeField]
    private UnityEvent footstepEvent;

    public void PlayFootstepEvent()
    {
        footstepEvent.Invoke();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
