using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Mara : Enemy
{
    public static Mara Instance;
    public float attackCooldown = 4f;
    private float attackTimer;
    [HideInInspector] public bool facingRight;
    [Header("Ground Check Settings:")]
    public Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    public GameObject barrageFireball;
    public GameObject pillar;

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
                StartCoroutine(DivingPillars());
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

    public IEnumerator DivingPillars()
    {
        anim.SetTrigger("Cast");
        Vector2 _impactPoint = groundCheckPoint.position;
        float _spawnDistance = 2f; // Adjust this as needed
        float _pillarHeight = 2f; // Adjust this as needed

        
       
        Debug.Log("Impact Point: " + _impactPoint);
        FlipTowardsPlayer();
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.3f);
            // Calculate spawn points
            Vector2 _pillarSpawnPointRight = _impactPoint + new Vector2(_spawnDistance, _pillarHeight);
            Vector2 _pillarSpawnPointLeft = _impactPoint - new Vector2(_spawnDistance, -_pillarHeight);

            // Debugging lines to check the spawn points
            Debug.Log("Right Spawn Point: " + _pillarSpawnPointRight);
            Debug.Log("Left Spawn Point: " + _pillarSpawnPointLeft);

            // Instantiate pillars
            GameObject rightPillar = Instantiate(pillar, _pillarSpawnPointRight, Quaternion.identity);
            GameObject leftPillar = Instantiate(pillar, _pillarSpawnPointLeft, Quaternion.identity);

            // Debug positions after instantiation
            Debug.Log("Right Pillar Position After Instantiation: " + rightPillar.transform.position);
            Debug.Log("Left Pillar Position After Instantiation: " + leftPillar.transform.position);

            if (rightPillar == null || leftPillar == null)
            {
                Debug.LogError("Failed to instantiate pillar at: " + _pillarSpawnPointRight + " and " + _pillarSpawnPointLeft);
            }
            
            _spawnDistance += 2f; // Adjust this increment as needed
        }
        
        yield return new WaitForSeconds(0.80f);
        anim.SetBool("Cast", false);
        // Reset Cast animation parameter after casting

    }



}
