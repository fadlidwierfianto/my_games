using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // public bool FacingLeft
    // {
    //     get { return facingLeft; }
    //     set { facingLeft = value; }
    // }

    //public static PlayerController Instance;
    [SerializeField]
    private float moveSpeed = 1f;
    private PlayerControl playerControl;
    private Vector2 movement;
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
    // private bool facingLeft = false;
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
        currentHealth = maxHealth;
        UpdateHealthUI();
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
        // AdjustPlayerFacingDirection();
        Move();
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();

        anim.SetFloat("moveX", movement.x);
        anim.SetFloat("moveY", movement.y);
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    // private void AdjustPlayerFacingDirection()
    // {
    //     if (movement.x != 0f)
    //     {
    //         sprite.flipX = movement.x < 0f;
    //         FacingLeft = true;
    //     }
    //     else if (movement.y != 0f)
    //     {
    //         sprite.flipX = false;
    //         FacingLeft = false;
    //     }
    // }

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
}
