using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5;

    [SerializeField] private float jumpForce = 10;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private int jumpBufferFrames = 60;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlatform;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    [SerializeField] Transform SideAttackT;
    [SerializeField] Transform UpAttackT;
    [SerializeField] Transform DownAttackT;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] Vector2 UpAttackArea;
    [SerializeField] Vector2 DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float attackStrength;

    [SerializeField] int recoilXSteps = 3;
    [SerializeField] int recoilYSteps = 3;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;

    public int health;
    public int maxHealth;

    private float xAxis, yAxis;
    private float gravity;
    private int jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    private int airJumpsCounter = 0;
    private bool canDash = true;
    private bool dashed = false;

    private bool attack = false;
    private float timeBetweenAttack;
    private float timeSinceAttack;

    private int stepsXRecoiled, stepsYRecoiled;

    private Vector2 velocity;
    private Rigidbody2D rb;
    [HideInInspector] public PlayerStateList pState;
    Animator animate;

    public static PlayerController Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Health = maxHealth;
    }
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        animate = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(SideAttackT.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackT.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackT.position, DownAttackArea);
    }
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        if (pState.dashing)
        {
            return;
        }
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
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
    }
    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        animate.SetBool("Walking", rb.velocity.x != 0 && true);
    }
    private void Jump()
    {
        if (pState.jumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            pState.jumping = false;
        }
        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if (!Grounded() && airJumpsCounter < maxAirJumps && Input.GetButtonDown("Jump") && !dashed)
            {
                airJumpsCounter += 1;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
        }
        animate.SetBool("Jumping", !Grounded());
    }
    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }
    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpsCounter = 0;
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
            jumpBufferCounter -= 1;
        }
    }
    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (Input.GetButtonDown("Dash") && canDash && !dashed && !Grounded())
        {
            StartCoroutine(Dash());
            dashed = true;
            canDash = false;
        }
        if (Grounded())
        {
            dashed = false;
            canDash = true;
        }
    }
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack && !dashed)
        {
            timeSinceAttack = 0;
            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackT, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackT, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackT, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
        }
    }
    private void Hit(Transform _attackT, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackT.position, _attackArea, 0, attackableLayer);
        if (objectsToHit.Length > 0 )
        {
            _recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().beingAttacked = true;
                objectsToHit[i].GetComponent<Enemy>().EnemyHit
                    (attackStrength, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
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
            airJumpsCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled += 1;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled += 1;
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
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        pState.invicible = true;
        yield return new WaitForSeconds(1f);
        pState.invicible = false;
    }
    public int Health
    {
        get
        {
            return health;
        }
        private set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
            }
        }
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
}
