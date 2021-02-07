using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawn : MonoBehaviour
{

    public GameObject zombiePrefab;
    public int number;
    public float spawnRadius;

     [SerializeField] private bool SpawnOnStart = true;

    // Start is called before the first frame update
    void Start()
    {
        if (SpawnOnStart)
        {
            SpawnAll();
        }
    }

    void SpawnAll()
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab, hit.position, Quaternion.identity);
            }
            else
            {
                i--;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!SpawnOnStart && other.gameObject.CompareTag("Player"))
        {
            SpawnAll();
        }
    }
}
