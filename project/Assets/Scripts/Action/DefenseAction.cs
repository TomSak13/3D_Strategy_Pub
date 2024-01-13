
public class DefenseAction : ActionBase
{

    public void Initialize(Unit unit)
    {
        _actionUnit = unit;
        _isActionStart = false;
    }
    public override void Execute(UnitController unitController)
    {
        if (_actionUnit != null)
        {
            _actionUnit.InDefense();
            _actionUnit.State = Unit.ActionState.Defense;
            _isActionStart = true;
        }
    }

    public override bool IsFinishedAction(UnitController unitController)
    {
        return _isActionStart;
    }
}
