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
using UnityEngine.Tilemaps;

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

            // Add Rigidbody2D and CompositeCollider2D for solid collision
            if (propvi.GetComponent<Rigidbody2D>() == null)
            {
                // Add Rigidbody2D
                Rigidbody2D rb = propvi.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static; // Static so props don't move
            }

            // Add BoxCollider2D if no collider exists
            if (propvi.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D boxCollider = propvi.AddComponent<BoxCollider2D>();
                boxCollider.usedByComposite = true; // Required for CompositeCollider2D
            }

            if (propvi.GetComponent<CompositeCollider2D>() == null)
            {
                // Add CompositeCollider2D
                CompositeCollider2D compositeCollider = propvi.AddComponent<CompositeCollider2D>();
                compositeCollider.isTrigger = false; // Make sure it blocks movement
            }
        }
    }
}
