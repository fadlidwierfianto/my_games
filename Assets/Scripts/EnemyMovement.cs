using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private float stoppingDistance = 0.5f;

    [SerializeField]
    private float detectionRange = 10f;

    [Header("Components")]
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugGizmos = true;

    private Vector2 movement;
    private Vector2 lastMovement;
    private bool isMoving = false;

    void Start()
    {
        // Inisialisasi komponen
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Cari player berdasarkan tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player tidak ditemukan! Pastikan player memiliki tag 'Player'");
        }

        // Pastikan Rigidbody2D tidak terpengaruh gravity jika ini game top-down
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Hitung jarak ke player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Cek apakah player dalam jangkauan detection
        if (distanceToPlayer <= detectionRange)
        {
            // Hitung arah ke player
            Vector2 direction = (player.position - transform.position).normalized;

            // Cek apakah sudah cukup dekat dengan player
            if (distanceToPlayer > stoppingDistance)
            {
                movement = direction;
                isMoving = true;
            }
            else
            {
                movement = Vector2.zero;
                isMoving = false;
            }
        }
        else
        {
            // Player di luar jangkauan, berhenti bergerak
            movement = Vector2.zero;
            isMoving = false;
        }

        // Update animator parameters
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // Gerakkan enemy menggunakan Rigidbody2D
        if (rb != null && isMoving)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        // Jika sedang bergerak, gunakan movement vector saat ini
        if (isMoving)
        {
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
            lastMovement = movement;
        }
        else
        {
            // Jika tidak bergerak, gunakan arah terakhir untuk idle animation
            animator.SetFloat("moveX", lastMovement.x);
            animator.SetFloat("moveY", lastMovement.y);
        }

        // Optional: Set boolean untuk state bergerak
        animator.SetBool("isMoving", isMoving);
    }

    // Method untuk mengubah target player (jika diperlukan)
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    // Method untuk mengubah kecepatan movement
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    // Method untuk menghentikan enemy sementara
    public void StopMovement()
    {
        movement = Vector2.zero;
        isMoving = false;
        UpdateAnimator();
    }

    // Method untuk melanjutkan movement
    public void ResumeMovement()
    {
        // Movement akan otomatis resume di Update()
    }

    // Gizmos untuk debug di Scene view
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
            return;

        // Draw detection range (circle)
        Gizmos.color = Color.yellow;
        DrawWireCircle2D(transform.position, detectionRange);

        // Draw stopping distance (circle)
        Gizmos.color = Color.red;
        DrawWireCircle2D(transform.position, stoppingDistance);

        // Draw line to player
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Draw movement direction
        if (isMoving)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, movement * 2f);
        }
    }

    // Helper method untuk draw circle di 2D
    void DrawWireCircle2D(Vector3 center, float radius)
    {
        int segments = 36;
        float angleStep = 360f / segments;

        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint =
                center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
