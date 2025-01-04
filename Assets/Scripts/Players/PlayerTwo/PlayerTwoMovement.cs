﻿using UnityEngine;
using UnityEngine.Events;

public class PlayerTwoMovement : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                            // Amount of force added when the player jumps.
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                          // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                           // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                            // A position marking where to check if the player is grounded.

    [SerializeField] private GameObject punchGameObject;

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    Animator animator;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    public void Move(float move, bool jump)
    {
        // Only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // Flip the character if needed
            if (move > 0 && !m_FacingRight)
                Flip();
            else if (move < 0 && m_FacingRight)
                Flip();
        }
        // If the player should jump
        if (m_Grounded && jump)
        {
            // Add a vertical force to the player
            animator.SetBool("isJump", true);
            m_Grounded = true;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }

    public void onLanding()
    {
        animator.SetBool("isJump", false);
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void Update()
    {
        // Get input values for Player 2
        float move = Input.GetAxis("Horizontal_P2"); // Player 2-specific horizontal axis

        // Check if Player 2 presses the "X" button (joystick 2 button 0) circle 1, square 2, triangle 3
        bool jump = Input.GetKeyDown(KeyCode.Joystick2Button0);  // Directly check the button press

        if (Input.GetKeyDown(KeyCode.Joystick2Button2))
        {
            // animator.SetTrigger("isPunch");
            punchGameObject.SetActive(true);
        }

        animator.SetFloat("speed", Mathf.Abs(move));
        // animator.SetBool("isGrounded", m_Grounded);

        Move(move, jump);
    }
}
