using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField] private float cooldown;

    private float timer = 0.0f;

    void Start()
    {
        timer = cooldown;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        while(timer < 0.0f) {
            timer += cooldown;
            var level = LevelManager.Instance;
            var bounds = level.Bounds;
            var min = bounds.min;
            var max = bounds.max;

            float x = UnityEngine.Random.Range(min.x, max.x);
            float z = UnityEngine.Random.Range(min.z, max.z);

            var go = Instantiate(prefab, new Vector3(Mathf.Round(x), 0.5f, Mathf.Round(z)), Quaternion.identity);
        }
    }
}
