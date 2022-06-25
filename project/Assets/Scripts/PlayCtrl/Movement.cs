using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Collision coll;

    [HideInInspector]
    public Rigidbody2D rb;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool isDashing;

    [Space]
    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);

        rb.gravityScale = 3;

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (Input.GetButtonDown("Jump"))
        {
            // anim.SetTrigger("jump");
            if (coll.onGround)
                Jump(Vector2.up, false);
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        if (!canMove)
            return;

        if (x > 0)
        {
            side = 1;
            // anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            // anim.Flip(side);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        // side = anim.sr.flipX ? -1 : 1;
    }
}
