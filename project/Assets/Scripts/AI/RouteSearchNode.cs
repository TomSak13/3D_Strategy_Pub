using System.Collections.Generic;

public class RouteSearchNode
{
    private readonly int _moveRemaining;

    public FieldCell Cell { get; }
    public RouteSearchNode PrevNode { get; }

    public RouteSearchNode(FieldCell nodeCell, int movePower)
    {
        Cell = nodeCell;
        _moveRemaining = movePower;
        PrevNode = null!;
    }
    private RouteSearchNode(FieldCell nodeCell, RouteSearchNode prevNode)
    {
        Cell = nodeCell;
        _moveRemaining = prevNode._moveRemaining - 1;
        PrevNode = prevNode;
    }

    /// <summary>
    /// 周囲四近傍の探索
    /// </summary>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public static List<RouteSearchNode> SearchNeighbor(GameFieldData fieldList, RouteSearchNode node)
    {
        List<RouteSearchNode> neighborList = new List<RouteSearchNode>();

        List<FieldCell> neighborCells = node.Cell.NeighborCells;

        if (neighborCells == null)
        {
            throw new System.InvalidOperationException("_cell.neighborCells field is null.");
        }

        foreach (var cell in neighborCells)
        {
            if (CheckMovable(cell, fieldList, node))
            {
                neighborList.Add(CreateNextNode(cell, node));
            }
        }

        return neighborList;
    }

    private static RouteSearchNode CreateNextNode(FieldCell nodePosition, RouteSearchNode parentNode)
        => new(nodePosition, parentNode);

    private static bool CheckMovable(FieldCell targetCell, GameFieldData fieldData, RouteSearchNode node)
    {
        bool ret = false;

        if (fieldData.FieldCells.ContainsKey(targetCell.gameObject.transform.position))
        {
            if (targetCell)
            {
                if (targetCell.Movable && (node._moveRemaining > 0))
                {
                    ret = true;
                }
            }
        }

        return ret;
    }
}
