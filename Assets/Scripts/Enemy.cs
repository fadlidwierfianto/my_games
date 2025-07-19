using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 20;
    public int pointsReward = 10;

    [Header("Death Settings")]
    public float deathDelay = 1f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State
    private bool isDead = false;

    // Reference ke spawn manager untuk tracking yang lebih baik
    private static EnemySpawn spawnManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;

        // Cari spawn manager jika belum ada
        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<EnemySpawn>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDirection)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        GivePointsToPlayer();

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        StartCoroutine(DestroyAfterDelay());
    }

    private void GivePointsToPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            player.AddPoints(pointsReward);
        }
    }

    public void GivePointsToPlayer(PlayerController targetPlayer)
    {
        if (targetPlayer != null)
        {
            targetPlayer.AddPoints(pointsReward);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);

        // Fade out effect
        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color startColor = spriteRenderer.color;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        // Notifikasi ke spawn manager bahwa enemy ini akan destroyed
        NotifySpawnManager();

        Destroy(gameObject);
    }

    // Method untuk memberitahu spawn manager
    private void NotifySpawnManager()
    {
        if (spawnManager != null)
        {
            // Spawn manager akan otomatis cleanup null references
            // Jadi tidak perlu action khusus di sini
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public int GetPointsReward()
    {
        return pointsReward;
    }

    public void SetPointsReward(int newPointsReward)
    {
        pointsReward = Mathf.Max(0, newPointsReward);
    }

    public void Heal(int healAmount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    // Method untuk mendapatkan spawn manager reference
    public static EnemySpawn GetSpawnManager()
    {
        return spawnManager;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
            float healthPercentage = GetHealthPercentage();

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                healthBarPos - Vector3.right * 0.5f,
                healthBarPos + Vector3.right * 0.5f
            );

            Gizmos.color = Color.green;
            Vector3 healthBarEnd =
                healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercentage;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarEnd);

#if UNITY_EDITOR
            Vector3 pointTextPos = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(pointTextPos, $"Points: {pointsReward}");
#endif
        }
    }
}
