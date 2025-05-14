using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PropsRandomizer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> propSpawnPoints;

    // [SerializeField]
    // private List<GameObject> invPrefabs;

    [SerializeField]
    private List<GameObject> viPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        SpawnProps();
    }

    // Update is called once per frame
    void Update() { }

    void SpawnProps()
    {
        foreach (GameObject sp in propSpawnPoints)
        {
            int randvi = UnityEngine.Random.Range(0, viPrefabs.Count);
            GameObject propvi = Instantiate(
                viPrefabs[randvi],
                sp.transform.position,
                Quaternion.identity
            );

            propvi.transform.parent = sp.transform;
        }
    }
}
