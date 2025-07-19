// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Enemy : MonoBehaviour, IDamageable
// {
//     [Header("Enemy Stats")]
//     public int maxHealth = 100;
//     public int currentHealth;
//     public int damage = 20;
//     public int pointsReward = 10; // TAMBAHAN: Point yang diberikan ketika musuh mati

//     [Header("Death Settings")]
//     public float deathDelay = 1f; // Delay sebelum destroy object

//     // Components
//     private Rigidbody2D rb;
//     private Animator animator;
//     private SpriteRenderer spriteRenderer;

//     // State
//     private bool isDead = false;

//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         spriteRenderer = GetComponent<SpriteRenderer>();

//         currentHealth = maxHealth;
//     }

//     private void OnTriggerEnter2D(Collider2D collision)
//     {
//         // Jangan damage player jika enemy sudah mati
//         if (isDead)
//             return;

//         PlayerController player = collision.GetComponent<PlayerController>();
//         if (player != null)
//         {
//             // PERBAIKAN: Hapus knockback direction, hanya kirim damage
//             player.TakeDamage(damage);
//         }
//     }

//     // Implementasi IDamageable interface
//     public void TakeDamage(int damageAmount, Vector2 knockbackDirection)
//     {
//         // Jangan terima damage jika sudah mati
//         if (isDead)
//             return;

//         // Kurangi health
//         currentHealth -= damageAmount;

//         // Debug log untuk testing
//         // Debug.Log(
//         //     $"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}"
//         // );

//         // Visual feedback
//         StartCoroutine(DamageFlash());

//         // Cek apakah enemy mati
//         if (currentHealth <= 0)
//         {
//             Die();
//         }
//     }

//     private IEnumerator DamageFlash()
//     {
//         // Flash effect dengan mengubah warna sprite
//         if (spriteRenderer != null)
//         {
//             Color originalColor = spriteRenderer.color;
//             spriteRenderer.color = Color.red;

//             yield return new WaitForSeconds(0.1f);

//             spriteRenderer.color = originalColor;
//         }
//     }

//     private void Die()
//     {
//         if (isDead)
//             return;

//         isDead = true;

//         // Debug.Log($"{gameObject.name} died!");

//         // TAMBAHAN: Berikan point ke player ketika musuh mati
//         GivePointsToPlayer();

//         // Trigger death animation
//         if (animator != null)
//         {
//             animator.SetTrigger("Die");
//             animator.SetBool("isDead", true);
//         }

//         // Disable collider agar tidak bisa damage player lagi
//         Collider2D col = GetComponent<Collider2D>();
//         if (col != null)
//         {
//             col.enabled = false;
//         }

//         // Stop movement
//         if (rb != null)
//         {
//             rb.velocity = Vector2.zero;
//             rb.isKinematic = true; // Atau disable rigidbody
//         }

//         // Destroy object setelah delay
//         StartCoroutine(DestroyAfterDelay());
//     }

//     // TAMBAHAN: Method untuk memberikan point ke player
//     private void GivePointsToPlayer()
//     {
//         // Cari player di scene
//         PlayerController player = FindObjectOfType<PlayerController>();

//         if (player != null)
//         {
//             // player.AddPoints(pointsReward);
//             // Debug.Log($"{gameObject.name} memberikan {pointsReward} points kepada player!");
//         }
//         else
//         {
//             // Debug.LogWarning("Player tidak ditemukan! Point tidak dapat diberikan.");
//         }
//     }

//     // TAMBAHAN: Method alternatif untuk memberikan point langsung ke player tertentu
//     public void GivePointsToPlayer(PlayerController targetPlayer)
//     {
//         if (targetPlayer != null)
//         {
//             targetPlayer.AddPoints(pointsReward);
//             // Debug.Log(
//             //     $"{gameObject.name} memberikan {pointsReward} points kepada {targetPlayer.name}!"
//             // );
//         }
//     }

//     private IEnumerator DestroyAfterDelay()
//     {
//         yield return new WaitForSeconds(deathDelay);

//         // Fade out effect (optional)
//         if (spriteRenderer != null)
//         {
//             float fadeTime = 0.5f;
//             float elapsedTime = 0f;
//             Color startColor = spriteRenderer.color;

//             while (elapsedTime < fadeTime)
//             {
//                 elapsedTime += Time.deltaTime;
//                 float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
//                 spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
//                 yield return null;
//             }
//         }

//         Destroy(gameObject);
//     }

//     // Public methods untuk AI atau sistem lain
//     public bool IsDead()
//     {
//         return isDead;
//     }

//     public float GetHealthPercentage()
//     {
//         return (float)currentHealth / maxHealth;
//     }

//     // TAMBAHAN: Method untuk mengakses points reward
//     public int GetPointsReward()
//     {
//         return pointsReward;
//     }

//     // TAMBAHAN: Method untuk mengubah points reward
//     public void SetPointsReward(int newPointsReward)
//     {
//         pointsReward = Mathf.Max(0, newPointsReward);
//     }

//     // Method untuk heal enemy jika diperlukan
//     public void Heal(int healAmount)
//     {
//         if (isDead)
//             return;

//         currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
//         // Debug.Log(
//         //     $"{gameObject.name} healed for {healAmount}. Health: {currentHealth}/{maxHealth}"
//         // );
//     }

//     // Gizmos untuk debugging
//     private void OnDrawGizmosSelected()
//     {
//         // Draw health bar di atas enemy
//         if (Application.isPlaying)
//         {
//             Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
//             float healthPercentage = GetHealthPercentage();

//             // Background
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(
//                 healthBarPos - Vector3.right * 0.5f,
//                 healthBarPos + Vector3.right * 0.5f
//             );

//             // Health bar
//             Gizmos.color = Color.green;
//             Vector3 healthBarEnd =
//                 healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercentage;
//             Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarEnd);

//             // TAMBAHAN: Tampilkan point reward di gizmos
//             Vector3 pointTextPos = transform.position + Vector3.up * 2f;
//             UnityEditor.Handles.Label(pointTextPos, $"Points: {pointsReward}");
//         }
//     }
// }
