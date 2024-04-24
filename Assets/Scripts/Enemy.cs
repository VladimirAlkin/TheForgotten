using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy settings")] 
    [SerializeField] float health;
    [SerializeField] float recoilLength;
    [SerializeField] float recoilFactor;
    [SerializeField] bool isRecoiling = false;

    float recoilTimer;
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) //Death check
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

    public void EnemyHit (float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling )
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }
}
