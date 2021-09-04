using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    MainTarget = 0,
    ArcheryBarrack = 1,
    WarriorBarrack = 2,
    Fountain = 3
}
public class BuildingRTS : MonoBehaviour
{
    [SerializeField] private BuildingType type;
    [SerializeField] private UnitType unitType;
    [SerializeField] private GameObject selectedGameObject;
    [SerializeField] private bool boxColliderEnabled;

    private ViewController viewController;
    private GameController gameController;
    private Model model;
    private Health destructionState;

    int pending;
    bool pendingState;
    float nextUnitRemainingTime;
    Queue<UnitType> queue;

    float sizeZ;

    public int Pending { get => pending; 
        set
        {
            pending = value;
            viewController.UpdatePendingText(this, queue.Count);
        }
    }
    public UnitType UnitType { get => unitType; set => unitType = value; }
    public string NameRTS { get => type.ToString(); }
    public BuildingType Type { get => type; set => type = value; }

    private void Start()
    {
        viewController = FindObjectOfType<ViewController>();
        gameController = FindObjectOfType<GameController>();
        model = FindObjectOfType<Model>();

        destructionState = GetComponent<Health>();

        queue = new Queue<UnitType>();

        sizeZ = GetComponent<BoxCollider>().bounds.size.z;
        GetComponent<BoxCollider>().enabled = boxColliderEnabled;
    }
    private void Update()
    {
        if (pendingState)
        {
            nextUnitRemainingTime -= Time.deltaTime;

            if(nextUnitRemainingTime < 0)
            {
                InstantiateUnit(queue.Dequeue());

                if(queue.Count > 0)
                {
                    nextUnitRemainingTime = GameConfigurations.UnitCreationDelay;
                }
                else
                {
                    pendingState = false;
                }
            }
        }
    }

    public void SetSelectedVisible(bool visible)
    {
        selectedGameObject.SetActive(visible);
    }

    public void CreateUnit()
    {
        Pending++;

        if(!pendingState)
        {
            nextUnitRemainingTime = GameConfigurations.UnitCreationDelay;

            queue.Enqueue(unitType);
            pendingState = true;
        }
        else
        {
            queue.Enqueue(unitType);
        }
    }
    public void InstantiateUnit(UnitType unitType)
    {

        Vector3 position = transform.position + sizeZ * transform.forward;
        gameController.SpawnDefender(unitType, position);
        Pending--;
        //
        //old
        //GameObject unitRTS = Resources.Load<GameObject>("DefenderUnit");
        //unitRTS.GetComponent<UnitRTS>().Setup(unitType, model.GetCurrentUnitLevelDamage(unitType), model.GetCurrentUnitLevel(unitType),  GameConfigurations.DefenderTeamIndex);

        //Vector3 position = transform.position + sizeZ * transform.forward;
        //Instantiate(unitRTS, position, Quaternion.identity, null);

        //Pending--;
    }
}
