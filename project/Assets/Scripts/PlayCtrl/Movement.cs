using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameCore.Animation
{
    public class Movement : MonoBehaviour
    {
        private Collision coll;
        public AnimComponent animComponent;
        public AnimationClip idleClip;
        public AnimationClip runClip;
        public AnimationClip jumpClip;
        public AnimationClip jumpBackClip;
        public AnimationClip attack01Clip;
        public AnimationClip attack02Clip;
        public AnimationClip deadClip;
        private AnimGraph graph;
        private GameObject heshang;
        private GameObject arrowDown;

        [HideInInspector]
        public Rigidbody2D rb;

        [Space]
        [Header("Stats")]
        public float speed = 10;
        public float jumpForce = 50;
        public float backForce = 50;
        public float slideSpeed = 5;

        [Space]
        [Header("Booleans")]
        public bool canMove;

        [Space]
        private bool groundTouch;

        [Header("技能冷却时间")]
        public float downSkill_CD = 5.0f;
        public float skill01_CD = 5.0f;
        public float skill02_CD = 5.0f;
        public float backSkill_CD = 2.0f;

        [Header("技能持续时间")]
        public float downSkill_Buff = 2.0f;

        public int side = 1;

        void Start()
        {
            heshang = gameObject.FindChild("Heshang");
            arrowDown = gameObject.FindChild("ArrowDown");
            coll = GetComponent<Collision>();
            rb = GetComponent<Rigidbody2D>();
            CDMgr.instance.AddCD("downSkill", downSkill_CD); //遁地冷却时间
            CDMgr.instance.AddCD("downSkill_Buff", downSkill_Buff); //遁地持续时间
            CDMgr.instance.AddCD("skill01", skill01_CD);
            CDMgr.instance.AddCD("skill02", skill02_CD);
            CDMgr.instance.AddCD("backSkill", backSkill_CD);
        }

        void Awake()
        {
            graph = animComponent.GetGraph();
            graph.Play(idleClip);
        }

        private void Attack(int skill)
        {
            if (coll.onOther)
            {
                switch (skill)
                {
                    case 1:
                        if (coll.otherColL != null)
                        {
                            coll.otherColL.GetComponent<Collision>().hp -= coll.skill01_atk;
                        }
                        else if (coll.otherColR != null)
                        {
                            coll.otherColR.GetComponent<Collision>().hp -= coll.skill01_atk;
                        }
                        break;
                    case 2:
                        if (coll.otherCol2L != null)
                        {
                            coll.otherCol2L.GetComponent<Collision>().hp -= coll.skill02_atk;
                        }
                        else if (coll.otherCol2R != null)
                        {
                            coll.otherCol2R.GetComponent<Collision>().hp -= coll.skill02_atk;
                        }
                        break;
                }
            }
        }

        async void Update()
        {
            if (coll.hp <= 0.0f)
            {
                Dead();
                return;
            }

            if (coll.onBeHit)
            {
                Material mat = new Material(Shader.Find("Custom/BossMouseShader"));
                mat.SetTexture(
                    "BossMouseShader",
                    Resources.Load("/Shaders/BossMouseShader") as Texture2D
                );
                coll.onBeHit = false;
            }

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
                && CDMgr.instance.IsReady("downSkill")
            )
            {
                OnDownSkill();
            }
            if (CDMgr.instance.IsReady("downSkill_Buff"))
            {
                OffDownSkill();
            }

            if (Input.GetButtonDown("Fire1") && CDMgr.instance.IsReady("skill01"))
            {
                var state = graph.Play(attack01Clip);
                state.OnEnd = () =>
                {
                    graph.CrossFade(idleClip);
                    Attack(1);
                };
            }

            if (Input.GetButtonDown("Fire2") && CDMgr.instance.IsReady("skill02"))
            {
                var state = graph.Play(attack02Clip);
                state.OnEnd = () =>
                {
                    graph.CrossFade(idleClip);
                    Attack(2);
                };
            }

            if (Input.GetButtonDown("Fire3") && CDMgr.instance.IsReady("backSkill"))
            {
                await BackSkill();
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
                OffDownSkill();
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.velocity += dir * jumpForce;
            }
        }

        private void ChangeFlip(int side)
        {
            SpriteRenderer spr = heshang.GetComponent<SpriteRenderer>();
            spr.flipX = side < 0;
        }

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
                graph.Play(runClip);
            }
        }

        private bool IsPlaying(AnimationClip clip)
        {
            var state = graph.GetState(clip);
            return state != null && state.IsPlaying;
        }

        private void StopClip(AnimationClip clip)
        {
            var state = graph.GetState(clip);
            if (state != null)
                state.Time = 0;
        }

        private bool isDowned = false;

        //遁地技能
        private void OnDownSkill()
        {
            heshang.SetActive(false);
            arrowDown.SetActive(true);
            gameObject.GetComponent<CapsuleCollider2D>().isTrigger = true;
            rb.isKinematic = true;
            isDowned = true;
        }

        private void OffDownSkill()
        {
            heshang.SetActive(true);
            arrowDown.SetActive(false);
            gameObject.GetComponent<CapsuleCollider2D>().isTrigger = false;
            rb.isKinematic = false;
            isDowned = false;
        }

        private void Dead()
        {
            var state = graph.CrossFade(deadClip);
            state.OnEnd = () =>
            {
                Debug.Log($"{gameObject.name} Dead!");
            };
        }

        //后退技能
        private async UniTask BackSkill()
        {
            heshang.GetComponent<GhostEffect>().openGhoseEffect = true;
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(new Vector2(-side, 0.38f) * backForce);
            graph.CrossFade(jumpBackClip);
            await UniTask.Delay(300);
            heshang.GetComponent<GhostEffect>().openGhoseEffect = false;
        }

        void GroundTouch()
        {
            side = heshang.GetComponent<SpriteRenderer>().flipX ? -1 : 1;
        }
    }
}
