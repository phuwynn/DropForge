using UnityEngine;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public Spawner spawner;
    public Door door;
    public int currentWave = 0;
    public int maxWave = 2;
    public int aliveEnemies = 0;
    public bool waveCleared = false;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        // test key (editor only): K = kill every enemy
        if (DevKeys.Enabled && kb.kKey.wasPressedThisFrame)
        {
            KillAllEnemies();
        }
    }

    public void StartNextWave()
    {
        // [1] go to the next wave
        currentWave = currentWave + 1;
        waveCleared = false;

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
            waveCleared = true;
            Debug.Log("Wave " + currentWave + " cleared");

            if (currentWave >= maxWave)
            {
                // [3] the last wave is cleared = the player wins
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.Win();
                }
            }
            else
            {
                // [4] not the last wave = open the door to the next wave
                if (door != null)
                {
                    door.SetOpen(true);
                }
            }
        }
    }

    void KillAllEnemies()
    {
        Enemy[] list = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < list.Length; i++)
        {
            list[i].Die();
        }
    }
}