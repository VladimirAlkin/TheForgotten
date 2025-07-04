using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton_warrior : Enemy
{
    [Header("Crawler Settings:")]
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;

    float timer;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rb.gravityScale = 12f;
    }

    protected override void UpdateEnemyStates()
    {
        switch (currentEnemyState)
        {
            case EnemyStates.Crawler_Idle:
                Vector3 _ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
                Vector2 _wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

                if (!Physics2D.Raycast(transform.position + _ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
                    || Physics2D.Raycast(transform.position, _wallCheckDir, ledgeCheckX, whatIsGround))
                {
                    ChangeState(EnemyStates.Crawler_Flip);
                }
                if (transform.localScale.x > 0)
                {
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(-speed, rb.velocity.y);
                }
                break;

            case EnemyStates.Crawler_Flip:
                timer += Time.deltaTime;

                if (timer > flipWaitTime)
                {
                    timer = 0;
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    ChangeState(EnemyStates.Crawler_Idle);
                }
                break;
        }
    }
}
