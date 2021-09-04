using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitRTS : UnitBase
{
    [SerializeField] private float rechargeTime = 1.5f;
    [SerializeField] private Rigidbody bullet;
    [SerializeField] private Transform fireTransform;
    [SerializeField] private float arrowLaunchForce;

    [SerializeField] private UnitState state;
    [SerializeField] private UnitType type;
    [SerializeField] private int teamIndex;

    private Transform currentTarget;
    private float minAttackDistance;
    private Vector3 targetBoofPosition;

    private Health health;
    private GameObject selectedGameObject;

    private int level;
    private int damage;

    bool recharging = false;
    float recharginRemainingTime;
    Coroutine attackCoroutine;

    public string NameRTS { get => type.ToString(); }
    public int TeamIndex { get => teamIndex; set => teamIndex = value; }
    internal UnitState State { get => state; set => state = value; }
    private Vector3 TargetPosBoof
    {
        get 
        {
            return targetBoofPosition;
        }
        set
        {
            targetBoofPosition = value;
        }
    }
    public Transform CurrentTarget { get => currentTarget; set => currentTarget = value; }
    public int Level { get => level; set => level = value; }
    public UnitType UnitType { get => type; set => type = value; }

    private void Awake()
    {
        SetAgent(GetComponent<NavMeshAgent>());

        selectedGameObject = transform.Find("Selected").gameObject;
        SetSelectedVisible(false);
    }
    private void Start()
    {
        minAttackDistance = GameConfigurations.GetMinAttackDistance(type);
    }
    private void Update()
    {
        switch (state)
        {
            case UnitState.Idle:
                return;

            case UnitState.Moving:
                if (CheckDestinationReached())
                {
                    Debug.Log("Destination reached. Changing state to idle");
                    StateIdle();
                }
                    
                break;

            case UnitState.Seek:
                if (CheckTargetPositionUpdated())
                    UpdateTargetPosition();

                if (currentTarget == null) //target destroyed
                    StateIdle();

                if (CheckDistanceToTarget(minAttackDistance))
                    StateAttack();
    
                break;

            case UnitState.Attack:
                if (currentTarget == null)
                {
                    Transform nearest = TryGetNearestTarget();
                    if (nearest != null)
                        Attack(nearest);
                    else
                        StateIdle();
                }

                else if (!CheckDistanceToTarget(minAttackDistance))
                    StateSeek();

                break;

            default:
                break;
        }

        if (recharging)
        {
            recharginRemainingTime -= Time.deltaTime;
            if (recharginRemainingTime < 0)
                recharging = false;
        }
    }


    
    public void Setup(UnitType type, int damage, int level, int teamIndex)
    {
        this.type = type;
        this.teamIndex = teamIndex;

        this.level = level;
        this.damage = damage;

        StateIdle();
    }
    public override void MoveTo(Vector3 targetPos)
    {
        base.MoveTo(targetPos);
        StateMoving();
    }
    //public void OnEnemyDetected(Transform enemy)
    //{
    //    Debug.Log($"OnEnemyDetected. Unit: {name}");
    //    if (state == UnitState.Idle)
    //    {
    //        Attack(enemy);
    //        Debug.Log("Attacking enemy: " + enemy.name);
    //    }
    //}

    //FCM
    private void StateIdle()
    {
        Debug.Log($"State idle of unit: {name}");

        if(gameObject.activeInHierarchy)
            StopMoving();

        //HideSword();
        currentTarget = null;

        state = UnitState.Idle;
    }
    private void StateMoving()
    {
        Debug.Log($"State moving of unit: {name}");

        //HideSword();
        currentTarget = null;

        state = UnitState.Moving;
    }
    private void StateAttack()
    {
        Debug.Log($"StateAttack of unit: {name}");

        StopMoving();
        transform.LookAt(currentTarget);

        if (state == UnitState.Attack) //already attacking
            return;
        else
            state = UnitState.Attack;

        if (type == UnitType.Warrior)
            Hit();
        else if (type == UnitType.Archer)
            Fire();

       
    }
    private void StateSeek()
    {
        Debug.Log($"StateSeek of unit: {name}");

        state = UnitState.Seek;
        UpdateTargetPosition();
    }


    public void SetSelectedVisible(bool visible)
    {
        selectedGameObject.SetActive(visible);
    }
    public void Attack(Transform target)
    {
        if (target == null || target == currentTarget)
            return;
        
        currentTarget = target;
        TargetPosBoof = target.position;

        if (CheckDistanceToTarget(minAttackDistance))
            StateAttack();
        else
            StateSeek();
    }
    private void UpdateTargetPosition()
    {
        base.MoveTo(currentTarget.position);
        TargetPosBoof = currentTarget.position;
    }
    private void Hit()
    {
        if (attackCoroutine == null)
            attackCoroutine = StartCoroutine(HitCoroutine());

        StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(HitCoroutine());
    }
    private IEnumerator HitCoroutine()
    {
        while (state == UnitState.Attack)
        {
            yield return new WaitUntil(() => recharging == false);

            if (currentTarget != null)
            {
                HitOnTarget(currentTarget.position);
                recharginRemainingTime = rechargeTime;
                recharging = true;
            }
        }
    }
    private void HitOnTarget(Vector3 targetPosition)
    {
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
        
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, (float) GameConfigurations.WarriorAttackDistance * 10);
        //Debug.DrawRay(transform.position, transform.forward, Color.red, 10f);
        
        //#refactor
        //getting only unit-enemies or building for enemies with tag "Enemy"
        var enemies = hits.Where(h => (h.transform.GetComponent<UnitRTS>() != null && !h.transform.CompareTag(tag))  
            || (h.transform.GetComponent<BuildingRTS>() != null && transform.CompareTag("Enemy")));

        if (enemies.Count() > 0)
        {
            if(tag == "Enemy")
                Debug.Log("Hit on: " + enemies.First().transform.name);
            enemies.First().transform.GetComponent<Health>().TakeDamage(damage);
            StartCoroutine(ShowSword());
        }


        //old
        //Collider[] colliders = Physics.OverlapSphere(fireTransform.position, GameConfigurations.WarriorAttackDistance);
        //var enemies = colliders.Where(c => (c.GetComponent<UnitRTS>() != null && !c.CompareTag(tag)) || c.GetComponent<BuildingRTS>() != null);
        //if (enemies.Count() > 0)
        //{
        //    enemies.First().GetComponent<Health>().TakeDamage(damage);
        //    StartCoroutine(ShowSword());
        //}            
    }
    private IEnumerator ShowSword()
    {
        GameObject sword = fireTransform.Find("Sword").gameObject;
        sword.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        sword.SetActive(false);
    }
    //private void HideSword()
    //{
    //    Debug.Log("HideSword()");
    //    fireTransform.Find("Sword").gameObject.SetActive(false);
    //}
    private void Fire() {
        if(attackCoroutine == null)
            attackCoroutine = StartCoroutine(FireCoroutine());

        StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(FireCoroutine());
    }
    private IEnumerator FireCoroutine()
    {
        while (state == UnitState.Attack)
        {
            yield return new WaitUntil(() => recharging == false);
            
            if(currentTarget != null)
            {
                FireOnTarget(currentTarget.position);
                recharginRemainingTime = rechargeTime;
                recharging = true;
            }

            //yield return new WaitForSeconds(rechargeTime);
            //recharging = false;
        }
    }
    private void FireOnTarget(Vector3 targetPos)
    {
        transform.LookAt(targetPos);

        Rigidbody bulletInstance = Instantiate(bullet, fireTransform.position, fireTransform.rotation) as Rigidbody;
        bulletInstance.velocity = arrowLaunchForce * fireTransform.forward;
        bulletInstance.GetComponent<Bullet>().Setup(transform.tag, damage); //edit this
    }
    private Transform TryGetNearestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(TargetPosBoof, 15f);
        var enemies = colliders.Where(c => c.GetComponent<UnitRTS>() != null && c.transform.tag != transform.tag).ToList();

        if (enemies.Count > 0)
            return enemies[0].transform;
        else
            return null;
    }
    
    private bool CheckTargetPositionUpdated()
    {
        return currentTarget != null && Vector3.Distance(GetDestination(), currentTarget.transform.position) > 1f;
    }
    public float Vector3DistanceTemp;
    public float DistanceTemp;
    private bool CheckDistanceToTarget(float distance)
    {
        if (currentTarget == null)
            return false;

        distance += (currentTarget.GetComponent<Collider>().bounds.size.magnitude / 2);
        DistanceTemp = distance;
        Vector3DistanceTemp = Vector3.Distance(transform.position, currentTarget.position);
        return Vector3.Distance(transform.position, currentTarget.position) < distance;

        //return Mathf.Abs((transform.position - currentTarget.position).magnitude) < distance;
    }
    private bool CheckDestinationReached()
    {
        Debug.LogWarning("Distance = " + Vector3.Distance(transform.position, GetDestination()));
        return Vector3.Distance(transform.position, GetDestination()) < 1f + GetStoppingDistance();
    }
}
