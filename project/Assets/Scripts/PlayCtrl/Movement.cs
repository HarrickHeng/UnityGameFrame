using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class Movement : MonoBehaviour
    {
        private Collision coll;
        public AnimComponent animComponent;
        public AnimationClip idleClip;
        public AnimationClip runClip;
        public AnimationClip jumpClip;
        public AnimationClip attack01Clip;
        public AnimationClip attack02Clip;
        private AnimGraph graph;

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

        void Awake()
        {
            graph = animComponent.GetGraph();
            graph.Play(idleClip);
        }

        void Update()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            Vector2 dir = new Vector2(x, y);

            Walk(dir);

            rb.gravityScale = 3;

            if (coll.onGround && !groundTouch)
            {
                GroundTouch();
                groundTouch = true;
            }

            if (
                (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                && coll.onGround
            )
            {
                Down();
            }

            if (Input.GetButtonDown("Fire1"))
            {
                var state = graph.Play(attack01Clip);
                state.OnEnd = () =>
                {
                    graph.CrossFade(idleClip);
                };
            }

            if (Input.GetButtonDown("Fire2"))
            {
                var state = graph.Play(attack02Clip);
                state.OnEnd = () =>
                {
                    graph.CrossFade(idleClip);
                };
            }

            if (Input.GetButtonDown("Jump"))
            {
                graph.CrossFade(jumpClip);
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
                ChangeFlip(side);
            }
            if (x < 0)
            {
                side = -1;
                ChangeFlip(side);
            }
        }

        private void Jump(Vector2 dir, bool wall)
        {
            if (isDowned)
            {
                gameObject.FindChild("Heshang").SetActive(true);
                gameObject.FindChild("ArrowDown").SetActive(false);
                gameObject.GetComponent<CapsuleCollider2D>().isTrigger = false;
                rb.isKinematic = false;
                isDowned = false;
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.velocity += dir * jumpForce;
            }
        }

        private void ChangeFlip(int side)
        {
            SpriteRenderer spr = gameObject.FindChild("Heshang").GetComponent<SpriteRenderer>();
            spr.flipX = side < 0;
        }

        private Vector2 lastPos;

        private void Walk(Vector2 dir)
        {
            if (!canMove)
                return;
            if (IsPlaying(attack01Clip) || IsPlaying(attack02Clip))
            {
                return;
            }

            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
            if (rb.velocity.magnitude <= 0.02f)
            {
                graph.CrossFade(idleClip);
            }
            else
            {
                graph.CrossFade(runClip);
            }
        }

        private bool IsPlaying(AnimationClip clip)
        {
            var state = graph.GetState(clip);
            return state != null && state.IsPlaying;
        }

        private bool isDowned = false;

        private void Down()
        {
            isDowned = true;
            gameObject.FindChild("Heshang").SetActive(false);
            gameObject.FindChild("ArrowDown").SetActive(true);
            gameObject.GetComponent<CapsuleCollider2D>().isTrigger = true;
            rb.isKinematic = true;
        }

        void GroundTouch()
        {
            hasDashed = false;
            isDashing = false;
            side = gameObject.FindChild("Heshang").GetComponent<SpriteRenderer>().flipX ? -1 : 1;
        }
    }
}
