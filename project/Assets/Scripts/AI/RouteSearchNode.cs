using System.Collections.Generic;
public class RouteSearchNode
{
    private FieldCell _cell;
    private int _needCostForGoal;
    private int _moveRemaining;

    private RouteSearchNode _prevNode;

    public FieldCell Cell { get => _cell; set => _cell = value; }
    public RouteSearchNode PrevNode { get => _prevNode; set => _prevNode = value; }

    public RouteSearchNode(FieldCell nodeCell, int needCostForGoal, int moveRemaining, RouteSearchNode prevNode)
    {
        _cell = nodeCell;
        _needCostForGoal = needCostForGoal;
        _moveRemaining = moveRemaining;
        _prevNode = prevNode;
    }

    // 周囲四近傍の探索
    public List<RouteSearchNode> SearchNeighbor(GameFieldData fieldList)
    {
        List<RouteSearchNode> neighborList = new List<RouteSearchNode>();

        List<FieldCell> neighborCells = _cell.NeighborCells;

        if (neighborCells == null)
        {
            return null;
        }

        foreach (var cell in neighborCells)
        {
            if (CheckMovable(cell, fieldList))
            {
                neighborList.Add(CreateNextNode(cell));
            }
        }

        return neighborList;
    }

    private RouteSearchNode CreateNextNode(FieldCell nodePosition)
    {
        RouteSearchNode node = new RouteSearchNode(nodePosition, _needCostForGoal - 1, _moveRemaining - 1, this);

        return node;
    }

    private bool CheckMovable(FieldCell targetCell, GameFieldData fieldData)
    {
        bool ret = false;

        if (fieldData.FieldCells.ContainsValue(targetCell))
        {
            if (targetCell)
            {
                if (targetCell.Movable && (_moveRemaining > 0))
                {
                    ret = true;
                }
            }
        }

        return ret;
    }
}
