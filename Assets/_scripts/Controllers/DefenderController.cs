using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderController : MonoBehaviour
{
    UnitRTS unitRTS;

    readonly float chechkingEnemyDelay = 1f;
    float lastEnemyCheckedAt;

    private void Awake()
    {
        unitRTS = GetComponent<UnitRTS>();
    }
    private void Start()
    {
        lastEnemyCheckedAt = Time.time;
    }
    private void Update()
    {
        if (Time.time - lastEnemyCheckedAt < chechkingEnemyDelay)
            return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, GameConfigurations.EnemyDetectRadius);
        foreach (var c in colliders)
            if (c.CompareTag("Enemy"))
            {
                OnEnemyDetected(c.transform);
                break;
            }

        lastEnemyCheckedAt = Time.time;
    }

    public void OnEnemyDetected(Transform enemy)
    {
        if (unitRTS.State == UnitState.Idle)
        {
            unitRTS.Attack(enemy);
        }
    }
}
