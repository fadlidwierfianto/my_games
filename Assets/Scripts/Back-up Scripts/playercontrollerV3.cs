// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class PlayerController : MonoBehaviour
// {
//     [SerializeField]
//     private float moveSpeed = 1f;
//     private PlayerControl playerControl;
//     private Vector2 movement;
//     private Vector3 PlayerMoveDirection;
//     private Rigidbody2D rb;

//     [Header("Health System")]
//     public int maxHealth = 100;
//     private int currentHealth;
//     public TextMeshProUGUI healthText;

//     [Header("Point System")]
//     public int currentPoints = 0;
//     public TextMeshProUGUI pointText;

//     [Header("Damage Feedback")]
//     [SerializeField]
//     private float damageFlashDuration = 0.2f;

//     [SerializeField]
//     private Color damageFlashColor = Color.red;

//     [SerializeField]
//     private float damageShakeIntensity = 0.1f;

//     [SerializeField]
//     private float damageShakeDuration = 0.2f;

//     [SerializeField]
//     private int damageFlashCount = 3;

//     private bool isDamaged = false;
//     private Color originalColor;

//     private Animator anim;
//     public SpriteRenderer sprite;

//     // TAMBAHAN: Menyimpan arah terakhir untuk idle animation
//     private Vector2 lastMoveDirection = Vector2.down; // Default menghadap bawah

//     // TAMBAHAN: Reference ke PlayerAttack component
//     private PlayerAttack playerAttack;

//     [Header("Health Bar Integration")]
//     public HealthBar healthBar;

//     // TAMBAHAN: Game Over Integration
//     [Header("Game Over")]
//     public GameOverManager gameOverManager;
//     private bool isDead = false;

//     // Property untuk mengakses currentHealth
//     public int CurrentHealth
//     {
//         get { return currentHealth; }
//     }

//     // Property untuk mengakses maxHealth (sudah public, tapi untuk konsistensi)
//     public int MaxHealth
//     {
//         get { return maxHealth; }
//     }

//     // TAMBAHAN: Property untuk mengakses point system
//     public int CurrentPoints
//     {
//         get { return currentPoints; }
//     }

//     public Vector2 moveDir
//     {
//         get { return movement; }
//     }

//     // TAMBAHAN: Property untuk mengecek apakah player mati
//     public bool IsDead
//     {
//         get { return isDead; }
//     }

//     private void Awake()
//     {
//         playerControl = new PlayerControl();
//         rb = GetComponent<Rigidbody2D>();
//         anim = GetComponent<Animator>();
//         sprite = GetComponent<SpriteRenderer>();
//         playerAttack = GetComponent<PlayerAttack>(); // Get PlayerAttack component

//         currentHealth = maxHealth;
//         currentPoints = 0; // Initialize points
//         UpdateHealthUI();
//         UpdatePointUI(); // Initialize point UI

//         // Initialize health bar
//         if (healthBar != null)
//         {
//             healthBar.InitializeHealthBar(maxHealth);
//         }

//         // Store original sprite color
//         originalColor = sprite.color;

//         // TAMBAHAN: Set initial animation direction
//         UpdateAnimatorParameters();

//         // TAMBAHAN: Find GameOverManager if not assigned
//         if (gameOverManager == null)
//         {
//             gameOverManager = FindObjectOfType<GameOverManager>();
//         }
//     }

//     private void OnEnable()
//     {
//         playerControl.Enable();
//     }

//     private void Update()
//     {
//         // TAMBAHAN: Jangan terima input jika player sudah mati
//         if (isDead)
//             return;

//         PlayerInput();
//     }

//     private void FixedUpdate()
//     {
//         // TAMBAHAN: Jangan bergerak jika player sudah mati
//         if (isDead)
//             return;

//         // MODIFIKASI: Cek apakah sedang attack, jika ya jangan move
//         if (playerAttack != null && playerAttack.IsAttacking())
//             return;

//         Move();
//     }

//     private void PlayerInput()
//     {
//         movement = playerControl.Movement.Move.ReadValue<Vector2>();
//         PlayerMoveDirection = new Vector3(movement.x, movement.y).normalized;

//         // PERBAIKAN: Update last direction dan animator parameters
//         UpdateMovementDirection();
//         UpdateAnimatorParameters();
//     }

//     // TAMBAHAN: Method untuk update movement direction
//     private void UpdateMovementDirection()
//     {
//         // Simpan arah terakhir hanya jika ada input movement dan tidak sedang attack
//         if (movement != Vector2.zero && (playerAttack == null || !playerAttack.IsAttacking()))
//         {
//             lastMoveDirection = movement.normalized;
//         }
//     }

//     // PERBAIKAN: Method terpisah untuk update animator parameters
//     private void UpdateAnimatorParameters()
//     {
//         // MODIFIKASI: Jangan update animator jika sedang attack atau sudah mati
//         if (isDead || (playerAttack != null && playerAttack.IsAttacking()))
//             return;

//         bool isMoving = movement != Vector2.zero;
//         anim.SetBool("moving", isMoving);

//         if (isMoving)
//         {
//             // Jika bergerak, gunakan current movement
//             anim.SetFloat("moveX", movement.x);
//             anim.SetFloat("moveY", movement.y);
//         }
//         else
//         {
//             // Jika idle, gunakan last direction untuk menentukan arah idle
//             anim.SetFloat("moveX", lastMoveDirection.x);
//             anim.SetFloat("moveY", lastMoveDirection.y);
//         }
//     }

//     private void Move()
//     {
//         rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
//     }

//     public void TakeDamage(int damage)
//     {
//         if (isDamaged || isDead)
//             return; // Prevent damage stacking during damage feedback atau jika sudah mati

//         currentHealth -= damage;
//         if (currentHealth <= 0)
//         {
//             currentHealth = 0;
//             Die(); // TAMBAHAN: Panggil method Die()
//         }

//         // Update health bar jika ada
//         if (healthBar != null)
//         {
//             healthBar.UpdateHealthBar(currentHealth);
//         }

//         // Start damage feedback effects hanya jika belum mati
//         if (!isDead)
//         {
//             StartCoroutine(DamageFlashEffect());
//             StartCoroutine(DamageShakeEffect());
//         }

//         UpdateHealthUI();
//     }

//     // TAMBAHAN: Method untuk menambah point dari musuh yang mati
//     public void AddPoints(int points)
//     {
//         if (isDead)
//             return; // Tidak bisa dapat point jika sudah mati

//         currentPoints += points;
//         UpdatePointUI();

//         // Debug log untuk testing
//         Debug.Log($"Player gained {points} points! Total: {currentPoints}");
//     }

//     // TAMBAHAN: Method untuk mengurangi point (jika diperlukan)
//     public void RemovePoints(int points)
//     {
//         if (isDead)
//             return;

//         currentPoints -= points;
//         if (currentPoints < 0)
//             currentPoints = 0;

//         UpdatePointUI();
//     }

//     // TAMBAHAN: Method untuk set point langsung
//     public void SetPoints(int points)
//     {
//         if (isDead)
//             return;

//         currentPoints = Mathf.Max(0, points);
//         UpdatePointUI();
//     }

//     // TAMBAHAN: Method untuk update point UI
//     private void UpdatePointUI()
//     {
//         if (pointText != null)
//             pointText.text = "Points: " + currentPoints;
//     }

//     // TAMBAHAN: Method untuk menangani kematian player
//     private void Die()
//     {
//         if (isDead)
//             return; // Prevent multiple death calls

//         isDead = true;

//         // Stop all movement
//         rb.velocity = Vector2.zero;

//         // Disable player controls
//         if (playerControl != null)
//         {
//             playerControl.Disable();
//         }

//         // Set death animation jika ada
//         if (anim != null)
//         {
//             anim.SetBool("isDead", true);
//             anim.SetBool("moving", false);
//         }

//         // Trigger game over
//         if (gameOverManager != null)
//         {
//             gameOverManager.TriggerGameOver();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "GameOverManager tidak ditemukan! Pastikan ada GameOverManager di scene."
//             );
//         }

//         Debug.Log("Player Mati - Game Over!");
//     }

//     // TAMBAHAN: Method untuk revive player (jika diperlukan untuk cheat atau power-up)
//     public void Revive()
//     {
//         if (!isDead)
//             return;

//         isDead = false;
//         currentHealth = maxHealth;

//         // Enable player controls
//         if (playerControl != null)
//         {
//             playerControl.Enable();
//         }

//         // Reset animations
//         if (anim != null)
//         {
//             anim.SetBool("isDead", false);
//         }

//         // Reset health bar
//         if (healthBar != null)
//         {
//             healthBar.UpdateHealthBar(currentHealth);
//         }

//         UpdateHealthUI();
//     }

//     // TAMBAHAN: Method untuk healing
//     public void Heal(int healAmount)
//     {
//         if (isDead)
//             return; // Tidak bisa heal jika sudah mati

//         currentHealth += healAmount;
//         if (currentHealth > maxHealth)
//             currentHealth = maxHealth;

//         // Update health bar jika ada
//         if (healthBar != null)
//         {
//             healthBar.UpdateHealthBar(currentHealth);
//         }

//         UpdateHealthUI();
//     }

//     // TAMBAHAN: Method untuk set health langsung
//     public void SetHealth(int newHealth)
//     {
//         if (isDead)
//             return; // Tidak bisa set health jika sudah mati

//         currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

//         // Check if this causes death
//         if (currentHealth <= 0)
//         {
//             Die();
//         }

//         // Update health bar jika ada
//         if (healthBar != null)
//         {
//             healthBar.UpdateHealthBar(currentHealth);
//         }

//         UpdateHealthUI();
//     }

//     private void UpdateHealthUI()
//     {
//         if (healthText != null)
//             healthText.text = "Health: " + currentHealth;
//     }

//     private IEnumerator DamageFlashEffect()
//     {
//         isDamaged = true;

//         for (int i = 0; i < damageFlashCount; i++)
//         {
//             // Flash to damage color
//             sprite.color = damageFlashColor;
//             yield return new WaitForSeconds(damageFlashDuration / (damageFlashCount * 2));

//             // Flash back to original color
//             sprite.color = originalColor;
//             yield return new WaitForSeconds(damageFlashDuration / (damageFlashCount * 2));
//         }

//         // Ensure sprite is back to original color
//         sprite.color = originalColor;
//         isDamaged = false;
//     }

//     private IEnumerator DamageShakeEffect()
//     {
//         Vector3 originalPosition = transform.position;
//         float elapsed = 0f;

//         while (elapsed < damageShakeDuration)
//         {
//             float x = Random.Range(-1f, 1f) * damageShakeIntensity;
//             float y = Random.Range(-1f, 1f) * damageShakeIntensity;

//             transform.position = originalPosition + new Vector3(x, y, 0);

//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         // Return to original position
//         transform.position = originalPosition;
//     }

//     // TAMBAHAN: Public methods untuk debugging atau akses dari script lain
//     public Vector2 GetLastFacingDirection()
//     {
//         return lastMoveDirection;
//     }

//     public bool IsMoving()
//     {
//         return movement != Vector2.zero && !isDead;
//     }

//     // TAMBAHAN: Method untuk mengecek apakah player sedang dalam aksi (attack atau damage feedback)
//     public bool IsInAction()
//     {
//         return isDamaged || (playerAttack != null && playerAttack.IsAttacking()) || isDead;
//     }

//     // Method untuk mengecek apakah sedang dalam damage state
//     public bool IsDamaged()
//     {
//         return isDamaged;
//     }

//     // TAMBAHAN: Method untuk disable/enable player controls dari luar
//     public void DisableControls()
//     {
//         if (playerControl != null)
//         {
//             playerControl.Disable();
//         }
//     }

//     public void EnableControls()
//     {
//         if (playerControl != null && !isDead)
//         {
//             playerControl.Enable();
//         }
//     }

//     // TAMBAHAN: Cleanup saat object dihancurkan
//     private void OnDestroy()
//     {
//         if (playerControl != null)
//         {
//             playerControl.Disable();
//         }
//     }
// }
