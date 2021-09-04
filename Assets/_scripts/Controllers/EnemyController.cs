using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Transform mainTarget;
    UnitRTS unitRTS;

    readonly float chechkingEnemyDelay = .2f;
    float lastEnemyCheckedAt;

    private void Awake()
    {
        mainTarget = GameObject.FindGameObjectWithTag("MainTarget").transform;
        unitRTS = GetComponent<UnitRTS>();
    }
    private void Start()
    {
        lastEnemyCheckedAt = Time.time;
    }
    private void Update()
    {
        if (unitRTS.State == UnitState.Idle)
            unitRTS.Attack(mainTarget);

        if (Time.time - lastEnemyCheckedAt < chechkingEnemyDelay)
            return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, GameConfigurations.EnemyDetectRadius);
        foreach (var c in colliders)
            if (c.CompareTag("Defender"))
            {
                OnEnemyDetected(c.transform);
                break;
            }
                
        lastEnemyCheckedAt = Time.time;
    }

    public void OnEnemyDetected(Transform enemy)
    {
        if (unitRTS.CurrentTarget == mainTarget)
        {
            unitRTS.Attack(enemy);
        }
            
    }
}
