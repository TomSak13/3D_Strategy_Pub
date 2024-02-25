
public class AttackAction : ActionBase
{
    private Unit _attackTarget;

    public AttackAction(Unit unit, Unit attackTarget)
    {
        _actionUnit = unit;
        _attackTarget = attackTarget;
    }

    public override void Execute(UnitController unitController)
    {
        if (unitController != null)
        {
            unitController.Battle(_actionUnit, _attackTarget);
        }
    }
}
