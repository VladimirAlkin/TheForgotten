using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    /*
     SERILIZED AND PRIVATE VARIABLES
     */
    [Header("Health Settings")]
    [SerializeField] public int health;
    [SerializeField] public int maxHealth;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [Space(5)]

    [Header("Spell Settings")]
    //spell stats
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] float spellDamage; //upspellexplosion and downspellfireball    
    //spell cast objects
    [SerializeField] GameObject sideSpellFireball;    
    [Space(5)]

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 5;
    [Space(5)]

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 35;
    [Space(5)]

    // Frame buffer for jumping
    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;
    [SerializeField] private int maxFallingSpeed; //max fall speed
    [Space(5)]

    // Coyote time
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    [Space(5)]

    //Double Jump
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    //Dash
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [Space(5)]

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] GameObject dashEffect;
    [Space(5)]

    [Header("Attack Settings")]    
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damage;
    [SerializeField] GameObject slashEffect;
    [SerializeField] float timeBetweenAttacks;
    [SerializeField] float restoreTimeSpeed;
    bool restoreTime;
    bool attack = false;
    float timeSinceAttack;
    [Space(5)]


    [Header("Recoil settings")]
    [SerializeField] int recoilXSteps;
    [SerializeField] int recoilYSteps;
    [SerializeField] float recoilXSpeed;
    [SerializeField] float recoilYSpeed;
    int stepsXrecoiled, stepsYrecoiled;
    [Space(5)]

    public PlayerStateList pState;
    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    Animator anim;
    bool openInventory;
    private bool canDash = true;
    private bool dashed;
    private SpriteRenderer sr;




    /*
    SINGLETONE
    */

    public static PlayerController Instance;


    /*
     FUNCTIONS
     */

    // Singletone call
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

        DontDestroyOnLoad(gameObject);
        
    }


    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;

        Mana = mana;
        Health = maxHealth;

        manaStorage.fillAmount = Mana;

        if (Health == 0)
        {
            pState.alive = false;
            GameManager.Instance.RespawnPlayer();
        }

    }

    //visualisation of attack hitbox
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (pState.alive)
        {
            GetInputs();
            ToggleInventory();
        }
        UpdateJumpVariables();
        RestoreTimeScale();

        if (pState.dashing) return; //if dashing - no interaprting 
        Flip();
        Move();
        Jump();
        StartDash();        
        Recoil();
        Attack();        
        FlashWhileInvincible();
        if (!pState.alive) return;
        CastSpell();

    }

    private void OnTriggerEnter2D(Collider2D _other) //for up and down cast spell
    {
        if (_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyGetsHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.dashing) return;
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        openInventory = Input.GetButton("Inventory");
    }

    public int Health
    {
        get { return health;}
        set 
        {
            if (health != value)
            {

                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    float Mana
    {
        get { return mana; }
        set
        {
            //if mana stats change
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }


    public void TakeDamage(float _damage) 
    {
        
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());

        pState.recoilingX = true;
        pState.recoilingY = true;
        
        if (Health <= 0)
        {
            Debug.Log("DEATH");
            Health = 0;
            StartCoroutine(Death());
        }
    }  

    IEnumerator StopTakingDamage()
    {
        
        pState.invincible = true;        
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("TakeDamage");        
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    void FlashWhileInvincible()
    {
        if (pState.invincible)
        {
            // Flash between fully opaque and half-transparent
            Color flashColor = Color.white;
            flashColor.a = Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f);
            sr.color = flashColor;
        }
        else
        {
            sr.color = Color.white;
        }
    }

    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }


    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
        Time.timeScale = _newTimeScale;
    }
    IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    void CastSpell()
    {
        if (Input.GetButtonDown("CastSpell") && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }
        
    }

    IEnumerator CastCoroutine()
    {
        anim.SetBool("Cast", true);
        yield return new WaitForSeconds(0.15f);

        //side cast
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            //flip fireball
            if (pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero; // if facing right, fireball continues as per normal
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180);
                //if not facing right, rotate the fireball 180 deg
            }
            pState.recoilingX = true;
        }      
               

        Mana -= manaSpellCost;
        yield return new WaitForSeconds(0.35f);
        anim.SetBool("Cast", false);
        pState.casting = false;
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.eulerAngles = new Vector2(0, 180);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.eulerAngles = new Vector2(0, 0);
            pState.lookingRight = true;
        }

    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walk", rb.velocity.x != 0 && Grounded());
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    void Jump()
    {

        if (!pState.jumping && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);

            pState.jumping = true;
        }

        if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButton("Jump"))
        {
            pState.jumping = true;

            airJumpCounter++;

            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            pState.jumping = false;
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxFallingSpeed, rb.velocity.y));

        anim.SetBool("Jump", !Grounded());
    }



    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
    
    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;        
        if (attack && timeSinceAttack >= timeBetweenAttacks)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attack");            

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {

                int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;

                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);

            }
            else if (yAxis > 0)
            {                
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);                
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);                
            }
            
        }

    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilBool, Vector2 _recoilDir, float _recoilStrength)
    {
                
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer); 
        List<Enemy> hitEnemies = new List<Enemy>();


        // CHECK FOR HITS
          
        if (objectsToHit.Length > 0 )
        {
            _recoilBool = true;

        }
                

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if(e && !hitEnemies.Contains(e))
            {
                e.EnemyGetsHit(damage, _recoilDir, _recoilStrength);
                hitEnemies.Add(e);

                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Debug.Log("Mana gain called");

                    Mana += manaGain;
                }
            }
        }
    }


    void Recoil()
    {
        if (pState.recoilingX)
        {
            if(pState.lookingRight)
            {
                rb.velocity = new Vector2 (-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {                
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }

            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if(pState.recoilingX && stepsXrecoiled < recoilXSteps)
        {
            stepsXrecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && stepsYrecoiled < recoilYSteps)
        {
            stepsYrecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

     void StopRecoilX()
    {
        stepsXrecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYrecoiled = 0; 
        pState.recoilingY = false;
    }


    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate (_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }


    IEnumerator Dash()
    {
        canDash = false; 
        pState.dashing = true;
        anim.SetTrigger("Dash");
        rb.gravityScale = 0;
        int _dir = pState.lookingRight ? 1 : -1;
        rb.velocity = new Vector2(_dir * dashSpeed, 0);
        if (Grounded() )
        {
            Instantiate(dashEffect, transform); //adding dash effect if grounded
        }
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void ToggleInventory()
    {
        if (openInventory)
        {
            UIManager.Instance.inventory.SetActive(true);
        }
        else
        {
            UIManager.Instance.inventory.SetActive(false);
        }
    }


    IEnumerator Death()
    {
        pState.alive = false;
        Time.timeScale = 1f;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);

        anim.SetTrigger("Death");
        Color flashColor = Color.white;
        flashColor.a = Mathf.PingPong(Time.time * 4, 2.0f);
        flashColor = Color.white;
        

        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(0.9f);

        pState.invincible = false;
        pState.cutscene = false;
        StartCoroutine(UIManager.Instance.ActivateDeathScreen());
    }


    public void Respawned()
    {
        if (!pState.alive)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            GetComponent<BoxCollider2D>().enabled = true;
            pState.alive = true;
            Health = maxHealth;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            anim.Play("Plyaer_Idle");
        }
    }

    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        pState.invincible = true;        

        //If exit direction is upwards
        if (_exitDir.y > 0)
        {
            rb.velocity = jumpForce * _exitDir;
        }

        //If exit direction requires horizontal movement
        if (_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;
            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.invincible = false;
        pState.cutscene = false;
        StartCoroutine(UIManager.Instance.ActivateDeathScreen());
    }

}
