using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawn : MonoBehaviour
{
    [Header("Enemy Setup")]
    public List<GameObject> enemyPrefabs;

    [Header("Spawn Rate Settings")]
    public float initialSpawnRate = 1f; // Spawn rate awal (enemy per detik)
    public float spawnRateIncreaseInterval = 30f; // Setiap berapa detik spawn rate naik
    public float spawnRateIncrease = 0.5f; // Berapa banyak spawn rate naik
    public float maxSpawnRate = 10f; // Maksimum spawn rate

    [Header("Enemy Limit Settings")]
    public int initialMaxEnemyCount = 50; // Batasan awal maksimum enemy di scene
    public int maxEnemyCountIncrease = 10; // Berapa banyak max enemy bertambah setiap spawn rate naik
    public int absoluteMaxEnemyCount = 200; // Batasan mutlak maksimum enemy (untuk performa)
    public bool enableEnemyLimit = true; // Toggle untuk mengaktifkan batasan

    [Header("Enemy Stats Scaling")]
    public float healthMultiplierPerLevel = 1.2f; // Health naik 20% per level
    public float damageMultiplierPerLevel = 1.15f; // Damage naik 15% per level
    public float pointsMultiplierPerLevel = 1.1f; // Points naik 10% per level
    public bool enableStatsScaling = true; // Toggle untuk mengaktifkan scaling

    [Header("Map Settings")]
    public Tilemap backgroundTilemap;
    public Camera mainCamera;

    [Header("Debug Info")]
    public bool showDebugInfo = true;

    // Private variables
    private float currentSpawnRate;
    private int currentMaxEnemyCount; // Variabel untuk menyimpan max enemy saat ini
    private float timeSinceLastIncrease = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Bounds backgroundBounds;
    private bool isSpawning = false;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        currentMaxEnemyCount = initialMaxEnemyCount; // Initialize max enemy count

        if (backgroundTilemap != null)
        {
            backgroundBounds = backgroundTilemap.localBounds;
        }

        // Mulai spawn loop
        StartCoroutine(SpawnLoop());

        // Mulai cleanup coroutine untuk menghapus enemy yang sudah destroyed dari list
        StartCoroutine(CleanupEnemyList());
    }

    void Update()
    {
        // Update spawn rate seiring waktu
        UpdateSpawnRate();

        // Debug info
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }

    void UpdateSpawnRate()
    {
        timeSinceLastIncrease += Time.deltaTime;

        // Tingkatkan spawn rate setiap interval tertentu
        if (timeSinceLastIncrease >= spawnRateIncreaseInterval)
        {
            currentSpawnRate += spawnRateIncrease;
            currentSpawnRate = Mathf.Min(currentSpawnRate, maxSpawnRate);

            //  Tingkatkan max enemy count juga
            currentMaxEnemyCount += maxEnemyCountIncrease;
            currentMaxEnemyCount = Mathf.Min(currentMaxEnemyCount, absoluteMaxEnemyCount);

            timeSinceLastIncrease = 0f;

            if (showDebugInfo)
            {
                Debug.Log(
                    $"Difficulty increased! Level: {GetCurrentDifficultyLevel()}, Spawn rate: {currentSpawnRate:F1}/sec, Max enemies: {currentMaxEnemyCount}"
                );
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        isSpawning = true;

        while (isSpawning)
        {
            // Hitung berapa enemy yang akan di-spawn
            int enemiesToSpawn = Mathf.RoundToInt(currentSpawnRate);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // Cek apakah masih bisa spawn enemy (jika limit diaktifkan)
                if (enableEnemyLimit && GetActiveEnemyCount() >= currentMaxEnemyCount)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"Enemy limit reached ({currentMaxEnemyCount}). Skipping spawn.");
                    }
                    break; // Keluar dari loop spawn
                }

                // Spawn enemy jika ada prefab
                if (enemyPrefabs != null && enemyPrefabs.Count > 0)
                {
                    SpawnEnemy();
                }
            }

            // Wait selama 1 detik sebelum spawn berikutnya
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetValidSpawnPosition();

        // Pilih random enemy prefab
        GameObject selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // Instantiate enemy
        GameObject newEnemy = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        // Scale enemy stats berdasarkan difficulty level
        if (enableStatsScaling)
        {
            ScaleEnemyStats(newEnemy);
        }

        // Tambahkan ke list active enemies
        activeEnemies.Add(newEnemy);

        if (showDebugInfo)
        {
            Debug.Log($"Enemy spawned at {spawnPos}. Active enemies: {GetActiveEnemyCount()}");
        }
    }

    // Method untuk scaling enemy stats
    private void ScaleEnemyStats(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript == null)
            return;

        int difficultyLevel = GetCurrentDifficultyLevel();

        // Hitung multiplier berdasarkan difficulty level
        float healthMultiplier = Mathf.Pow(healthMultiplierPerLevel, difficultyLevel - 1);
        float damageMultiplier = Mathf.Pow(damageMultiplierPerLevel, difficultyLevel - 1);
        float pointsMultiplier = Mathf.Pow(pointsMultiplierPerLevel, difficultyLevel - 1);

        // Ambil base stats dari enemy
        int baseHealth = enemyScript.maxHealth;
        int baseDamage = enemyScript.damage;
        int basePoints = enemyScript.pointsReward;

        // Terapkan scaling
        int scaledHealth = Mathf.RoundToInt(baseHealth * healthMultiplier);
        int scaledDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        int scaledPoints = Mathf.RoundToInt(basePoints * pointsMultiplier);

        // Update enemy stats
        enemyScript.maxHealth = scaledHealth;
        enemyScript.currentHealth = scaledHealth;
        enemyScript.damage = scaledDamage;
        enemyScript.pointsReward = scaledPoints;

        if (showDebugInfo)
        {
            Debug.Log(
                $"Enemy scaled - Level: {difficultyLevel}, Health: {scaledHealth}, Damage: {scaledDamage}, Points: {scaledPoints}"
            );
        }
    }

    Vector2 GetValidSpawnPosition()
    {
        // Convert tilemap bounds to world space
        Vector3 bgMin = backgroundTilemap.CellToWorld(backgroundTilemap.cellBounds.min);
        Vector3 bgMax = backgroundTilemap.CellToWorld(backgroundTilemap.cellBounds.max);

        // Get camera world-space bounds
        Vector2 camMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0));
        Vector2 camMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));

        Vector2 spawnPos;
        int safety = 0;

        do
        {
            float x = Random.Range(bgMin.x, bgMax.x);
            float y = Random.Range(bgMin.y, bgMax.y);
            spawnPos = new Vector2(x, y);

            safety++;
            if (safety > 100)
                break;
        } while (
            spawnPos.x > camMin.x
            && spawnPos.x < camMax.x
            && spawnPos.y > camMin.y
            && spawnPos.y < camMax.y
        );

        return spawnPos;
    }

    // Coroutine untuk membersihkan list enemy yang sudah destroyed
    IEnumerator CleanupEnemyList()
    {
        while (true)
        {
            // Hapus enemy yang sudah null/destroyed dari list
            activeEnemies.RemoveAll(enemy => enemy == null);

            // Tunggu 2 detik sebelum cleanup berikutnya
            yield return new WaitForSeconds(2f);
        }
    }

    // Method untuk mendapatkan jumlah enemy aktif
    public int GetActiveEnemyCount()
    {
        // Bersihkan list terlebih dahulu
        activeEnemies.RemoveAll(enemy => enemy == null);
        return activeEnemies.Count;
    }

    // Method untuk mengatur spawn rate secara manual
    public void SetSpawnRate(float newRate)
    {
        currentSpawnRate = Mathf.Clamp(newRate, 0f, maxSpawnRate);
    }

    // Method untuk mengatur batasan enemy
    public void SetMaxEnemyCount(int newMax)
    {
        currentMaxEnemyCount = Mathf.Clamp(newMax, 1, absoluteMaxEnemyCount);
    }

    //  Method untuk reset difficulty ke awal
    public void ResetDifficulty()
    {
        currentSpawnRate = initialSpawnRate;
        currentMaxEnemyCount = initialMaxEnemyCount;
        timeSinceLastIncrease = 0f;

        if (showDebugInfo)
        {
            Debug.Log("Difficulty reset to initial values");
        }
    }

    //  Method untuk mendapatkan level kesulitan saat ini
    public int GetCurrentDifficultyLevel()
    {
        return Mathf.FloorToInt((currentSpawnRate - initialSpawnRate) / spawnRateIncrease) + 1;
    }

    //   Method untuk mendapatkan stats multiplier saat ini
    public void GetCurrentMultipliers(
        out float healthMult,
        out float damageMult,
        out float pointsMult
    )
    {
        int level = GetCurrentDifficultyLevel();
        healthMult = Mathf.Pow(healthMultiplierPerLevel, level - 1);
        damageMult = Mathf.Pow(damageMultiplierPerLevel, level - 1);
        pointsMult = Mathf.Pow(pointsMultiplierPerLevel, level - 1);
    }

    // Method untuk menghentikan spawn
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    // Method untuk memulai spawn lagi
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnLoop());
            StartCoroutine(CleanupEnemyList());
        }
    }

    // Method untuk menghapus semua enemy
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    // Method untuk force spawn enemy (bypass limit)
    public void ForceSpawnEnemy()
    {
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            SpawnEnemy();
        }
    }

    void UpdateDebugInfo()
    {
        // Update debug info bisa ditampilkan di UI atau console

        if (Time.frameCount % 300 == 0)
        {
            GetCurrentMultipliers(out float healthMult, out float damageMult, out float pointsMult);

            Debug.Log(
                $"Spawn Status - Level: {GetCurrentDifficultyLevel()}, Rate: {currentSpawnRate:F1}/sec, "
                    + $"Active Enemies: {GetActiveEnemyCount()}/{currentMaxEnemyCount}, "
                    + $"Multipliers - Health: {healthMult:F2}x, Damage: {damageMult:F2}x, Points: {pointsMult:F2}x"
            );
        }
    }

    // Gizmos untuk debugging
    void OnDrawGizmosSelected()
    {
        if (backgroundTilemap != null)
        {
            // Draw background bounds
            Gizmos.color = Color.green;
            Vector3 bgMin = backgroundTilemap.CellToWorld(backgroundTilemap.cellBounds.min);
            Vector3 bgMax = backgroundTilemap.CellToWorld(backgroundTilemap.cellBounds.max);

            Vector3 size = bgMax - bgMin;
            Vector3 center = bgMin + size * 0.5f;

            Gizmos.DrawWireCube(center, size);
        }

        if (mainCamera != null)
        {
            // Draw camera bounds
            Gizmos.color = Color.red;
            Vector2 camMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0));
            Vector2 camMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));

            Vector3 camSize = new Vector3(camMax.x - camMin.x, camMax.y - camMin.y, 0);
            Vector3 camCenter = new Vector3(
                (camMin.x + camMax.x) * 0.5f,
                (camMin.y + camMax.y) * 0.5f,
                0
            );

            Gizmos.DrawWireCube(camCenter, camSize);
        }
    }
}
