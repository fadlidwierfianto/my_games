using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField]
    private float attackDuration = 0.5f;

    [SerializeField]
    private float attackCooldown = 0.3f;

    [SerializeField]
    private int attackDamage = 25;

    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("Grid Attack Settings")]
    [SerializeField]
    private float gridCellSize = 1f;

    [SerializeField]
    private bool useGridAttack = true;

    [SerializeField]
    private bool showGridGizmos = true;

    [Header("Legacy Attack Area Settings (Fallback)")]
    [SerializeField]
    private Vector2 attackAreaSize = new Vector2(1f, 1f);

    [SerializeField]
    private Vector2 attackOffset = new Vector2(0f, 0.5f);

    // Components
    private PlayerController playerController;
    private PlayerControl playerControl;
    private Animator animator;
    private Rigidbody2D rb;

    // Attack State
    private bool isAttacking = false;
    private bool canAttack = true;
    private Vector2 attackDirection;

    // Grid System
    private Vector2[] activeGridCells;
    private Vector2 gridCenter;

    // Legacy attack area calculation
    private Vector2 currentAttackPosition;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerControl = new PlayerControl();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Subscribe to attack input
        playerControl.Combat.Attack.performed += _ => TryAttack();
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
        // Update attack direction berdasarkan facing direction dari PlayerController
        attackDirection = playerController.GetLastFacingDirection();

        // Update grid system
        if (useGridAttack)
        {
            UpdateGridSystem();
        }
        else
        {
            // Legacy system - Calculate attack position for visualization
            CalculateAttackPosition();
        }
    }

    private void UpdateGridSystem()
    {
        // Set grid center to player position
        gridCenter = transform.position;

        // Get active grid cells based on direction
        activeGridCells = GetActiveGridCells(attackDirection);
    }

    private Vector2[] GetActiveGridCells(Vector2 direction)
    {
        List<Vector2> cells = new List<Vector2>();

        // Normalize direction untuk memastikan hanya 4 arah
        Vector2 normalizedDir = GetNormalizedDirection(direction);

        // Grid positions relative to center (player position)
        // Grid layout:
        // [0,1] [1,1] [2,1]  -> Grid 1, 2, 3
        // [0,0] [1,0] [2,0]  -> Grid 4, P, 6
        // [0,-1][1,-1][2,-1] -> Grid 7, 8, 9

        if (normalizedDir.y > 0.5f) // Atas - Grid 1, 2, 3
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, gridCellSize)); // Grid 1
            cells.Add(gridCenter + new Vector2(0, gridCellSize)); // Grid 2
            cells.Add(gridCenter + new Vector2(gridCellSize, gridCellSize)); // Grid 3
        }
        else if (normalizedDir.y < -0.5f) // Bawah - Grid 7, 8, 9
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, -gridCellSize)); // Grid 7
            cells.Add(gridCenter + new Vector2(0, -gridCellSize)); // Grid 8
            cells.Add(gridCenter + new Vector2(gridCellSize, -gridCellSize)); // Grid 9
        }
        else if (normalizedDir.x < -0.5f) // Kiri - Grid 1, 4, 7
        {
            cells.Add(gridCenter + new Vector2(-gridCellSize, gridCellSize)); // Grid 1
            cells.Add(gridCenter + new Vector2(-gridCellSize, 0)); // Grid 4
            cells.Add(gridCenter + new Vector2(-gridCellSize, -gridCellSize)); // Grid 7
        }
        else if (normalizedDir.x > 0.5f) // Kanan - Grid 3, 6, 9
        {
            cells.Add(gridCenter + new Vector2(gridCellSize, gridCellSize)); // Grid 3
            cells.Add(gridCenter + new Vector2(gridCellSize, 0)); // Grid 6
            cells.Add(gridCenter + new Vector2(gridCellSize, -gridCellSize)); // Grid 9
        }

        return cells.ToArray();
    }

    private Vector2 GetNormalizedDirection(Vector2 direction)
    {
        // Normalize ke 4 arah saja
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absY > absX)
        {
            // Vertical movement dominan
            return new Vector2(0, direction.y > 0 ? 1 : -1);
        }
        else
        {
            // Horizontal movement dominan
            return new Vector2(direction.x > 0 ? 1 : -1, 0);
        }
    }

    private void TryAttack()
    {
        // Cek apakah bisa attack
        if (!canAttack || isAttacking)
            return;

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        canAttack = false;

        // Stop player movement saat attack
        rb.velocity = Vector2.zero;

        // Set animator parameters untuk attack direction
        animator.SetFloat("moveX", attackDirection.x);
        animator.SetFloat("moveY", attackDirection.y);

        // Trigger attack animation
        animator.SetTrigger("Attack");
        animator.SetBool("isAttacking", true);

        // Start attack coroutine
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        // Wait for attack point (biasanya di tengah animasi)
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // Perform attack hit detection
        if (useGridAttack)
        {
            PerformGridAttack();
        }
        else
        {
            PerformLegacyAttack();
        }

        // Wait for attack to finish
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // End attack
        EndAttack();
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);

        // Start cooldown
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void PerformGridAttack()
    {
        List<Collider2D> allHitEnemies = new List<Collider2D>();

        // Check each active grid cell
        foreach (Vector2 cellPosition in activeGridCells)
        {
            // Detect colliders in this grid cell
            Collider2D[] cellColliders = Physics2D.OverlapBoxAll(
                cellPosition,
                new Vector2(gridCellSize, gridCellSize),
                0f
            );

            // Filter enemies in this cell
            foreach (Collider2D collider in cellColliders)
            {
                if (collider.CompareTag(enemyTag) && !allHitEnemies.Contains(collider))
                {
                    allHitEnemies.Add(collider);
                }
            }
        }

        // Process each enemy hit
        foreach (Collider2D enemy in allHitEnemies)
        {
            ProcessEnemyHit(enemy);
        }

        // Debug info
        Debug.Log(
            $"Grid Attack performed! Hit {allHitEnemies.Count} enemies across {activeGridCells.Length} grid cells"
        );

        // Optional: Visual feedback
        if (allHitEnemies.Count > 0)
        {
            Debug.Log("Grid Hit confirmed!");
        }
    }

    private void PerformLegacyAttack()
    {
        // Legacy attack system (fallback)
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

        Debug.Log($"Legacy Attack performed! Hit {hitEnemies.Count} enemies");
    }

    private void ProcessEnemyHit(Collider2D enemy)
    {
        // METHOD 1: Cek apakah enemy memiliki component yang implements IDamageable
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
            damageable.TakeDamage(attackDamage, knockbackDir);
            return;
        }

        // METHOD 2: Cek apakah enemy memiliki component Enemy dengan method TakeDamage
        // Enemy enemyScript = enemy.GetComponent<Enemy>();
        // if (enemyScript != null)
        // {
        //     Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
        //     enemyScript.TakeDamage(attackDamage, knockbackDir);
        //     return;
        // }

        // METHOD 4: Generic approach - coba panggil method TakeDamage via SendMessage
        // try
        // {
        //     Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
        //     enemy.SendMessage(
        //         "TakeDamage",
        //         new object[] { attackDamage, knockbackDir },
        //         SendMessageOptions.DontRequireReceiver
        //     );
        // }
        // catch
        // {
        //     Debug.LogWarning(
        //         $"Enemy {enemy.name} tidak memiliki method TakeDamage yang compatible!"
        //     );
        // }
    }

    private Vector2 GetAttackOffset()
    {
        // Legacy system - Calculate offset berdasarkan attack direction
        Vector2 offset = attackOffset;

        if (attackDirection.y > 0.5f) // Up
        {
            offset = new Vector2(0f, attackOffset.y);
        }
        else if (attackDirection.y < -0.5f) // Down
        {
            offset = new Vector2(0f, -attackOffset.y);
        }
        else if (attackDirection.x > 0.5f) // Right
        {
            offset = new Vector2(attackOffset.y, 0f);
        }
        else if (attackDirection.x < -0.5f) // Left
        {
            offset = new Vector2(-attackOffset.y, 0f);
        }

        return offset;
    }

    private void CalculateAttackPosition()
    {
        // Legacy system
        currentAttackPosition = (Vector2)transform.position + GetAttackOffset();
    }

    // Public getters
    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool CanAttack()
    {
        return canAttack;
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

    // Gizmos untuk visualisasi attack area di Scene view
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (useGridAttack && showGridGizmos)
            {
                // Draw grid system
                DrawGridGizmos();
            }
            else
            {
                // Draw legacy attack area
                DrawLegacyGizmos();
            }

            // Draw attack direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)attackDirection);
        }
        else
        {
            // Preview in editor
            if (useGridAttack && showGridGizmos)
            {
                // Preview grid system (default up direction)
                DrawGridPreview();
            }
            else
            {
                // Preview legacy attack area
                Gizmos.color = Color.gray;
                Vector2 previewOffset = new Vector2(0f, attackOffset.y);
                Gizmos.DrawWireCube((Vector2)transform.position + previewOffset, attackAreaSize);
            }
        }
    }

    private void DrawGridGizmos()
    {
        if (activeGridCells == null)
            return;

        // Draw active grid cells
        Gizmos.color = isAttacking ? Color.red : Color.yellow;
        foreach (Vector2 cellPosition in activeGridCells)
        {
            Gizmos.DrawWireCube(cellPosition, new Vector3(gridCellSize, gridCellSize, 0));
        }

        // Draw complete 3x3 grid untuk reference
        Gizmos.color = Color.white;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 cellPos = gridCenter + new Vector2(x * gridCellSize, y * gridCellSize);
                Gizmos.DrawWireCube(cellPos, new Vector3(gridCellSize, gridCellSize, 0));
            }
        }

        // Draw player position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    private void DrawLegacyGizmos()
    {
        // Legacy attack area
        Gizmos.color = isAttacking ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(currentAttackPosition, attackAreaSize);
    }

    private void DrawGridPreview()
    {
        // Preview grid system dengan default up direction
        Vector2 previewCenter = transform.position;

        // Draw preview active cells (up direction)
        Gizmos.color = Color.yellow;
        Vector2[] previewCells =
        {
            previewCenter + new Vector2(-gridCellSize, gridCellSize), // Grid 1
            previewCenter + new Vector2(0, gridCellSize), // Grid 2
            previewCenter + new Vector2(gridCellSize, gridCellSize) // Grid 3
        };

        foreach (Vector2 cellPos in previewCells)
        {
            Gizmos.DrawWireCube(cellPos, new Vector3(gridCellSize, gridCellSize, 0));
        }

        // Draw complete 3x3 grid untuk reference
        Gizmos.color = Color.gray;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 cellPos = previewCenter + new Vector2(x * gridCellSize, y * gridCellSize);
                Gizmos.DrawWireCube(cellPos, new Vector3(gridCellSize, gridCellSize, 0));
            }
        }
    }
}

// Interface untuk damage system (optional, tapi recommended)
public interface IDamageable
{
    void TakeDamage(int damage, Vector2 knockbackDirection);
}
