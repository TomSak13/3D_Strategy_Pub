
public class DefenseAction : ActionBase
{
    public DefenseAction(Unit unit)
    {
        _actionUnit = unit;
    }

    public override void Execute(UnitController unitController)
    {
        if (_actionUnit != null)
        {
            _actionUnit.InDefense();
            _actionUnit.State = Unit.ActionState.Defense;
        }
    }
}
