using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathArea : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HeroKnight hero = collision.gameObject.GetComponent<HeroKnight>();

            if (!hero.Dead)
            {
                hero.CurrentHitPoint -= hero.HitPoint;
            }

        }
    }

}
