using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{
    [SerializeField] float  m_minDistanceX = 4.0f;
    [SerializeField] float  m_minDistanceY = 4.0f;
    [SerializeField] float  m_speed = 4.0f;
    [SerializeField] float  m_hitPoint = 100.0f;
    [SerializeField] float  m_currentHitPoint;
    public float            damage = 2.0f;

    private Transform       m_player;
    private Rigidbody2D     m_body2d;
    private Animator        m_animator;
    private Vector3         m_playerDistance;
    private bool            m_facingRight = true;
    private bool            m_dead = false;
    private bool            m_hited = false;
    private WaitForSeconds  m_hitSeconds = new WaitForSeconds(0.5f);

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
        if(!m_hited && !m_dead)
        {
            m_playerDistance = m_player.transform.position - transform.position;
            if(Mathf.Abs(m_playerDistance.x) < m_minDistanceX && Mathf.Abs(m_playerDistance.y) < m_minDistanceY)
            {
                m_body2d.velocity = new Vector2(m_speed * (m_playerDistance.x / Mathf.Abs(m_playerDistance.x)), m_body2d.velocity.y);
            }

            m_animator.SetFloat("Speed", Mathf.Abs(m_body2d.velocity.x));

            if(m_body2d.velocity.x > 0 && !m_facingRight)
            {
                Flip();
            }
            else if(m_body2d.velocity.x < 0 && m_facingRight)
            {
                Flip();
            }
        }
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
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
