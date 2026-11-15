using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Spawner spawner;
    public int currentWave = 0;
    public int maxWave = 2;
    public int aliveEnemies = 0;

    void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        // [1] go to the next wave
        currentWave = currentWave + 1;

        // [2] ask the spawner, and remember how many enemies we must kill
        aliveEnemies = spawner.SpawnWave(currentWave);
        Debug.Log("Wave " + currentWave + " start, enemies = " + aliveEnemies);
    }

    public void OnEnemyDied()
    {
        // [1] one less enemy
        aliveEnemies = aliveEnemies - 1;
        Debug.Log("Enemies left = " + aliveEnemies);

        // [2] no enemy left = the wave is cleared
        if (aliveEnemies <= 0)
        {
            Debug.Log("Wave " + currentWave + " cleared");
        }
    }
}