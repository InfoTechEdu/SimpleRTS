using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public enum GameResult
{
    WIN,
    LOSE,
    DRAW
}

public class GameController : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Transform enemiesSpawnPoint;
    [SerializeField] private Transform defenderSpawnPoint;
    [SerializeField] private GameObject defenderPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Scene objects")]
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Transform mainBase;

    [Header("Controllers")]
    [SerializeField] private ViewController viewController;

    private Model model;

    private Vector2 startSelectionPos;
    private Vector2 endSelectionPos;
    bool dragging;

    ObjectPlacer placer;

    private List<UnitRTS> selectedUnitRTSList;
    private List<BuildingRTS> selectedBuildings;

    private List<List<UnitRTS>> allEnemies;

    private float lastSpawnedAt;
    private bool isTimerActive;

    #region MonoBehaviour
    private void Awake()
    {
        selectedUnitRTSList = new List<UnitRTS>();
        selectedBuildings = new List<BuildingRTS>();

        allEnemies = new List<List<UnitRTS>>();
        //allEnemies = new Dictionary<int, List<UnitRTS>>();
    }
    private void Start()
    {
        model = FindObjectOfType<Model>();

        placer = Camera.main.GetComponent<ObjectPlacer>();
    }
    private void Update()
    {
        if (mainBase == null)
        {
            StopTimer();
            OnGameEnd();
        }
        else
        {
            UpdateTimer();
        }

        
        if (placer.Placing)
            return;

        if (IsPointerOverUIObject(out List<RaycastResult> raycastedList) && !dragging)
        {
            if (raycastedList.Where((raycasted) => raycasted.gameObject.tag == "SelectionBox" || raycasted.gameObject.tag == "UnitUI") != null)
                return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            startSelectionPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            float width = Input.mousePosition.x - startSelectionPos.x;
            float height = Input.mousePosition.y - startSelectionPos.y;

            if (width == 0 && height == 0)
            {
                dragging = false;
                selectionBox.gameObject.SetActive(false);
            }
            else
            {
                dragging = true;
                selectionBox.gameObject.SetActive(true);
                selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            }


            dragging = !(width == 0 && height == 0);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            endSelectionPos = startSelectionPos + new Vector2(width, height);
            selectionBox.anchoredPosition = startSelectionPos + new Vector2(width / 2, height / 2);
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);

            DeselectAllUnits();
            DeselectAllBuildings();
            viewController.OnEverythingDeselected();

            if (!dragging)
            {
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
                UpdateSelectedUnits(new Collider[] { hit.collider });
                UpdateSelectedBuildings(new Collider[] { hit.collider });

                if (selectedBuildings.Count > 0)
                {
                    viewController.OnBuildingSelected(selectedBuildings.First());
                }
                else if (selectedUnitRTSList.Count > 0)
                {
                    viewController.OnUnitSelected(selectedUnitRTSList.First());
                }
                return;
            }

            Ray left = Camera.main.ScreenPointToRay(startSelectionPos);
            Ray right = Camera.main.ScreenPointToRay(endSelectionPos);
            if (Physics.Raycast(left) && Physics.Raycast(right))
            {
                Physics.Raycast(Camera.main.ScreenPointToRay(selectionBox.anchoredPosition), out RaycastHit hit);
                Vector3 center = hit.point;

                //RECT TRANSFORM METHOD
                Vector3 radius = CalculateSelectionBoxRadius();
                Collider[] hitColliders = Physics.OverlapBox(center, radius, Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0), LayerMask.GetMask("Selectable"));
                
                UpdateSelectedUnits(hitColliders);
                UpdateSelectedBuildings(hitColliders);

                if(selectedBuildings.Count > 0)
                    viewController.OnBuildingSelected(selectedBuildings.First());
                else if (selectedUnitRTSList.Count > 0)
                    viewController.OnUnitSelected(selectedUnitRTSList.First());
            }

        }

        if (Input.GetMouseButtonDown(1) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
        {
            if (selectedUnitRTSList.Find(unit => unit.transform.tag == "Enemy") != null)
                return;

            if(hitInfo.collider.GetComponent<UnitRTS>() != null)
            {
                List<Transform> nearestEnemies = GetNearestEnemies(hitInfo.transform);
                Debug.Log("Nearest enemies was get. Count - " + nearestEnemies.Count);
                TeamAttack(nearestEnemies);
            }
            else
            {
                Vector3 destination = hitInfo.point;

                List<Vector3> targetPositionList = GetPositionListAround(destination, new float[] { 10f, 20f, 30f }, new int[] { 5, 10, 20 });
                int targetPositionListIndex = 0;
                foreach (UnitRTS unitRTS in selectedUnitRTSList)
                {
                    Debug.Log("Starting moving");
                    unitRTS.MoveTo(targetPositionList[targetPositionListIndex]);
                    targetPositionListIndex = (targetPositionListIndex + 1) % targetPositionList.Count;
                }
            }

            
        }
    }
    #endregion

    public void StartGame()
    {
        SpawnDefenders(1, UnitType.Archer, defenderSpawnPoint.position);
        SpawnDefenders(4, UnitType.Warrior, defenderSpawnPoint.position);

        NextWave();
    }
    
    public void SpawnBuilding(BuildingType bType)
    {
        if (placer.CurrentPlacing != null)
            return;

        string bName = bType.ToString();
        GameObject prefab = Resources.Load<GameObject>(bName);
        GameObject building = Instantiate(prefab);

        placer.SetObject(building);
    }
    public void OnUnitPurchased(BuildingRTS manufacturer)
    {
        manufacturer.CreateUnit();
        model.PurchaseUnit(manufacturer.UnitType);
    }
    public void OnUnitUpgraded(UnitType unitType)
    {
        model.PurchaseUnitUpgrade(unitType);
    }

    private void NextWave()
    {
        WaveData next = model.GetNextWave();
        if(next != null)
        {
            List<UnitRTS> currentWaveEnemies = new List<UnitRTS>();
            currentWaveEnemies.AddRange(SpawnEnemies(next.archers, UnitType.Archer, enemiesSpawnPoint.position));
            currentWaveEnemies.AddRange(SpawnEnemies(next.warriors, UnitType.Warrior, enemiesSpawnPoint.position));
            allEnemies.Add(currentWaveEnemies);

            lastSpawnedAt = Time.time;
            isTimerActive = true;
        }
        else
        {
            StopTimer();
        }
    }
    private void UpdateTimer()
    {
        if (Time.time - lastSpawnedAt > model.EnemySpawnDelay && isTimerActive)
            NextWave();
    }
    private void StopTimer()
    {
        isTimerActive = false;
    }
    private void OnGameEnd()
    {
        if(mainBase != null)
        {
            viewController.ShowGameEndDisplay(GameResult.WIN, model.Cash);
        }
        else
        {
            viewController.ShowGameEndDisplay(GameResult.LOSE, 0);
        }
    }

    private List<UnitRTS> SpawnEnemies(int count, UnitType unitType, Vector3 spawnPosition)
    {
        List<UnitRTS> spawned = new List<UnitRTS>();

        List<Vector3> positions = GetPositionListAround(spawnPosition, 8f, count);
        foreach (var p in positions)
        {
            Vector3 enemyPos = new Vector3(p.x, enemyPrefab.transform.position.y, p.z);
            GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity, null);

            UnitRTS rts = enemy.GetComponent<UnitRTS>();
            int teamIndex = 0;

            rts.Setup(unitType, model.GetCurrentUnitLevelDamage(unitType), model.GetCurrentUnitLevel(unitType), teamIndex);
            rts.GetComponent<Health>().SetOnDeadCallback(CheckWaveCompletion);

            spawned.Add(rts);
        }

        return spawned;
    }
    private List<UnitRTS> SpawnDefenders(int count, UnitType unitType, Vector3 spawnPosition)
    {
        List<UnitRTS> spawned = new List<UnitRTS>();

        List<Vector3> positions = GetPositionListAround(spawnPosition, 8f, count);
        foreach (var p in positions)
        {
            Vector3 enemyPos = new Vector3(p.x, defenderPrefab.transform.position.y, p.z);
            GameObject enemy = Instantiate(defenderPrefab, enemyPos, Quaternion.identity, null);

            UnitRTS rts = enemy.GetComponent<UnitRTS>();
            int teamIndex = GameConfigurations.DefenderTeamIndex;
            rts.Setup(unitType, model.GetCurrentUnitLevelDamage(unitType), model.GetCurrentUnitLevel(unitType), teamIndex);
            rts.GetComponent<Health>().SetOnDeadCallback(OnDefenderDead);

            spawned.Add(rts);
        }

        return spawned;
    }
    public UnitRTS SpawnDefender(UnitType unitType, Vector3 spawnPosition) => SpawnDefenders(1, unitType, spawnPosition)[0];

    private List<UnitRTS> SpawnUnits(int count, UnitType unitType, Vector3 spawnPosition, int layerMask, bool isEnemy)
    {
        List<UnitRTS> spawned = new List<UnitRTS>();

        List<Vector3> positions = GetPositionListAround(spawnPosition, 8f, count);
        foreach (var p in positions)
        {
            Vector3 enemyPos = new Vector3(p.x, defenderPrefab.transform.position.y, p.z);
            GameObject enemy = Instantiate(defenderPrefab, enemyPos, Quaternion.identity, null);

            UnitRTS rts = enemy.GetComponent<UnitRTS>();
            int teamIndex = isEnemy ? 0 : GameConfigurations.DefenderTeamIndex;
            string tag = isEnemy ? "Enemy" : "Untagged";

            rts.Setup(unitType, model.GetCurrentUnitLevelDamage(unitType), model.GetCurrentUnitLevel(unitType), teamIndex);
            rts.gameObject.layer = layerMask;
            rts.gameObject.tag = tag;
            
            spawned.Add(rts);
        }

        return spawned;
    }
    private List<Vector3> GetPositionListAround(Vector3 startPosition, float[] ringDistanceArray, int[] ringPositionCountArray)
    {
        List<Vector3> positionList = new List<Vector3>();
        positionList.Add(startPosition);
        for (int i = 0; i < ringDistanceArray.Length; i++)
        {
            positionList.AddRange(GetPositionListAround(startPosition, ringDistanceArray[i], ringPositionCountArray[i]));
        }
        return positionList;
    }
    private List<Vector3> GetPositionListAround(Vector3 startPosition, float distance, int positionCount)
    {
        List<Vector3> positionList = new List<Vector3>();
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (360f / positionCount);
            Vector3 dir = ApplyRotationToVector(new Vector3(1, 0, 0), angle);
            Vector3 position = startPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }
    private Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, angle, 0) * vec;
    }

    private bool IsPointerOverUIObject(out List<RaycastResult> raycasted)
    {
        raycasted = new List<RaycastResult>();
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        EventSystem.current.RaycastAll(eventDataCurrentPosition, raycasted);

        return raycasted.Count > 0;
    }
    private Vector3 CalculateSelectionBoxRadius()
    {
        float boxWidth = selectionBox.sizeDelta.x;
        float boxHeight = selectionBox.sizeDelta.y;

        float screenAspect = Screen.width / Screen.height;
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = screenAspect * camHalfHeight;
        float camWidth = 2.0f * camHalfWidth;
        float camHeight = 2.0f * camHalfHeight;

        float pixelPerUnitW = Screen.width / camWidth;
        float pixelPerUnitH = Screen.height / camHeight;
        float w2 = (boxWidth / pixelPerUnitH);
        float h2 = (boxHeight / pixelPerUnitW);
        Vector3 radius = new Vector3(w2, 10, h2);

        return radius;
    }

    private List<Transform> GetNearestEnemies(Transform enemy)
    {
        Collider[] colliders = Physics.OverlapSphere(enemy.position, GameConfigurations.TeamAttackRadius);

        var filtered = colliders.Where((c) => c.transform.tag == "Enemy");
        var transforms = from f in filtered select f.transform;

        return transforms.ToList();

        //old
        //List<Transform> filtered = new List<Transform>();
        //foreach (var collider in colliders)
        //{
        //    UnitRTS enemyUnit = collider.GetComponent<UnitRTS>();
        //    if (enemyUnit != null && enemyUnit.TeamIndex != GameConfigurations.DefenderTeamIndex)
        //        filtered.Add(collider.transform);
        //}
        //return filtered;
    }
    private void TeamAttack(List<Transform> enemies)
    {
        int index = 0;
        foreach (var defender in selectedUnitRTSList)
        {
            defender.Attack(enemies[index]);
            
            index++;
            if (index > enemies.Count - 1)
                index = 0;
        }
    }

    private void DeselectAllUnits()
    {
        // Deselect all Units
        foreach (UnitRTS unitRTS in selectedUnitRTSList)
        {
            if(unitRTS != null)
            {
                Debug.Log($"Deselected unit: {unitRTS.name}");
                unitRTS.SetSelectedVisible(false);
            }       
        }

        selectedUnitRTSList.Clear();
    }
    private void DeselectAllBuildings()
    {
        // Deselect all Units
        foreach (BuildingRTS buildingRTS in selectedBuildings)
        {
            if(buildingRTS != null)
            {
                Debug.Log($"Deselected building: {buildingRTS.name}");
                buildingRTS.SetSelectedVisible(false);
            }
        }

        viewController.OnBuildingDeselected();
        selectedBuildings.Clear();
    }
    private void UpdateSelectedUnits(Collider[] hitColliders)
    {
        //Select Units within Selection Area
        foreach (Collider collider in hitColliders)
        {
            UnitRTS unitRTS = collider.GetComponent<UnitRTS>();
            if (unitRTS != null)
            {
                Debug.Log($"Selected unit: {unitRTS.name}");
                unitRTS.SetSelectedVisible(true);
                selectedUnitRTSList.Add(unitRTS);
            }
        }
    }
    private void UpdateSelectedBuildings(Collider[] hitColliders)
    {
        //Select Units within Selection Area
        foreach (Collider collider in hitColliders)
        {
            BuildingRTS building = collider.GetComponent<BuildingRTS>();
            if (building != null)
            {
                Debug.Log($"Selected building: {building.name}");
                building.SetSelectedVisible(true);
                selectedBuildings.Add(building);
            }
        }
    }

    private void OnDefenderDead(Transform dead)
    {
        UnitRTS defender = dead.GetComponent<UnitRTS>();
        if (selectedUnitRTSList.Contains(defender))
            selectedUnitRTSList.Remove(defender);
    }
    private void CheckWaveCompletion(Transform dead)
    {
        List<int> completedWavesIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            var destroyed = allEnemies[i].Where(e => e == null || e.GetComponent<Health>().IsDead); //#refactor
            if (destroyed.ToList().Count == allEnemies[i].Count)
                completedWavesIndexes.Add(i);
        }

        string log = "";
        foreach (var cw in completedWavesIndexes)
        {
            log += cw + ", ";
        }
        Debug.Log("WaveCompletion checked. Data: " + log);

        completedWavesIndexes.ForEach(cw => OnWaveCompleted(cw));
    }
    private void OnWaveCompleted(int waveIndex)
    {
        Debug.Log("Wave completed! Reward!");
        allEnemies.RemoveAt(waveIndex);
        model.Cash += GameConfigurations.AttackDefenseBonus;

        if (allEnemies.Count == 0 && model.AllWavesSpawned())
            OnGameEnd();
    }
    
    //testing
    //void OnDrawGizmos()
    //{

    //    //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
    //    if (radiusForDraw != null && centerForDraw != null)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawCube(centerForDraw, radiusForDraw);
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawSphere(centerForDraw, 1);
    //    }
    //    //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)

    //    //Gizmos.DrawWireCube(transform.position, transform.localScale);
    //}
}
