using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class HeroKnight : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 5.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] float      m_hitPoint = 100.0f;
    [SerializeField] int        m_hitForce = 5;
    [SerializeField] float      m_currentHitPoint;
    [SerializeField] Transform  m_attackCheck;
    [SerializeField] float      m_radiusAttack;


    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
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
        get => m_hitPoint;
    }

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        CurrentHitPoint = m_hitPoint;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

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
        if (!m_rolling && !m_dead)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

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
        }

        else if (Input.GetMouseButtonUp(1) || Input.GetKeyUp("k") && !m_dead)
            m_animator.SetBool("IdleBlock", false);

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
            m_groundSensor.Disable(0.2f);
        }
        else if (Input.GetKeyDown("space") && m_extraJump && !m_rolling && !m_dead)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_extraJump = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
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
            m_animator.SetTrigger("Hurt");
            CurrentHitPoint -= aiController.damage;
            Vector2 direction = collision.gameObject.transform.position.normalized;
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
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");

        yield return m_deathSeconds;
        m_dead = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_attackCheck.position, m_radiusAttack);
    }
}
