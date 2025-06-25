using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    public float chaseSpeed = 3f;
    public float detectionRange = 10f;
    public bool alwaysChase = true; // Set true untuk selalu mengejar

    [Header("Animation")]
    private Animator animator;
    private Rigidbody2D rb;

    // Parameter animator sesuai dengan setup Anda
    private readonly string moveXParam = "moveX";
    private readonly string moveYParam = "moveY";
    private readonly string movingParam = "moving";

    private Vector2 movement;
    private Vector2 lastMovement;

    void Start()
    {
        // Dapatkan komponen yang dibutuhkan
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Jika player tidak di-assign, cari GameObject dengan tag "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Hitung jarak ke player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Optimasi: Kurangi update frequency untuk enemy yang jauh
        if (distanceToPlayer > 50f && Time.time % 0.5f > 0.1f)
            return;

        // Tentukan apakah enemy harus mengejar
        bool shouldChase = alwaysChase || distanceToPlayer <= detectionRange;

        if (shouldChase)
        {
            ChasePlayer();
        }
        else
        {
            StopChasing();
        }

        // Update animasi
        UpdateAnimation();
    }

    void ChasePlayer()
    {
        // Hitung arah ke player
        Vector2 direction = (player.position - transform.position).normalized;
        movement = direction;

        // Gerakkan enemy
        rb.velocity = movement * chaseSpeed;

        // Simpan arah terakhir untuk animasi
        if (movement.magnitude > 0.1f)
        {
            lastMovement = movement;
        }
    }

    void StopChasing()
    {
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    void UpdateAnimation()
    {
        // Set parameter animasi
        bool isMoving = movement.magnitude > 0.1f;

        animator.SetFloat(moveXParam, lastMovement.x);
        animator.SetFloat(moveYParam, lastMovement.y);
        animator.SetBool(movingParam, isMoving);
    }

    void FixedUpdate()
    {
        // Alternative movement menggunakan FixedUpdate untuk physics yang lebih smooth
        // Uncomment jika ingin menggunakan ini dan comment bagian rb.velocity di ChasePlayer()

        /*
        if (movement.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + movement * chaseSpeed * Time.fixedDeltaTime);
        }
        */
    }

    // Method untuk debugging - tampilkan detection range di scene view
    void OnDrawGizmosSelected()
    {
        if (!alwaysChase)
        {
            Gizmos.color = Color.red;

            // Gambar lingkaran menggunakan segments untuk kompatibilitas
            int segments = 32;
            float angle = 0f;
            Vector3 lastPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * detectionRange;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * detectionRange;
                Vector3 currentPoint = transform.position + new Vector3(x, y, 0);

                if (i > 0)
                {
                    Gizmos.DrawLine(lastPoint, currentPoint);
                }

                lastPoint = currentPoint;
                angle += (360f / segments);
            }
        }
    }
}
