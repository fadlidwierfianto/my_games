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

//     [Header("Knockback Settings")]
//     [SerializeField]
//     private float knockBackTime = 0.2f;

//     [SerializeField]
//     private float knockBackThrust = 10f;

//     private bool isKnockedBack = false;

//     private Animator anim;
//     public SpriteRenderer sprite;

//     // private bool facingLeft = false;
//     public Vector2 moveDir
//     {
//         get { return movement; }
//     }

//     private void Awake()
//     {
//         playerControl = new PlayerControl();
//         rb = GetComponent<Rigidbody2D>();
//         anim = GetComponent<Animator>();
//         sprite = GetComponent<SpriteRenderer>();
//         currentHealth = maxHealth;
//         UpdateHealthUI();
//     }

//     private void OnEnable()
//     {
//         playerControl.Enable();
//     }

//     private void Update()
//     {
//         PlayerInput();
//     }

//     private void FixedUpdate()
//     {
//         if (isKnockedBack)
//             return;
//         Move();
//     }

//     private void PlayerInput()
//     {
//         movement = playerControl.Movement.Move.ReadValue<Vector2>();
//         PlayerMoveDirection = new Vector3(movement.x, movement.y).normalized;

//         anim.SetFloat("moveX", movement.x);
//         anim.SetFloat("moveY", movement.y);

//         if (PlayerMoveDirection == Vector3.zero)
//         {
//             anim.SetBool("moving", false);
//         }
//         else
//         {
//             anim.SetBool("moving", true);
//         }
//     }

//     private void Move()
//     {
//         rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
//     }

//     public void TakeDamage(int damage, Vector2 direction)
//     {
//         if (isKnockedBack)
//             return; // Jangan stack knockback

//         currentHealth -= damage;
//         if (currentHealth <= 0)
//         {
//             currentHealth = 0;
//             Debug.Log("Player Mati");
//         }

//         StartCoroutine(HandleKnockback(direction.normalized));
//         UpdateHealthUI();
//     }

//     private void UpdateHealthUI()
//     {
//         if (healthText != null)
//             healthText.text = "Health: " + currentHealth;
//     }

//     private IEnumerator HandleKnockback(Vector2 direction)
//     {
//         isKnockedBack = true;
//         rb.velocity = Vector2.zero;

//         Vector2 force = direction * knockBackThrust * rb.mass;
//         rb.AddForce(force, ForceMode2D.Impulse);

//         yield return new WaitForSeconds(knockBackTime);
//         rb.velocity = Vector2.zero;
//         isKnockedBack = false;
//     }
// }