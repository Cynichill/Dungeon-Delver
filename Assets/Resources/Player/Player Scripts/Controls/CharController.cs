using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour
{

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private BoxCollider2D interactBox;
    [SerializeField] private float moveSpeed = 5;

    //Control
    public PlayerControl controls;

    private bool antiSpam = false;

    private bool m_FacingRight = true;  // For determining which way the player is currently facing.

    //Animation Variables
    private string currentState;
    private Animator anim;
    private const string PLAYER_IDLE = "player_idle";
    private const string PLAYER_WALK = "player_walk";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        interactBox = GameObject.FindGameObjectWithTag("Interaction").GetComponent<BoxCollider2D>();

        controls = new PlayerControl();
        controls.Player.Interact.performed += ctx => Interact();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.deltaTime);
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput == new Vector2(0, 0))
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
        else
        {
            ChangeAnimationState(PLAYER_WALK);
        }

        // If the input is moving the player right and the player is facing left...
        if (moveInput.x > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && m_FacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void Interact()
    {
        if (!antiSpam)
        {
            interactBox.enabled = true;
            antiSpam = true;
            StartCoroutine(ActivateDeactivateBox());
        }
    }

    private IEnumerator ActivateDeactivateBox()
    {
        yield return new WaitForSeconds(0.3f);
        interactBox.enabled = false;
        antiSpam = false;
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        //play the animation
        anim.Play(newState);
        //reassign the current state
        currentState = newState;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
