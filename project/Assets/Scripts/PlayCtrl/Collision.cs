using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask attackLayer;

    [Space]
    public bool onGround;
    public Vector2 bottomOffset;

    [Header("附近有敌人")]
    public bool onOther;

    [Space]
    [Header("拳击范围")]
    public float collisionRadius = 0.02f;
    public Vector2 rightOffset,
        leftOffset;

    public Collider2D otherColL = null;
    public Collider2D otherColR = null;

    [Header("脚踢范围")]
    public float collisionRadius2 = 0.02f;
    public Vector2 rightOffset2,
        leftOffset2;

    public Collider2D otherCol2L = null;
    public Collider2D otherCol2R = null;
    private Color debugCollisionColor = Color.red;

    [Header("角色属性")]
    public float hp = 100.0f;
    private float lastHP;
    public float skill01_atk = 10.0f;
    public float skill02_atk = 10.0f;

    public bool onBeHit;

    void Start()
    {
        onBeHit = false;
        lastHP = hp;
    }

    void Update()
    {
        //被攻击了
        if (lastHP > hp)
        {
            lastHP = hp;
            onBeHit = true;
        }

        //是否落地
        onGround = Physics2D.OverlapCircle(
            (Vector2)transform.position + bottomOffset,
            collisionRadius,
            groundLayer
        );

        //skill01
        otherColL = Physics2D.OverlapCircle(
            (Vector2)transform.position + leftOffset,
            collisionRadius,
            attackLayer
        );
        otherColR = Physics2D.OverlapCircle(
            (Vector2)transform.position + rightOffset,
            collisionRadius,
            attackLayer
        );

        //skill02
        otherCol2L = Physics2D.OverlapCircle(
            (Vector2)transform.position + leftOffset2,
            collisionRadius2,
            attackLayer
        );
        otherCol2R = Physics2D.OverlapCircle(
            (Vector2)transform.position + rightOffset2,
            collisionRadius2,
            attackLayer
        );

        if (CheckIsSelf(otherColL))
        {
            otherColL = null;
        }
        if (CheckIsSelf(otherColR))
        {
            otherColR = null;
        }
        if (CheckIsSelf(otherCol2L))
        {
            otherCol2L = null;
        }
        if (CheckIsSelf(otherCol2R))
        {
            otherCol2R = null;
        }

        if (otherColL == null && otherColR == null && otherCol2L == null && otherCol2R == null)
        {
            onOther = false;
        }
        else
        {
            onOther = true;
        }
    }

    private bool CheckIsSelf(Collider2D coll)
    {
        if (coll != null && gameObject.GetHashCode() == coll.gameObject.GetHashCode())
        {
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);

        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);

        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset2, collisionRadius2);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset2, collisionRadius2);
    }
}
