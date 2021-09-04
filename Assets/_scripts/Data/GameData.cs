


using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int initialCash;
    public int upgradeCost;
    public int enemiesSpawnDelay;

    public UnitData[] archerUnitData;
    public UnitData[] warriorUnitData;
    public WaveData[] waves;


    private Dictionary<string, UnitData[]> unitsData = new Dictionary<string, UnitData[]>();
    public Dictionary<string, UnitData[]> UnitsData { get => unitsData; set => unitsData = value; }
    public UnitData[] ArcherUnitData { get => archerUnitData; set { archerUnitData = value; unitsData.Add(UnitType.Archer.ToString(), archerUnitData); } }
    public UnitData[] WarriorUnitData { get => warriorUnitData; set { warriorUnitData = value; unitsData.Add(UnitType.Warrior.ToString(), archerUnitData); }}
}
