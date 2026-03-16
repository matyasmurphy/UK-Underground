using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    public Animator animator;

    public SpriteRenderer body;
    public SpriteRenderer head;
    public SpriteRenderer armLeft;
    public SpriteRenderer forearmLeft;
    public SpriteRenderer armRight;
    public SpriteRenderer forearmRight;
    public SpriteRenderer leftThigh;
    public SpriteRenderer leftShin;
    public SpriteRenderer rightThigh;
    public SpriteRenderer rightShin;

    [SerializeField] private Transform graphics;
    private bool facingRight = true;

    public float moveSpeed;
    private float playerInput;

    public LayerMask groundLayer;
    private bool isGrounded;
    private Vector2 slopeNormal;
    void Flip()
    {
        Vector3 scale = graphics.localScale;
        scale.x *= -1;
        graphics.localScale = scale;
    }

    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        if (hit)
        {
            isGrounded = true;
            slopeNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            slopeNormal = Vector2.up;
        }
    }

    void Update()
    {
        playerInput = Input.GetAxisRaw("Horizontal");

        if (playerInput > 0 && !facingRight)
        {
            facingRight = true;
            Flip();
        }
        else if (playerInput < 0 && facingRight)
        {
            facingRight = false;
            Flip();
        }

        animator.SetFloat("Speed", Mathf.Abs(playerInput));
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (isGrounded)
        {
            Vector2 slopeDirection = new Vector2(slopeNormal.y, -slopeNormal.x);
            Vector2 moveDirection = slopeDirection * -playerInput * moveSpeed * Time.fixedDeltaTime;
            rigidbody.linearVelocity = new Vector2(moveDirection.x, moveDirection.y);
        }
        else
        {
            rigidbody.linearVelocity = new Vector2(playerInput * moveSpeed * Time.fixedDeltaTime, rigidbody.linearVelocity.y);
        }
    }
}
