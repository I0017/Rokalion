using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingRockHuman : Enemy
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite attackSprite;
    [Space(7)]

    [SerializeField] private GameObject draggingSounds;

    protected override void Start()
    {
        base.Start();
        StopFootsteps();
        rb.gravityScale = 12f;
    }
    protected override void Update()
    {
        if (!isAttacking)
        {
            this.GetComponent<SpriteRenderer>().sprite = idleSprite;
        }
        base.Update();
        Chase();
        Flip();
    }
    public override void EnemyHit(float _damage, Vector2 _hitDir, float _hitForce)
    {
        base.EnemyHit(_damage, _hitDir, _hitForce);
    }
    protected override void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invicible && !PlayerController.Instance.pState.dashing)
        {
            PlayerController.Instance.kbCounter = PlayerController.Instance.kbTotalTime;
            isAttacking = true;
            base.OnCollisionStay2D(coll);
            this.GetComponent<SpriteRenderer>().sprite = attackSprite;
        }
    }
    protected override void Flip()
    {
        base.Flip();
    }
    private void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            isAttacking = false;
        }
    }
    private void Chase()
    {
        float distance = Vector3.Distance(PlayerController.Instance.transform.position, this.transform.position);
        if (distance <= chasingDistance
            && PlayerController.Instance.transform.position.x >= patrolPoints[1].transform.position.x
            && PlayerController.Instance.transform.position.x <= patrolPoints[0].transform.position.x
            && transform.position.x <= patrolPoints[0].position.x + 0.5 && transform.position.x >= patrolPoints[1].position.x - 0.5)
        {
            Footsteps();
            transform.position = Vector2.MoveTowards
                (transform.position,
                new Vector2(PlayerController.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
        }
        else
        {
            StopFootsteps();
        }
    }
    void Footsteps()
    {
        draggingSounds.SetActive(true);
    }
    void StopFootsteps()
    {
        draggingSounds.SetActive(false);
    }
}
