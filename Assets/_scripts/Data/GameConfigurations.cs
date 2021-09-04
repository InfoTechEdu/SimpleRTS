using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfigurations
{
    public const float ArcherAttackDistance = 30f; //минимальная дистанция для атаки
    public const float WarriorAttackDistance = 5f; //минимальная дистанция для атаки

    public const float UnitCreationDelay = 2f; //задержка инстанцирования юнитов (с барраков)

    public const int DefenderTeamIndex = 1; 

    public const float TeamAttackRadius = 35f; //радиус поиска врагов для атаки при групповом нападении

    public const float EnemyDetectRadius = 50; //дальность видимости чужого юнита

    public const int AttackDefenseBonus = 100; //награда за отраженную атаку

    public static float GetMinAttackDistance(UnitType type)
    {
        switch (type)
        {
            case UnitType.Warrior:
                return WarriorAttackDistance;
            case UnitType.Archer:
                return ArcherAttackDistance;
            default:
                return 0f;
        }
    }


}
