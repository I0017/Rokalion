using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    protected bool isRecoiling = false;
    protected bool isChasing = false;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float attackStrength;

    [SerializeField] protected float chasingDistance;

    protected float recoilTimer;

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
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }
    public virtual void EnemyHit(float _damage, Vector2 _hitDir, float _hitForce)
    {
        health -= _damage;
        if(!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDir);
        }
    }
    protected virtual void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invicible && !PlayerController.Instance.pState.dashing)
        {
            Attack();
            PlayerController.Instance.kbCounter = PlayerController.Instance.kbTotalTime;
            if (coll.transform.position.x <= transform.position.x)
            {
                PlayerController.Instance.kbFromRight = true;
            }
            else
            {
                PlayerController.Instance.kbFromRight = false;
            }
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
