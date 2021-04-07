using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{
    [SerializeField] float      m_minDistanceX = 4.0f;
    [SerializeField] float      m_minDistanceY = 4.0f;
    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_hitPoint = 100.0f;
    [SerializeField] float      m_currentHitPoint;
    [SerializeField] Transform  m_attackCheck;
    [SerializeField] float      m_radiusAttack;
    [SerializeField] LayerMask  m_playerLayer;
    public float                damage = 2.0f;

    private Transform           m_player;
    private Rigidbody2D         m_body2d;
    private Animator            m_animator;
    private Vector3             m_playerDistance;
    private bool                m_facingRight = true;
    private bool                m_dead = false;
    private bool                m_hited = false;
    private bool                m_attacking = false;
    private WaitForSeconds      m_hitSeconds = new WaitForSeconds(0.5f);

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
                m_animator.SetTrigger("Death");
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player").transform;
        m_body2d = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        CurrentHitPoint = m_hitPoint;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!m_hited && !m_dead && !m_attacking)
        {
            m_playerDistance = m_player.transform.position - transform.position;
            if(Mathf.Abs(m_playerDistance.x) < m_minDistanceX && Mathf.Abs(m_playerDistance.y) < m_minDistanceY)
            {
                m_body2d.velocity = new Vector2(m_speed * (m_playerDistance.x / Mathf.Abs(m_playerDistance.x)), m_body2d.velocity.y);
            }


            if(m_body2d.velocity.x > 0 && !m_facingRight && !m_attacking)
            {
                Flip();
            }
            else if(m_body2d.velocity.x < 0 && m_facingRight && !m_attacking)
            {
                Flip();
            }
        }

        m_animator.SetFloat("Speed", Mathf.Abs(m_body2d.velocity.x));
    }

    private void Flip()
    {
        m_facingRight = !m_facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(float damage)
    {
        CurrentHitPoint -= damage;
        m_hited = true;
        m_animator.SetTrigger("Hit");
        StartCoroutine(OnDamageWaiting());
    }

    private IEnumerator OnDamageWaiting()
    {
        yield return m_hitSeconds;
        m_hited = false;
        ContinuousAttacking();
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void EnemyAttack()
    {
        Collider2D playerAttacked = Physics2D.OverlapCircle(m_attackCheck.position, m_radiusAttack, m_playerLayer);

        if (playerAttacked && !m_dead)
        {
            HeroKnight player = playerAttacked.GetComponent<HeroKnight>();

            Vector2 direction = this.gameObject.transform.position.normalized;

            player.TakeDamage(direction, this);
        }
    }

    private void ContinuousAttacking()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(m_attackCheck.position, m_radiusAttack, m_playerLayer);

        if (playerInRange == null || m_dead)
        {
            m_attacking = false;
        }
        else
        {
            m_animator.SetTrigger("Attack");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            m_animator.SetTrigger("Attack");
            m_attacking = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_attackCheck.position, m_radiusAttack);
    }
}
