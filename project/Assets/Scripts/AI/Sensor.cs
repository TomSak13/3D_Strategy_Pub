using System.Collections.Generic;
using UnityEngine;

public class Sensor
{
    private GameFieldData _field;

    public Sensor()
    {

    }

    public void Initialize(GameFieldData field)
    {
        _field = field;
    }

    private Unit SeekNeighborUnitFromList(List<Unit> unitList, Unit targetUnit)
    {
        Unit retUnit = null;
        float diff = float.MaxValue;

        foreach (var unit in unitList)
        {
            if ((unit.transform.position - targetUnit.transform.position).sqrMagnitude < diff)
            {
                diff = (unit.transform.position - targetUnit.transform.position).sqrMagnitude;
                retUnit = unit;
            }
        }

        return retUnit;
    }

    public bool IsInAttackableRange(Unit attackCharacter, Unit defenseCharacter)
    {
        if (Mathf.Pow(attackCharacter.AttackRange, 2) < (attackCharacter.OnCell.transform.position - defenseCharacter.OnCell.transform.position).sqrMagnitude)
        {
            return false;
        }

        return true;
    }

    public List<Unit> GetAttackableUnits(Unit unit)
    {
        List<Unit> AttackableUnits = new List<Unit>();
        List<Unit> targetUnits;

        if (unit.UnitTeam == Unit.Team.Player)
        {
            targetUnits = _field.EnemyUnits;
        }
        else
        {
            targetUnits = _field.PlayerUnits;
        }

        foreach (var enemyUnit in targetUnits)
        {
            if (IsInAttackableRange(unit, enemyUnit))
            {
                AttackableUnits.Add(enemyUnit);
            }
        }

        return AttackableUnits;
    }

    public bool IsInAttackableRangeForCell(Unit attackCharacter, Unit defenseCharacter, FieldCell cell)
    {
        if (Mathf.Pow(attackCharacter.AttackRange, 2) < (cell.transform.position - defenseCharacter.OnCell.transform.position).sqrMagnitude)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 第二引数の位置にユニットが移動したときに攻撃可能なユニットを返す
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<Unit> GetAttacakbleUnitAfterMoved(Unit unit, FieldCell cell)
    {
        List<Unit> AttackableUnits = new List<Unit>();
        List<Unit> targetUnits;

        if (unit.UnitTeam == Unit.Team.Player)
        {
            targetUnits = _field.EnemyUnits;
        }
        else
        {
            targetUnits = _field.PlayerUnits;
        }

        foreach (var enemyUnit in targetUnits)
        {
            if (IsInAttackableRangeForCell(unit, enemyUnit, cell))
            {
                AttackableUnits.Add(enemyUnit);
            }
        }

        return AttackableUnits;
    }

    /// <summary>
    /// 一番近い敵を返す
    /// </summary>
    /// <returns></returns>
    public Unit SeekNeighborEnemy(Unit targetUnit)
    {
        List<Unit> targetList;

        if (targetUnit.UnitTeam == Unit.Team.Player)
        {
            targetList = _field.EnemyUnits;
        }
        else
        {
            targetList = _field.PlayerUnits;
        }

        return SeekNeighborUnitFromList(targetList, targetUnit);
    }

}
