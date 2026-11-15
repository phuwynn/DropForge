using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Enemy slimePrefab;
    public Enemy zombiePrefab;
    public Enemy skeletonPrefab;
    public int slimeCount = 5;
    public int zombieCount = 7;
    public int skeletonCount = 3;

    // resource nodes (wave 1 only)
    public ResourceNode ironOrePrefab;
    public ResourceNode cottonPrefab;
    public int ironOreCount = 7;
    public int cottonCount = 5;

    // the area where things can appear
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    public float spawnDelay = 0.3f;

    Queue<Enemy> spawnQueue = new Queue<Enemy>();
    float spawnTimer = 0;

    void Update()
    {
        // [1] nothing left in the queue = nothing to spawn
        if (spawnQueue.Count == 0)
        {
            return;
        }

        // [2] wait for the timer, then take one enemy out of the queue
        spawnTimer = spawnTimer - Time.deltaTime;
        if (spawnTimer <= 0)
        {
            Enemy prefab = spawnQueue.Dequeue();
            Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
            spawnTimer = spawnDelay;
        }
    }

    // fill the queue and return how many enemies this wave has
    public int SpawnWave(int wave)
    {
        spawnQueue.Clear();

        if (wave == 1)
        {
            AddToQueue(slimePrefab, slimeCount);
            AddToQueue(zombiePrefab, zombieCount);
            AddToQueue(skeletonPrefab, skeletonCount);

            // nodes are not enemies, so they are not counted in the return value
            SpawnNodes(ironOrePrefab, ironOreCount);
            SpawnNodes(cottonPrefab, cottonCount);
        }
        else
        {
            Debug.Log("Wave " + wave + " is not set yet");
        }

        return spawnQueue.Count;
    }

    void AddToQueue(Enemy prefab, int count)
    {
        // no prefab = skip
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            spawnQueue.Enqueue(prefab);
        }
    }

    void SpawnNodes(ResourceNode prefab, int count)
    {
        // no prefab = skip
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
        }
    }

    Vector2 GetRandomPosition()
    {
        Player player = FindFirstObjectByType<Player>();
        Vector2 position = Vector2.zero;

        // try up to 10 times to find a spot far from the player
        for (int i = 0; i < 10; i++)
        {
            position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

            if (player == null)
            {
                break;
            }
            if (Vector2.Distance(position, player.transform.position) > 3f)
            {
                break;
            }
        }
        return position;
    }
}