using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum UnitState
{
    Idle,
    Moving,
    Seek,
    Attack   
}

public class UnitBase : MonoBehaviour
{
    private NavMeshAgent agent;

    public void SetAgent(NavMeshAgent agent)
    {
        this.agent = agent;
    }
    public virtual void MoveTo(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
        agent.isStopped = false;
    }

    public void StopMoving()
    {
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
    }

    public Vector3 GetDestination()
    {
        return agent.destination;
    }

    public float GetStoppingDistance()
    {
        return agent.stoppingDistance;
    }
}
