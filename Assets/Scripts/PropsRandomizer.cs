// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Mathematics;
// using UnityEngine;

// public class PropsRandomizer : MonoBehaviour
// {
//     [SerializeField]
//     private List<GameObject> propSpawnPoints;

//     // [SerializeField]
//     // private List<GameObject> invPrefabs;

//     [SerializeField]
//     private List<GameObject> viPrefabs;

//     // Start is called before the first frame update
//     void Start()
//     {
//         SpawnProps();
//     }

//     // Update is called once per frame
//     void Update() { }

//     void SpawnProps()
//     {
//         foreach (GameObject sp in propSpawnPoints)
//         {
//             int randvi = UnityEngine.Random.Range(0, viPrefabs.Count);
//             GameObject propvi = Instantiate(
//                 viPrefabs[randvi],
//                 sp.transform.position,
//                 Quaternion.identity
//             );

//             propvi.transform.parent = sp.transform;
//         }
//     }
// }

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

            // Set the layer to Default (layer 0)
            propvi.layer = 0;

            // Set the sorting order to -1 for SpriteRenderer components
            SpriteRenderer spriteRenderer = propvi.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = -1;
            }

            // If the prop has child objects with SpriteRenderers, set their sorting order too
            SpriteRenderer[] childRenderers = propvi.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in childRenderers)
            {
                renderer.sortingOrder = -1;
            }

            // Add collider to make the prop solid (blocks player movement)
            if (propvi.GetComponent<Collider2D>() == null)
            {
                // Add BoxCollider2D if no collider exists
                BoxCollider2D collider = propvi.AddComponent<BoxCollider2D>();
                // Make sure it's not a trigger so it blocks movement
                collider.isTrigger = false;
            }
        }
    }
}
