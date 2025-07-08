// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerAttack : MonoBehaviour
// {
//     [Header("Attack Settings")]
//     [SerializeField]
//     private float attackDuration = 0.5f;

//     [SerializeField]
//     private float attackCooldown = 0.3f;

//     [SerializeField]
//     private int attackDamage = 25;

//     [SerializeField]
//     private string enemyTag = "Enemy";

//     [Header("Attack Area Settings")]
//     [SerializeField]
//     private Vector2 attackAreaSize = new Vector2(1f, 1f);

//     [SerializeField]
//     private Vector2 attackOffset = new Vector2(0f, 0.5f);

//     // Components
//     private PlayerController playerController;
//     private PlayerControl playerControl;
//     private Animator animator;
//     private Rigidbody2D rb;

//     // Attack State
//     private bool isAttacking = false;
//     private bool canAttack = true;
//     private Vector2 attackDirection;

//     // Attack area calculation
//     private Vector2 currentAttackPosition;

//     private void Awake()
//     {
//         playerController = GetComponent<PlayerController>();
//         playerControl = new PlayerControl();
//         animator = GetComponent<Animator>();
//         rb = GetComponent<Rigidbody2D>();

//         // Subscribe to attack input
//         playerControl.Combat.Attack.performed += _ => TryAttack();
//     }

//     private void OnEnable()
//     {
//         playerControl.Enable();
//     }

//     private void OnDisable()
//     {
//         playerControl.Disable();
//     }

//     private void Update()
//     {
//         // Update attack direction berdasarkan facing direction dari PlayerController
//         attackDirection = playerController.GetLastFacingDirection();

//         // Calculate attack position for visualization
//         CalculateAttackPosition();
//     }

//     private void TryAttack()
//     {
//         // Cek apakah bisa attack
//         if (!canAttack || isAttacking)
//             return;

//         StartAttack();
//     }

//     private void StartAttack()
//     {
//         isAttacking = true;
//         canAttack = false;

//         // Stop player movement saat attack
//         rb.velocity = Vector2.zero;

//         // Set animator parameters untuk attack direction
//         animator.SetFloat("moveX", attackDirection.x);
//         animator.SetFloat("moveY", attackDirection.y);

//         // Trigger attack animation
//         animator.SetTrigger("Attack");
//         animator.SetBool("isAttacking", true);

//         // Start attack coroutine
//         StartCoroutine(AttackCoroutine());
//     }

//     private IEnumerator AttackCoroutine()
//     {
//         // Wait for attack point (biasanya di tengah animasi)
//         yield return new WaitForSeconds(attackDuration * 0.5f);

//         // Perform attack hit detection
//         PerformAttackHit();

//         // Wait for attack to finish
//         yield return new WaitForSeconds(attackDuration * 0.5f);

//         // End attack
//         EndAttack();
//     }

//     private void EndAttack()
//     {
//         isAttacking = false;
//         animator.SetBool("isAttacking", false);

//         // Start cooldown
//         StartCoroutine(AttackCooldown());
//     }

//     private IEnumerator AttackCooldown()
//     {
//         yield return new WaitForSeconds(attackCooldown);
//         canAttack = true;
//     }

//     private void PerformAttackHit()
//     {
//         // Calculate attack area berdasarkan direction
//         Vector2 attackPos = (Vector2)transform.position + GetAttackOffset();

//         // Detect ALL colliders in attack area
//         Collider2D[] allColliders = Physics2D.OverlapBoxAll(attackPos, attackAreaSize, 0f);

//         // Filter hanya yang memiliki tag "Enemy"
//         List<Collider2D> hitEnemies = new List<Collider2D>();

//         foreach (Collider2D collider in allColliders)
//         {
//             if (collider.CompareTag(enemyTag))
//             {
//                 hitEnemies.Add(collider);
//             }
//         }

//         // Process each enemy hit
//         foreach (Collider2D enemy in hitEnemies)
//         {
//             // METHOD 1: Cek apakah enemy memiliki component yang implements IDamageable
//             IDamageable damageable = enemy.GetComponent<IDamageable>();
//             if (damageable != null)
//             {
//                 Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
//                 damageable.TakeDamage(attackDamage, knockbackDir);
//                 continue; // Skip ke enemy berikutnya
//             }

//             // METHOD 2: Cek apakah enemy memiliki component Enemy dengan method TakeDamage
//             Enemy enemyScript = enemy.GetComponent<Enemy>();
//             if (enemyScript != null)
//             {
//                 Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
//                 enemyScript.TakeDamage(attackDamage, knockbackDir);
//                 continue;
//             }

//             // METHOD 3: Cek apakah enemy memiliki component EnemyHealth
//             // EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
//             // if (enemyHealth != null)
//             // {
//             //     Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
//             //     enemyHealth.TakeDamage(attackDamage, knockbackDir);
//             //     continue;
//             // }

//             // METHOD 4: Generic approach - coba panggil method TakeDamage via SendMessage
//             // (Tidak disarankan untuk performance, tapi bisa sebagai fallback)
//             try
//             {
//                 Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
//                 enemy.SendMessage(
//                     "TakeDamage",
//                     new object[] { attackDamage, knockbackDir },
//                     SendMessageOptions.DontRequireReceiver
//                 );
//             }
//             catch
//             {
//                 Debug.LogWarning(
//                     $"Enemy {enemy.name} tidak memiliki method TakeDamage yang compatible!"
//                 );
//             }
//         }

//         // Debug info
//         Debug.Log($"Attack performed! Hit {hitEnemies.Count} enemies");

//         // Optional: Visual feedback
//         if (hitEnemies.Count > 0)
//         {
//             // Bisa tambahkan screen shake, particle effect, etc.
//             Debug.Log("Hit confirmed!");
//         }
//     }

//     private Vector2 GetAttackOffset()
//     {
//         // Calculate offset berdasarkan attack direction
//         Vector2 offset = attackOffset;

//         // Adjust offset based on direction
//         if (attackDirection.y > 0.5f) // Up
//         {
//             offset = new Vector2(0f, attackOffset.y);
//         }
//         else if (attackDirection.y < -0.5f) // Down
//         {
//             offset = new Vector2(0f, -attackOffset.y);
//         }
//         else if (attackDirection.x > 0.5f) // Right
//         {
//             offset = new Vector2(attackOffset.y, 0f);
//         }
//         else if (attackDirection.x < -0.5f) // Left
//         {
//             offset = new Vector2(-attackOffset.y, 0f);
//         }

//         return offset;
//     }

//     private void CalculateAttackPosition()
//     {
//         currentAttackPosition = (Vector2)transform.position + GetAttackOffset();
//     }

//     // Public getters
//     public bool IsAttacking()
//     {
//         return isAttacking;
//     }

//     public bool CanAttack()
//     {
//         return canAttack;
//     }

//     public Vector2 GetAttackDirection()
//     {
//         return attackDirection;
//     }

//     // Gizmos untuk visualisasi attack area di Scene view
//     private void OnDrawGizmosSelected()
//     {
//         if (Application.isPlaying)
//         {
//             // Draw attack area
//             Gizmos.color = isAttacking ? Color.red : Color.yellow;
//             Gizmos.DrawWireCube(currentAttackPosition, attackAreaSize);

//             // Draw attack direction
//             Gizmos.color = Color.blue;
//             Gizmos.DrawLine(transform.position, transform.position + (Vector3)attackDirection);
//         }
//         else
//         {
//             // Preview attack area in editor
//             Gizmos.color = Color.gray;
//             Vector2 previewOffset = new Vector2(0f, attackOffset.y); // Default up direction
//             Gizmos.DrawWireCube((Vector2)transform.position + previewOffset, attackAreaSize);
//         }
//     }
// }

// // Interface untuk damage system (optional, tapi recommended)
// public interface IDamageable
// {
//     void TakeDamage(int damage, Vector2 knockbackDirection);
// }
