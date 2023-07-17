using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    protected bool isChasing = false;
    [Space(7)]

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float attackStrength;
    [SerializeField] protected float chasingDistance;

    public bool isAttacking = false;
    public bool isBeingAttacked = false;

    protected Rigidbody2D rb;
    protected Vector3 objectScale;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
        objectScale = this.transform.localScale;
    }
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public virtual void EnemyHit(float _damage, Vector2 _hitDir, float _hitForce)
    {
        health -= _damage;
    }
    protected virtual void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invicible && !PlayerController.Instance.pState.dashing)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 10, 0.5f);
        }
    }
    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(attackStrength);
    }
    protected virtual void Flip()
    {
        if (PlayerController.Instance.transform.position.x >= this.transform.position.x)
        {
            this.transform.localScale = new Vector3(-objectScale.x, objectScale.y, objectScale.z);
        }
        else
        {
            this.transform.localScale = new Vector3(objectScale.x, objectScale.y, objectScale.z);
        }
    }
}
