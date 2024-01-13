using System.Collections.Generic;
using UnityEngine;

public class MoveAction : ActionBase
{
    private GameFieldData _field;
    private FieldCell _destinationCell;
    private List<RouteSearchNode>[] _route;

    public void initialize(GameFieldData field, Unit unit, FieldCell destinationCell, List<RouteSearchNode>[] route)
    {
        _actionUnit = unit;
        _field = field;
        _destinationCell = destinationCell;
        _isActionStart = false;
        _route = route;
    }
    public override void Execute(UnitController unitController)
    {
        if (unitController != null && _destinationCell != null && _route != null)
        {
            unitController.MoveUnit(_field, _actionUnit, _destinationCell, _route);
            _isActionStart = true;
        }
    }

    public override bool IsFinishedAction(UnitController unitController)
    {
        if (_actionUnit == null)
        {
            Debug.Log("_actionUnit is null");
            return false;
        }
        if (!_isActionStart)
        {
            return false;/* 実行前 */
        }

        if (_actionUnit.IsInRunAnim)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
