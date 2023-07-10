using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Enemy
{
    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 12f;
    }
    protected override void Update()
    {
        base.Update();
        float distance = Vector3.Distance(PlayerController.Instance.transform.position, this.transform.position);
        if (!isRecoiling && distance <= chasingDistance)
        {
            transform.position = Vector2.MoveTowards
                (transform.position, 
                new Vector2(PlayerController.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
        }
    }
    public override void EnemyHit(float _damage, Vector2 _hitDir, float _hitForce)
    {
        base.EnemyHit(_damage, _hitDir, _hitForce);
    }
}
