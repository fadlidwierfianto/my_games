using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1f;
    private PlayerControl playerControl;
    private Vector2 movement;
    private Vector3 PlayerMoveDirection;
    private Rigidbody2D rb;

    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Knockback Settings")]
    [SerializeField]
    private float knockBackTime = 0.2f;

    [SerializeField]
    private float knockBackThrust = 10f;

    private bool isKnockedBack = false;

    private Animator anim;
    public SpriteRenderer sprite;

    // TAMBAHAN: Menyimpan arah terakhir untuk idle animation
    private Vector2 lastMoveDirection = Vector2.down; // Default menghadap bawah

    // TAMBAHAN: Reference ke PlayerAttack component
    private PlayerAttack playerAttack;

    public Vector2 moveDir
    {
        get { return movement; }
    }

    private void Awake()
    {
        playerControl = new PlayerControl();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        playerAttack = GetComponent<PlayerAttack>(); // Get PlayerAttack component

        currentHealth = maxHealth;
        UpdateHealthUI();

        // TAMBAHAN: Set initial animation direction
        UpdateAnimatorParameters();
    }

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void Update()
    {
        PlayerInput();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack)
            return;

        // MODIFIKASI: Cek apakah sedang attack, jika ya jangan move
        if (playerAttack != null && playerAttack.IsAttacking())
            return;

        Move();
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();
        PlayerMoveDirection = new Vector3(movement.x, movement.y).normalized;

        // PERBAIKAN: Update last direction dan animator parameters
        UpdateMovementDirection();
        UpdateAnimatorParameters();
    }

    // TAMBAHAN: Method untuk update movement direction
    private void UpdateMovementDirection()
    {
        // Simpan arah terakhir hanya jika ada input movement dan tidak sedang attack
        if (movement != Vector2.zero && (playerAttack == null || !playerAttack.IsAttacking()))
        {
            lastMoveDirection = movement.normalized;
        }
    }

    // PERBAIKAN: Method terpisah untuk update animator parameters
    private void UpdateAnimatorParameters()
    {
        // MODIFIKASI: Jangan update animator jika sedang attack
        if (playerAttack != null && playerAttack.IsAttacking())
            return;

        bool isMoving = movement != Vector2.zero;
        anim.SetBool("moving", isMoving);

        if (isMoving)
        {
            // Jika bergerak, gunakan current movement
            anim.SetFloat("moveX", movement.x);
            anim.SetFloat("moveY", movement.y);
        }
        else
        {
            // Jika idle, gunakan last direction untuk menentukan arah idle
            anim.SetFloat("moveX", lastMoveDirection.x);
            anim.SetFloat("moveY", lastMoveDirection.y);
        }
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack)
            return; // Jangan stack knockback

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
        }

        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    private IEnumerator HandleKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        Vector2 force = direction * knockBackThrust * rb.mass;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    // TAMBAHAN: Public methods untuk debugging atau akses dari script lain
    public Vector2 GetLastFacingDirection()
    {
        return lastMoveDirection;
    }

    public bool IsMoving()
    {
        return movement != Vector2.zero;
    }

    // TAMBAHAN: Method untuk mengecek apakah player sedang dalam aksi (knockback atau attack)
    public bool IsInAction()
    {
        return isKnockedBack || (playerAttack != null && playerAttack.IsAttacking());
    }
}
