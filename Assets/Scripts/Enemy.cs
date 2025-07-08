using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 20;
    public float knockbackForce = 5f;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.3f;
    public float knockbackResistance = 1f; // Multiplier untuk knockback strength

    [Header("Death Settings")]
    public float deathDelay = 1f; // Delay sebelum destroy object

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State
    private bool isDead = false;
    private bool isKnockedBack = false;
    private Vector2 originalVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Jangan damage player jika enemy sudah mati
        if (isDead)
            return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
            player.TakeDamage(damage, knockbackDir);
        }
    }

    // Implementasi IDamageable interface
    public void TakeDamage(int damageAmount, Vector2 knockbackDirection)
    {
        // Jangan terima damage jika sudah mati
        if (isDead)
            return;

        // Kurangi health
        currentHealth -= damageAmount;

        // Debug log untuk testing
        Debug.Log(
            $"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}"
        );

        // Visual feedback
        StartCoroutine(DamageFlash());

        // Apply knockback
        ApplyKnockback(knockbackDirection);

        // Cek apakah enemy mati
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        if (rb != null && !isKnockedBack)
        {
            // Simpan velocity asli jika ada enemy movement
            originalVelocity = rb.velocity;

            // Apply knockback force
            Vector2 knockbackForceVector =
                knockbackDirection * knockbackForce * knockbackResistance;
            rb.AddForce(knockbackForceVector, ForceMode2D.Impulse);

            // Start knockback coroutine
            StartCoroutine(KnockbackCoroutine());
        }
    }

    private IEnumerator KnockbackCoroutine()
    {
        isKnockedBack = true;

        // Wait untuk knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Reset velocity (atau kembali ke movement normal)
        if (rb != null && !isDead)
        {
            rb.velocity = Vector2.zero; // Atau kembali ke originalVelocity jika ada AI movement
        }

        isKnockedBack = false;
    }

    private IEnumerator DamageFlash()
    {
        // Flash effect dengan mengubah warna sprite
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

        Debug.Log($"{gameObject.name} died!");

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("isDead", true);
        }

        // Disable collider agar tidak bisa damage player lagi
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Atau disable rigidbody
        }

        // Destroy object setelah delay
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);

        // Fade out effect (optional)
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

        Destroy(gameObject);
    }

    // Public methods untuk AI atau sistem lain
    public bool IsDead()
    {
        return isDead;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Method untuk heal enemy jika diperlukan
    public void Heal(int healAmount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log(
            $"{gameObject.name} healed for {healAmount}. Health: {currentHealth}/{maxHealth}"
        );
    }

    // Gizmos untuk debugging
    private void OnDrawGizmosSelected()
    {
        // Draw health bar di atas enemy
        if (Application.isPlaying)
        {
            Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
            float healthPercentage = GetHealthPercentage();

            // Background
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                healthBarPos - Vector3.right * 0.5f,
                healthBarPos + Vector3.right * 0.5f
            );

            // Health bar
            Gizmos.color = Color.green;
            Vector3 healthBarEnd =
                healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercentage;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarEnd);
        }
    }
}
