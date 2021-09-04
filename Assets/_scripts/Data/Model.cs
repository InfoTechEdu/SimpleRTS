using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class Model : MonoBehaviour
{
    private GameController gameController;
    private GameData gameData;

    private int cash;
    private Dictionary<UnitType, int> currentUnitLevel;
    private WaveData currentWave;

    private int currentWaveIndex;

    public List<Action<int>> onCashUpdatedCallbacks; 
    private delegate void OnCashUpdated();

    public int Cash
    {
        get
        {
            //gameController.OnCashUpdated
            return cash;
        }
        set
        {
            cash = value;
            onCashUpdatedCallbacks.ForEach(callback => callback.Invoke(cash));
        }
    }
    public Dictionary<UnitType, int> CurrentUnitLevel { get => currentUnitLevel; }
    public int EnemySpawnDelay { get => gameData.enemiesSpawnDelay; }
    public int CurrentWaveIndex { get => currentWaveIndex; }
    public int WavesCount { get => gameData.waves.Length; }

    private void Awake()
    {
        onCashUpdatedCallbacks = new List<Action<int>>();
    }
    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        Init();
        yield return new WaitUntil(() => gameData != null);
        yield return new WaitForSeconds(.5f); //emulating data loading. #Remove this later

        SceneManager.LoadScene("Main");
    }

    private void LoadSettings()
    {
        string path = Application.streamingAssetsPath + "/game_data.json";
        string gameDataStr = File.ReadAllText(path);

        gameData = JsonUtility.FromJson<GameData>(gameDataStr);
    }
    private void Init()
    {
        currentUnitLevel = new Dictionary<UnitType, int> { {UnitType.Warrior, 0}, {UnitType.Archer, 0} };
        currentWave = gameData.waves[currentWaveIndex];

        cash = gameData.initialCash;

        gameData.UnitsData.Add(UnitType.Archer.ToString(), gameData.ArcherUnitData);
        gameData.UnitsData.Add(UnitType.Warrior.ToString(), gameData.WarriorUnitData);
    }

    public void PurchaseUnit(UnitType unitType)
    {
        Cash -= GetUnitPrice(unitType, GetCurrentUnitLevel(unitType));
    }
    public void PurchaseUnitUpgrade(UnitType unitType)
    {
        if(currentUnitLevel[unitType] < gameData.UnitsData[unitType.ToString()].Length - 1)
        {
            currentUnitLevel[unitType]++;
            Cash -= gameData.upgradeCost;
        }
    }

    public void AddOnCashUpdatedCallback(Action<int> callback) =>
        onCashUpdatedCallbacks.Add(callback);
    public void RemoveOnCashUpdatedCallback(Action<int> callback) =>
        onCashUpdatedCallbacks.Remove(callback);

    public bool AllWavesSpawned()
    {
        return currentWaveIndex >= WavesCount;
    }
    public bool HasUpgrade(UnitType unitType)
    {
        if (!gameData.UnitsData.ContainsKey(unitType.ToString()))
            return false;

        int maxLevel = gameData.UnitsData[unitType.ToString()].Length - 1;
        return currentUnitLevel[unitType] < maxLevel;
    }
    public int GetUpgradeCost()
    {
        return gameData.upgradeCost;
    }
    public WaveData GetNextWave()
    {
        if (currentWaveIndex > gameData.waves.Length - 1)
            return null;

        WaveData wave = gameData.waves[currentWaveIndex];
        currentWaveIndex++;
        return wave;
    }
    public int GetCurrentUnitLevel(UnitType unitType)
    {
        currentUnitLevel.TryGetValue(unitType, out int level);
        return level;
    }
    public int GetCurrentUnitLevelDamage(UnitType unitType) {
        string strType = unitType.ToString();
        int level = GetCurrentUnitLevel(unitType);

        return gameData.UnitsData[strType][level].damage;
    }
    public int GetUnitPrice(UnitType unitType, int unitLevel)
    {
        if (!gameData.UnitsData.ContainsKey(unitType.ToString()))
            return 0;

        string strType = unitType.ToString();
        if (unitLevel > gameData.UnitsData[strType].Length - 1)
            return gameData.UnitsData[strType].Last().price; //return last level price for unitType
        else
            return gameData.UnitsData[strType][unitLevel].price;
    }
    public int GetUnitDamage(UnitType unitType, int unitLevel)
    {
        string strType = unitType.ToString();
        if (unitLevel > gameData.UnitsData[strType].Length - 1)
            return gameData.UnitsData[strType].Last().damage; //return last damage level price for unitType
        else
            return gameData.UnitsData[strType][unitLevel].damage;
    }
}
