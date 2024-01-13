using UnityEngine;

public class AttackAction : ActionBase
{
    private Unit _attackTarget;

    public void Initialize(Unit unit, Unit attackTarget)
    {
        _actionUnit = unit;
        _attackTarget = attackTarget;
        _isActionStart = false;
    }
    
    public override void Execute(UnitController unitController)
    {
        if (unitController != null)
        {
            unitController.Battle(_actionUnit, _attackTarget);
            _isActionStart = true;
        }
    }

    public override bool IsFinishedAction(UnitController unitController)
    {
        if (unitController == null)
        {
            Debug.Log("_battle is null");
            return false;
        }
        if (!_isActionStart)
        {
            return false;/* 実行前 */
        }

        if (unitController.GetBattleState() == Battle.BattleState.None)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
