using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfigurations
{
    public const float ArcherAttackDistance = 30f; //����������� ��������� ��� �����
    public const float WarriorAttackDistance = 5f; //����������� ��������� ��� �����

    public const float UnitCreationDelay = 2f; //�������� ��������������� ������ (� ��������)

    public const int DefenderTeamIndex = 1; 

    public const float TeamAttackRadius = 35f; //������ ������ ������ ��� ����� ��� ��������� ���������

    public const float EnemyDetectRadius = 50; //��������� ��������� ������ �����

    public const int AttackDefenseBonus = 100; //������� �� ���������� �����

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
