using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameCore.Animation
{
    public class AIMovement : MonoBehaviour
    {
        private Collision coll;
        public AnimComponent animComponent;
        public AnimationClip idleClip;
        public AnimationClip runClip;
        public AnimationClip jumpClip;
        public AnimationClip jumpBackClip;
        public AnimationClip attack01Clip;
        public AnimationClip attack02Clip;
        public AnimationClip beHitClip;
        public AnimationClip deadClip;
        private AnimGraph graph;
        private GameObject heshang;

        [HideInInspector]
        public Rigidbody2D rb;

        public EAIState CurrentState = EAIState.Ilde;

        [Space]
        [Header("Stats")]
        public float speed = 10;
        public float jumpForce = 50;
        public float backForce = 50;
        public float slideSpeed = 5;

        [Space]
        [Header("Booleans")]
        public bool canMove;

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
            heshang = gameObject.FindChild("HeshangE");
            coll = GetComponent<Collision>();
            rb = GetComponent<Rigidbody2D>();
            CDMgr.instance.AddCD("skill01" + gameObject.GetHashCode(), skill01_CD);
            CDMgr.instance.AddCD("skill02" + gameObject.GetHashCode(), skill02_CD);
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
                graph.Play(beHitClip);
                coll.onBeHit = false;
            }

            rb.gravityScale = 3;

            switch (CurrentState)
            {
                case EAIState.Ilde:
                    graph.CrossFade(idleClip);
                    rb.velocity = Vector2.zero;
                    if (coll.onOther)
                    {
                        CurrentState = EAIState.Attack;
                    }
                    break;
                case EAIState.Attack:
                    rb.velocity = Vector2.zero;
                    if (!IsPlaying(attack01Clip) && !IsPlaying(attack02Clip))
                    {
                        if (CDMgr.instance.IsReady("skill01" + gameObject.GetHashCode()))
                        {
                            var state = graph.Play(attack01Clip);
                            state.OnEnd = () =>
                            {
                                graph.CrossFade(idleClip);
                                Attack(1);
                            };
                        }

                        if (CDMgr.instance.IsReady("skill02" + gameObject.GetHashCode()))
                        {
                            var state = graph.Play(attack02Clip);
                            state.OnEnd = () =>
                            {
                                graph.CrossFade(idleClip);
                                Attack(2);
                            };
                        }
                    }

                    if (!coll.onOther)
                    {
                        CurrentState = EAIState.Move;
                    }
                    break;
                case EAIState.Move:
                    break;
            }

            // Walk(dir);

            // if (!canMove)
            //     return;

            // if (x > 0)
            // {
            //     side = 1;
            //     ChangeFlip(side);
            // }
            // if (x < 0)
            // {
            //     side = -1;
            //     ChangeFlip(side);
            // }
        }

        private void Jump(Vector2 dir, bool wall)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
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
            rb.AddForce(new Vector2(-side, 0.38f) * backForce);
            graph.CrossFade(jumpBackClip);
            await UniTask.Delay(300);
            heshang.GetComponent<GhostEffect>().openGhoseEffect = false;
        }
    }
}

public enum EAIState
{
    Ilde,
    Attack,
    Move
}
