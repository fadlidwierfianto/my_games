using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool FacingLeft
    {
        get { return facingLeft; }
        set { facingLeft = value; }
    }

    //public static PlayerController Instance;
    [SerializeField]
    private float moveSpeed = 1f;
    private PlayerControl playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;

    private Animator anim;
    public SpriteRenderer sprite;
    private bool facingLeft = false;
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
}
