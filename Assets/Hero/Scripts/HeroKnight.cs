/*
* Links utilizados como apoio:
* https://assetstore.unity.com/packages/2d/characters/hero-knight-pixel-art-165188 - código demo do asset utilizado como base
* https://docs.unity3d.com/Manual/index.html
* https://www.youtube.com/watch?v=zdoIsdXNHKc
* https://www.youtube.com/watch?v=NqPYWkmm09E&list=PLgTmU6kuSLtzI4b28f-y4XUV52j7EFx0w
* https://www.youtube.com/watch?v=uxsajSuJP0w&list=PLgTmU6kuSLtzI4b28f-y4XUV52j7EFx0w
*/
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroKnight : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 5.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;

    [SerializeField] float      m_maxHitPoint = 100.0f;
    [SerializeField] float      m_currentHitPoint;

    [SerializeField] int        m_hitForce = 5;
    [SerializeField] Transform  m_attackCheck;
    [SerializeField] float      m_radiusAttack;
    [SerializeField] LayerMask  m_enemyLayer;
    [SerializeField] float      m_attackForce = 10.0f;
    [SerializeField] Vector3    m_offset = Vector2.zero;
    [SerializeField] float      m_radius = 10.0f;
    [SerializeField] LayerMask  m_groundLayer;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private bool                m_grounded = false;
    private bool                m_extraJump = false;
    private bool                m_rolling = false;
    private bool                m_dead = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private WaitForSeconds      m_hitSeconds = new WaitForSeconds(0.3f);
    private WaitForSeconds      m_deathSeconds = new WaitForSeconds(2f);
    private float               m_timeNextAttack;
    private bool                m_isBlocking = false;

    private Slider hitBar = null;


    public float CurrentHitPoint
    {
        get => m_currentHitPoint;
        set
        {
            m_currentHitPoint = value;
            if (m_currentHitPoint <= 0.0f)
            {
                m_currentHitPoint = 0;
                m_dead = true;
                StartCoroutine(OnDeath());
            }
        }
    }

    public float HitPoint
    {
        get => m_maxHitPoint;
    }

    public bool Dead {
        get => m_dead;
    }

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        CurrentHitPoint = m_maxHitPoint;

        hitBar = GameObject.Find("LifeBar").GetComponent<Slider>();
        if (hitBar != null)
            hitBar.value = m_currentHitPoint / m_maxHitPoint;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        //Check if character just landed on the ground
        m_grounded = Physics2D.OverlapCircle(this.transform.position + m_offset, m_radius, m_groundLayer);
        m_animator.SetBool("Grounded", m_grounded);

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0 && !m_dead)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
            m_attackCheck.localPosition = new Vector2(m_attackCheck.localPosition.x > 0 ? m_attackCheck.localPosition.x : -m_attackCheck.localPosition.x, m_attackCheck.localPosition.y);
        }

        else if (inputX < 0 && !m_dead)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
            m_attackCheck.localPosition = new Vector2(m_attackCheck.localPosition.x < 0 ? m_attackCheck.localPosition.x : -m_attackCheck.localPosition.x, m_attackCheck.localPosition.y);
        }

        // Move
        if (!m_rolling && !m_dead && !m_isBlocking)
        {
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --

        //Attack
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown("j")) && m_timeSinceAttack > 0.25f && !m_rolling && !m_dead)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown("k")) && !m_rolling && !m_dead)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            m_isBlocking = true;
        }

        else if (Input.GetMouseButtonUp(1) || Input.GetKeyUp("k") && !m_dead)
        {
            m_animator.SetBool("IdleBlock", false);
            m_isBlocking = false;
        }

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_dead)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }


        //Jump
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling && !m_dead)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_extraJump = true;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        }
        else if (Input.GetKeyDown("space") && m_extraJump && !m_rolling && !m_dead)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_extraJump = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon && !m_dead)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    // Animation Events
    // Called in end of roll animation.
    void AE_ResetRoll()
    {
        m_rolling = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AiController aiController = collision.gameObject.GetComponent<AiController>();

        if (aiController)
        {
            Vector2 direction = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            TakeDamage(direction, aiController);
        }        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Pikup"))
        {
            CurrentHitPoint += 25;
            if (m_currentHitPoint > m_maxHitPoint)
                m_currentHitPoint = m_maxHitPoint;
            if (hitBar != null)
                hitBar.value = m_currentHitPoint / m_maxHitPoint;

            Destroy(collision.gameObject);
        }
    }



    public void TakeDamage(Vector2 direction, AiController aiController)
    {
        if (!m_isBlocking)
        {
            m_animator.SetTrigger("Hurt");           
            CurrentHitPoint -= aiController.damage;
            if (hitBar != null)
                hitBar.value = m_currentHitPoint / m_maxHitPoint;
            m_rolling = true;
            m_body2d.AddForce(direction * m_hitForce, ForceMode2D.Impulse);
            StartCoroutine(OnHitWaiting());
        }
    }

    private IEnumerator OnHitWaiting()
    {
        yield return m_hitSeconds;
        m_rolling = false;
    }

    private IEnumerator OnDeath()
    {
        if (hitBar != null)
            hitBar.value = 0.0f;
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");

        yield return m_deathSeconds;
        m_dead = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void PlayerAttack()
    {
        Collider2D[] enemiesAttacked = Physics2D.OverlapCircleAll(m_attackCheck.position, m_radiusAttack, m_enemyLayer);
        for(int i = 0; i < enemiesAttacked.Length; i++)
        {
            AiController enemy = enemiesAttacked[i].GetComponent<AiController>();
            enemy.TakeDamage(m_attackForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_attackCheck.position, m_radiusAttack);
        Gizmos.DrawWireSphere((this.transform.position + m_offset), m_radius);
    }
}
