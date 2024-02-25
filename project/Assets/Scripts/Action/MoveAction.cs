
public class MoveAction : ActionBase
{
    private GameFieldData _field;
    private FieldCell _destinationCell;
    private RouteSearchNode[][] _route;

    public MoveAction(GameFieldData field, Unit unit, FieldCell destinationCell, RouteSearchNode[][] route)
    {
        _actionUnit = unit;
        _field = field;
        _destinationCell = destinationCell;
        _route = route;
    }

    public override void Execute(UnitController unitController)
    {
        if (unitController != null && _destinationCell != null && _route != null)
        {
            unitController.MoveUnit(_field, _actionUnit, _destinationCell, _route);
        }
    }
}
