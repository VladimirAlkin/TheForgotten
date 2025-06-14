using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Tamu : Enemy
{
    public static Tamu Instance;
    public float attackCooldown = 4f;
    private float attackTimer;
    [HideInInspector] public bool facingRight;
    [Header("Ground Check Settings:")]
    public Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    public GameObject barrageFireball;

    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();        
        playerTransform = PlayerController.Instance.transform; // Assuming PlayerController is a singleton
        attackTimer = attackCooldown; // Start with cooldown to avoid immediate attack
        anim = GetComponent<Animator>(); // Assign the Animator
    }



    protected override void Update()
    {
        base.Update();
        if (health > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                StartCoroutine(Barrage());
                attackTimer = attackCooldown; // Reset the attack timer
            }
        }
        if (health <= 0) 
        {
            anim.SetBool("Death", true);
            Color flashColor = Color.green;
            flashColor.a = Mathf.PingPong(Time.time * 35, 2.0f);
            sr.color = flashColor;
            Destroy(gameObject, 2f);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (playerTransform != null)
        {
            if (playerTransform.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
            else if (playerTransform.position.x < transform.position.x && facingRight)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public IEnumerator Barrage()
    {
        FlipTowardsPlayer(); // Ensure Tamu faces the player before attacking

        rb.velocity = Vector2.zero;

        float _currentAngle = -30f; // Adjust initial angle as needed
        Vector3 direction = (playerTransform.position - transform.position).normalized; // Calculate the direction towards the player

        for (int i = 0; i < 4; i++)
        {
            anim.SetBool("Cast", true);            
            GameObject _projectile = Instantiate(barrageFireball, transform.position, Quaternion.identity);

           
            // Set the projectile's direction to face the player
            _projectile.transform.right = direction;
            _projectile.transform.eulerAngles += new Vector3(0, 0, _currentAngle);

            _currentAngle += 10f; // Adjust angle increment as needed

            yield return new WaitForSeconds(0.45f);
        }
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("Cast", false);
    }


}
