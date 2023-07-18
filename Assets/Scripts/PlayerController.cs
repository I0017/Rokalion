using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private GameObject cliffsFootstepsSFX;
    [SerializeField] private GameObject mossFootstepsSFX;
    [SerializeField] private GameObject healingSFX;
    [SerializeField] private AudioSource jumpSFX;
    [SerializeField] private AudioSource hurtSFX;
    [SerializeField] private AudioSource attackSFX;
    [SerializeField] private AudioSource rockEnemyHitSFX;
    [Space(7)]

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] public int maxAirJumps;
    [SerializeField] private int jumpBufferFrames;
    [SerializeField] private float coyoteTime;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY;
    [SerializeField] private float groundCheckX;
    [SerializeField] private LayerMask whatIsGround;
    [Space(7)]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [Space(7)]

    [Header("Attack Settings")]
    [SerializeField] Transform SideAttackT;
    [SerializeField] Transform UpAttackT;
    [SerializeField] Transform DownAttackT;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] Vector2 UpAttackArea;
    [SerializeField] Vector2 DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] public float attackStrength;
    [Space(7)]

    [Header("Knockback Settings")]
    [SerializeField] float kbForce;
    [SerializeField] public float kbCounter;
    [SerializeField] public float kbTotalTime;
    public bool kbFromRight;
    [Space(7)]

    [Header("Health Settings")]
    [SerializeField] GameObject blood;
    [SerializeField] float hitFlashSpeed;
    public int health;
    public int maxHealth;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    private float healTimer;
    [SerializeField] float timeToHeal;
    [Space(7)]

    [Header("Mana Settings")]
    [SerializeField] Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [Space(7)]

    [SerializeField] public GameObject respawnPoint;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject slashEffect;

    private float xAxis, yAxis;
    private float gravity;
    private int jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    private int airJumpsCounter = 0;
    private bool canDash = true;
    private bool canJump = true;
    private bool canAttack = true;
    private bool dashed = false;

    private bool attack = false;
    private bool inspect = false;

    private float timeBetweenAttack;
    private float timeSinceAttack;

    private bool restoreTime;
    private float restoreTimeSpeed;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    [HideInInspector] public PlayerStateList pState;
    Animator animate;

    public static PlayerController Instance;

    public bool Inspect
    {
        get
        {
            return inspect;
        }
    }
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
        sr = GetComponent<SpriteRenderer>();
        animate = GetComponent<Animator>();
        StopFootsteps();
        gravity = rb.gravityScale;
        Mana = mana;
        manaStorage.fillAmount = Mana;
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
        RestoreTimeScale();
        if (pState.dashing)
        {
            return;
        }
        KnockBack();
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
        Flash();
        Heal();
        End();
    }
    private void FixedUpdate()
    {
        if (pState.dashing)
        {
            return;
        }
    }
    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        inspect = Input.GetButtonDown("Inspect");
    }
    private void Move()
    {
        if (canMove() && Grounded())
        {
            Footsteps();
        }
        else
        {
            StopFootsteps();
        }
        if (canMove())
        {
            rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
            animate.SetBool("Walking", rb.velocity.x != 0 && true);
            pState.walking = true;
        }
        if (xAxis == 0)
        {
            pState.walking = false;
            StopFootsteps();
        }
    }
    private void Jump()
    {
        if (pState.jumping && canMove() && canJump)
        {
            jumpSFX.Play();
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            pState.jumping = false;
        }
        if (!pState.jumping)
        {
            if (canJump && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if (canJump && !Grounded() && airJumpsCounter < maxAirJumps && Input.GetButtonDown("Jump") && !dashed)
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
        if (canAttack && attack && timeSinceAttack >= timeBetweenAttack && !dashed)
        {
            timeSinceAttack = 0;
            animate.SetTrigger("Attacking");
            attackSFX.Play();
            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackT, SideAttackArea, ref pState.recoilingX, 1);
                Instantiate(slashEffect, SideAttackT);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackT, UpAttackArea, ref pState.recoilingY, 1);
                SlashEffectAngle(slashEffect, 90, UpAttackT);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackT, DownAttackArea, ref pState.recoilingY, 1);
                SlashEffectAngle(slashEffect, -90, DownAttackT);
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
                objectsToHit[i].GetComponent<Enemy>().EnemyHit
                    (attackStrength, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    rockEnemyHitSFX.Play();
                    Mana += manaGain;
                }
            }
        }
    }
    void SlashEffectAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
    public void TakeDamage(float _damage)
    {
        hurtSFX.Play();
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
        if (Health == 0)
        {
            Respawn();
        }
    }
    IEnumerator StopTakingDamage()
    {
        pState.invicible = true;
        GameObject _bloodParticles = Instantiate(blood, transform.position, Quaternion.identity);
        Destroy(_bloodParticles, 1.5f);
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
                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }
    void Heal()
    {
        if (Input.GetButton("Cast") && Health < maxHealth && Mana > 0 && !pState.dashing && !pState.jumping && !pState.walking)
        {
            healingSFX.gameObject.SetActive(true);
            pState.healing = true;
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health += 1;
                healTimer = 0;
            }
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            healingSFX.gameObject.SetActive(false);
            pState.healing = false;
            healTimer = 0;
        }
    }
    private void Respawn()
    {
        Fade.Instance.fadeNeeded = true;
        transform.position = respawnPoint.transform.position;
        Mana = 0;
        Health = 5;
    }
    float Mana
    {
        get
        {
            return mana;
        }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }
    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }
    void Flash()
    {
        sr.material.color = pState.invicible ? Color.Lerp(Color.white, Color.black,
            Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }
    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        Fade.Instance.fadeNeeded = true;
        if (_exitDir.y > 0)
        {
            rb.velocity = jumpForce * _exitDir;
        }
        if (_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;
            Move();
        }
        Flip();
        yield return new WaitForSeconds(_delay);
        pState.cutscene = false;
    }
    void KnockBack()
    {
        if (kbCounter > 0)
        {
            if (kbFromRight)
            {
                rb.velocity = new Vector2(-kbForce, kbForce);
            }
            else
            {
                rb.velocity = new Vector2(kbForce, kbForce);
            }
            kbCounter -= Time.deltaTime;
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
    public bool canMove()
    {
        if (!pState.inDialogue && !pState.healing && !pState.end)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void Footsteps()
    {
        if (pState.isInCliffs)
        {
            cliffsFootstepsSFX.SetActive(true);
        }
        if (pState.isInMoss)
        {
            mossFootstepsSFX.SetActive(true);
        }

    }
    void StopFootsteps()
    {
        cliffsFootstepsSFX.SetActive(false);
        mossFootstepsSFX.SetActive(false);
    }
    void End()
    {
        if (pState.end)
        {
            canDash = false;
            canJump = false;
            canAttack = false;
            HUD.SetActive(false);
        }
    }
}
