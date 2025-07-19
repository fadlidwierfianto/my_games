using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 3f;
    private PlayerControl playerControl;
    private Vector2 movement;
    private Vector3 PlayerMoveDirection;
    private Rigidbody2D rb;

    [Header("Health System")]
    public int maxHealth = 200;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Point System")]
    public int currentPoints = 0;
    public TextMeshProUGUI pointText;

    [Header("Attack Settings")]
    [SerializeField]
    private float attackDuration = 0.5f;

    [SerializeField]
    private float attackCooldown = 0.5f;

    [SerializeField]
    private int attackDamage = 25;

    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("Grid Attack Settings")]
    [SerializeField]
    private float gridCellSize = 1f;

    [SerializeField]
    private bool useGridAttack = true;

    [Header("Legacy Attack Area Settings (Fallback)")]
    [SerializeField]
    private Vector2 attackAreaSize = new Vector2(1f, 1f);

    [SerializeField]
    private Vector2 attackOffset = new Vector2(0f, 0.5f);

    [Header("Damage Feedback")]
    [SerializeField]
    private float damageFlashDuration = 0.2f;

    [SerializeField]
    private Color damageFlashColor = Color.red;

    [SerializeField]
    private float damageShakeIntensity = 0.1f;

    [SerializeField]
    private float damageShakeDuration = 0.2f;

    [SerializeField]
    private int damageFlashCount = 3;

    [Header("Health Bar Integration")]
    public HealthBar healthBar;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    // Movement variables
    private Vector2 lastMoveDirection = Vector2.down;

    // Attack State
    private bool isAttacking = false;
    private bool canAttack = true;
    private Vector2 attackDirection;

    // Grid System
    private Vector2[] activeGridCells;
    private Vector2 gridCenter;
    private Vector2 currentAttackPosition;

    // Damage and health states
    private bool isDamaged = false;
    private bool isDead = false;
    private Color originalColor;

    // Components
    private Animator anim;
    public SpriteRenderer sprite;

    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentPoints => currentPoints;
    public Vector2 moveDir => movement;
    public bool IsDead => isDead;

    // Upgrade-related properties
    public float MoveSpeed => moveSpeed;
    public float AttackDuration => attackDuration;
    public float AttackCooldownValue => attackCooldown;
    public int AttackDamage => attackDamage;
    public float GridCellSize => gridCellSize;
    public Vector2 AttackAreaSize => attackAreaSize;

    private void Awake()
    {
        // Initialize components
        playerControl = new PlayerControl();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        // Subscribe to attack input
        playerControl.Combat.Attack.performed += _ => TryAttack();

        // Initialize stats
        currentHealth = maxHealth;
        currentPoints = 0;
        UpdateHealthUI();
        UpdatePointUI();

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.InitializeHealthBar(maxHealth);
        }

        // Store original sprite color
        originalColor = sprite.color;

        // Set initial animation direction
        UpdateAnimatorParameters();

        // Find GameOverManager if not assigned
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }
    }

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }

    private void Update()
    {
        // AddPoints(100);
        if (isDead)
            return;

        PlayerInput();

        // Update attack direction and grid system
        attackDirection = lastMoveDirection;

        if (useGridAttack)
        {
            UpdateGridSystem();
        }
        else
        {
            CalculateAttackPosition();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking)
            return;

        Move();
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();
        PlayerMoveDirection = new Vector3(movement.x, movement.y).normalized;

        UpdateMovementDirection();
        UpdateAnimatorParameters();
    }

    private void UpdateMovementDirection()
    {
        if (movement != Vector2.zero && !isAttacking)
        {
            lastMoveDirection = movement.normalized;
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (isDead || isAttacking)
            return;

        bool isMoving = movement != Vector2.zero;
        anim.SetBool("moving", isMoving);

        if (isMoving)
        {
            anim.SetFloat("moveX", movement.x);
            anim.SetFloat("moveY", movement.y);
        }
        else
        {
            anim.SetFloat("moveX", lastMoveDirection.x);
            anim.SetFloat("moveY", lastMoveDirection.y);
        }
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    #region Upgrade System Methods

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0.1f, newSpeed); // Minimum speed to prevent stopping
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.Max(1, newMaxHealth); // Minimum 1 health

        // Adjust current health proportionally
        float healthRatio = (float)currentHealth / oldMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.InitializeHealthBar(maxHealth);
            healthBar.UpdateHealthBar(currentHealth);
        }

        UpdateHealthUI();
    }

    public void SetAttackDamage(int newDamage)
    {
        attackDamage = Mathf.Max(1, newDamage); // Minimum 1 damage
    }

    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = Mathf.Max(0.1f, newCooldown); // Minimum cooldown to prevent spam
    }

    public void SetAttackDuration(float newDuration)
    {
        attackDuration = Mathf.Max(0.1f, newDuration); // Minimum duration
    }

    public void SetAttackRange(float newRange)
    {
        gridCellSize = Mathf.Max(0.5f, newRange); // Minimum range

        // Also update legacy attack area if needed
        attackAreaSize = new Vector2(gridCellSize, gridCellSize);
    }

    // Method to upgrade all stats at once (useful for loading saved upgrades)
    public void SetAllStats(
        float newMoveSpeed,
        int newMaxHealth,
        int newAttackDamage,
        float newAttackCooldown,
        float newAttackDuration,
        float newAttackRange
    )
    {
        SetMoveSpeed(newMoveSpeed);
        SetMaxHealth(newMaxHealth);
        SetAttackDamage(newAttackDamage);
        SetAttackCooldown(newAttackCooldown);
        SetAttackDuration(newAttackDuration);
        SetAttackRange(newAttackRange);
    }

    #endregion

    #region Attack System

    private void UpdateGridSystem()
    {
        gridCenter = transform.position;
        activeGridCells = GetActiveGridCells(attackDirection);
    }

    private Vector2[] GetActiveGridCells(Vector2 direction)
    {
        List<Vector2> cells = new List<Vector2>();
        Vector2 normalizedDir = GetNormalizedDirection(direction);

        if (normalizedDir.y > 0.5f) // Up - Grid 1, 2, 3
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, gridCellSize));
            cells.Add(gridCenter + new Vector2(0, gridCellSize));
            cells.Add(gridCenter + new Vector2(gridCellSize, gridCellSize));
        }
        else if (normalizedDir.y < -0.5f) // Down - Grid 7, 8, 9
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, -gridCellSize));
            cells.Add(gridCenter + new Vector2(0, -gridCellSize));
            cells.Add(gridCenter + new Vector2(gridCellSize, -gridCellSize));
        }
        else if (normalizedDir.x < -0.5f) // Left - Grid 1, 4, 7
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, gridCellSize));
            cells.Add(gridCenter + new Vector2(-gridCellSize, 0));
            cells.Add(gridCenter + new Vector2(-gridCellSize, -gridCellSize));
        }
        else if (normalizedDir.x > 0.5f) // Right - Grid 3, 6, 9
        {
            cells.Add(gridCenter + new Vector2(gridCellSize, gridCellSize));
            cells.Add(gridCenter + new Vector2(gridCellSize, 0));
            cells.Add(gridCenter + new Vector2(gridCellSize, -gridCellSize));
        }

        return cells.ToArray();
    }

    private Vector2 GetNormalizedDirection(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absY > absX)
        {
            return new Vector2(0, direction.y > 0 ? 1 : -1);
        }
        else
        {
            return new Vector2(direction.x > 0 ? 1 : -1, 0);
        }
    }

    private void TryAttack()
    {
        if (!canAttack || isAttacking || isDead)
            return;

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        canAttack = false;

        rb.velocity = Vector2.zero;

        anim.SetFloat("moveX", attackDirection.x);
        anim.SetFloat("moveY", attackDirection.y);
        anim.SetTrigger("Attack");
        anim.SetBool("isAttacking", true);

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(attackDuration * 0.5f);

        if (useGridAttack)
        {
            PerformGridAttack();
        }
        else
        {
            PerformLegacyAttack();
        }

        yield return new WaitForSeconds(attackDuration * 0.5f);

        EndAttack();
    }

    private void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", false);
        StartCoroutine(AttackCooldownCoroutine());
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void PerformGridAttack()
    {
        List<Collider2D> allHitEnemies = new List<Collider2D>();

        foreach (Vector2 cellPosition in activeGridCells)
        {
            Collider2D[] cellColliders = Physics2D.OverlapBoxAll(
                cellPosition,
                new Vector2(gridCellSize, gridCellSize),
                0f
            );

            foreach (Collider2D collider in cellColliders)
            {
                if (collider.CompareTag(enemyTag) && !allHitEnemies.Contains(collider))
                {
                    allHitEnemies.Add(collider);
                }
            }
        }

        foreach (Collider2D enemy in allHitEnemies)
        {
            ProcessEnemyHit(enemy);
        }
    }

    private void PerformLegacyAttack()
    {
        Vector2 attackPos = (Vector2)transform.position + GetAttackOffset();
        Collider2D[] allColliders = Physics2D.OverlapBoxAll(attackPos, attackAreaSize, 0f);

        List<Collider2D> hitEnemies = new List<Collider2D>();
        foreach (Collider2D collider in allColliders)
        {
            if (collider.CompareTag(enemyTag))
            {
                hitEnemies.Add(collider);
            }
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            ProcessEnemyHit(enemy);
        }
    }

    private void ProcessEnemyHit(Collider2D enemy)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
            damageable.TakeDamage(attackDamage, knockbackDir);
            return;
        }
    }

    private Vector2 GetAttackOffset()
    {
        Vector2 offset = attackOffset;

        if (attackDirection.y > 0.5f)
        {
            offset = new Vector2(0f, attackOffset.y);
        }
        else if (attackDirection.y < -0.5f)
        {
            offset = new Vector2(0f, -attackOffset.y);
        }
        else if (attackDirection.x > 0.5f)
        {
            offset = new Vector2(attackOffset.y, 0f);
        }
        else if (attackDirection.x < -0.5f)
        {
            offset = new Vector2(-attackOffset.y, 0f);
        }

        return offset;
    }

    private void CalculateAttackPosition()
    {
        currentAttackPosition = (Vector2)transform.position + GetAttackOffset();
    }

    #endregion

    #region Health and Damage System

    public void TakeDamage(int damage)
    {
        if (isDamaged || isDead)
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth);
        }

        if (!isDead)
        {
            StartCoroutine(DamageFlashEffect());
            StartCoroutine(DamageShakeEffect());
        }

        UpdateHealthUI();
    }

    public void Heal(int healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth);
        }

        UpdateHealthUI();
    }

    public void SetHealth(int newHealth)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth);
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    private IEnumerator DamageFlashEffect()
    {
        isDamaged = true;

        for (int i = 0; i < damageFlashCount; i++)
        {
            sprite.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration / (damageFlashCount * 2));

            sprite.color = originalColor;
            yield return new WaitForSeconds(damageFlashDuration / (damageFlashCount * 2));
        }

        sprite.color = originalColor;
        isDamaged = false;
    }

    private IEnumerator DamageShakeEffect()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < damageShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * damageShakeIntensity;
            float y = Random.Range(-1f, 1f) * damageShakeIntensity;

            transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    #endregion

    #region Point System

    public void AddPoints(int points)
    {
        if (isDead)
            return;

        currentPoints += points;
        UpdatePointUI();
        Debug.Log($"Player gained {points} points! Total: {currentPoints}");
    }

    public void RemovePoints(int points)
    {
        if (isDead)
            return;

        currentPoints -= points;
        if (currentPoints < 0)
            currentPoints = 0;

        UpdatePointUI();
    }

    public void SetPoints(int points)
    {
        if (isDead)
            return;

        currentPoints = Mathf.Max(0, points);
        UpdatePointUI();
    }

    private void UpdatePointUI()
    {
        if (pointText != null)
            pointText.text = "Points: " + currentPoints;
    }

    #endregion

    #region Death and Revival System

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        rb.velocity = Vector2.zero;

        if (playerControl != null)
        {
            playerControl.Disable();
        }

        if (anim != null)
        {
            anim.SetBool("isDead", true);
            anim.SetBool("moving", false);
        }

        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager tidak ditemukan!");
        }

        Debug.Log("Player Mati - Game Over!");
    }

    public void Revive()
    {
        if (!isDead)
            return;

        isDead = false;
        currentHealth = maxHealth;

        if (playerControl != null)
        {
            playerControl.Enable();
        }

        if (anim != null)
        {
            anim.SetBool("isDead", false);
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth);
        }

        UpdateHealthUI();
    }

    #endregion

    #region Public Getters and Utility Methods

    public Vector2 GetLastFacingDirection()
    {
        return lastMoveDirection;
    }

    public bool IsMoving()
    {
        return movement != Vector2.zero && !isDead;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public bool IsInAction()
    {
        return isDamaged || isAttacking || isDead;
    }

    public bool IsDamaged()
    {
        return isDamaged;
    }

    public Vector2 GetAttackDirection()
    {
        return attackDirection;
    }

    public Vector2[] GetActiveGridCells()
    {
        return activeGridCells;
    }

    public bool IsUsingGridAttack()
    {
        return useGridAttack;
    }

    public void DisableControls()
    {
        if (playerControl != null)
        {
            playerControl.Disable();
        }
    }

    public void EnableControls()
    {
        if (playerControl != null && !isDead)
        {
            playerControl.Enable();
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (playerControl != null)
        {
            playerControl.Disable();
        }
    }
}
