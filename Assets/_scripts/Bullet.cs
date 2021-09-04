using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float maxLifeTime = 2f;
    
    private string shooterTag;

    private void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Попадание в другую стрелу
        if (other.GetComponent<Bullet>() != null)
        {
            Destroy(gameObject);
            return;
        }
        
        //Если unit попал по своему unit - пролетать мимо
        UnitRTS targetUnitRTS = other.GetComponent<UnitRTS>();
        if (targetUnitRTS != null && targetUnitRTS.CompareTag(shooterTag))
            return;

        //old
        //if (targetUnitRTS != null && targetUnitRTS.TeamIndex == shooterTeamIndex)
        //    return;

        //Если unit-защитник попал по зданию
        if (targetUnitRTS == null && shooterTag == "Defender")
        {
            Destroy(gameObject);
            return;
        }

        //old
        //if(targetUnitRTS == null && shooterTeamIndex == GameConfigurations.DefenderTeamIndex)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        Health health = other.GetComponent<Health>();
        if(health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    public void Setup(string shooter, float damage)
    {
        shooterTag = shooter;
        this.damage = damage;
    }
}
