using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationAI
{
    /// <summary>
    /// 移動ルートの取得
    /// </summary>
    /// <param name="goalCell"></param>
    /// <param name="routeNode"></param>
    /// <returns></returns>
    public static List<FieldCell> GetRoute(FieldCell goalCell, RouteSearchNode[][] routeNode)
    {
        if (routeNode == null)
        {
            throw new System.InvalidOperationException("routeNode field is null.");
        }

        int targetNodeIndex;
        int targetListIndex = 0;
        bool isFoundRoute = false;
        for (targetNodeIndex = 0; targetNodeIndex < (routeNode.Length); targetNodeIndex++)
        {
            for (targetListIndex = 0; targetListIndex < routeNode[targetNodeIndex].Length; targetListIndex++)
            {
                if (routeNode[targetNodeIndex][targetListIndex].Cell == goalCell)
                {
                    /* ターゲットノード発見 */
                    isFoundRoute = true;
                    break;
                }
            }

            if (isFoundRoute)
            {
                break;
            }
        }

        RouteSearchNode node = routeNode[targetNodeIndex][targetListIndex];
        List<FieldCell> routeList = new List<FieldCell>();

        /* 移動予定地からさかのぼる形にルートリスト変換 */
        while (node != null)
        {
            routeList.Add(node.Cell);
            node = node.PrevNode;
        }

        routeList.Reverse(); /* index:0が次の移動位置になるようリスト逆転 */

        return routeList;
    }

    public static bool IsExistMovablePosition(GameFieldData fieldData, FieldCell targetCell)
    {
        if (fieldData == null)
        {
            throw new System.InvalidOperationException("fieldData field is null or movePower <= 0.");
        }

        foreach(var cell in targetCell.NeighborCells)
        {
            if (cell.Movable)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 移動可能範囲を取得
    /// </summary>
    /// <param name="fieldData"></param>
    /// <param name="targetCell"></param>
    /// <param name="movePower"></param>
    /// <returns></returns>
    public static RouteSearchNode[][] InvestigateMovablePosition(GameFieldData fieldData, FieldCell targetCell, int movePower)
    {
        if (movePower <= 0 || fieldData == null)
        {
            throw new System.InvalidOperationException("fieldData field is null or movePower <= 0.");
        }

        var routeNode = new List<RouteSearchNode>[movePower + 1];

        /* 初期ノードを格納 */
        RouteSearchNode startNode = new RouteSearchNode(targetCell, movePower);

        routeNode[0] = new List<RouteSearchNode> { startNode };

        /* ルート探索 */
        for (int i = 0; i < (routeNode.Length - 1); i++)
        {
            /* 次のノードを入れる用のリストを作成 */
            routeNode[i + 1] = new List<RouteSearchNode>();
            foreach (var node in routeNode[i])
            {
                /* リスト内のノードを確認し、四近傍内の移動可能エリアリストを作成 */
                List<RouteSearchNode> nextRouteList = RouteSearchNode.SearchNeighbor(fieldData, node);
                /* 次のノードを入れる用のリストに格納 */
                routeNode[i + 1].AddRange(nextRouteList);
            }

            /* 次のリストに何も入っていない場合は終了 */
            if (routeNode[i + 1].Count <= 0)
            {
                break;
            }
        }

        /* 移動先がない場合はダミーのから配列を返す */
        if (routeNode[0].Count <= 0)
        {

        }

        return routeNode.Select(static x => x.ToArray()).ToArray();
    }

    public static bool IsMovable(RouteSearchNode[][] routeNode, FieldCell targetCell)
    {
        bool ret = false;
        if (routeNode == null)
        {
            throw new System.InvalidOperationException("routeNode field is null.");
        }

        foreach (var nodes in routeNode)
        {
            if (nodes == null)
            {
                break;
            }

            foreach (var node in nodes)
            {
                if (node.Cell == targetCell)
                {
                    return true;
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// 第一引数に入れたユニットが第二引数を目標に移動する際に、移動可能範囲内の目標に近い位置を返す
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="destinationCell"></param>
    /// <param name="fieldData"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    public static FieldCell InvestigateMovePosition(Unit unit, FieldCell destinationCell, GameFieldData fieldData, RouteSearchNode[][] route)
    {
        if (unit == null || destinationCell == null || fieldData == null || route == null)
        {
            throw new System.InvalidOperationException("unit or DestinationCell or fieldData or route field is null.");
        }

        /* 目標セルが移動可能範囲にある場合 */
        if (IsMovable(route, destinationCell))
        {
            return destinationCell;
        }
        /* 目的地が移動不可能ならば探索 */


        FieldCell searchCell = destinationCell;
        List<FieldCell> neighborCells;
        float distance = float.MaxValue;

        while (searchCell != null)
        {
            neighborCells = searchCell.NeighborCells;

            searchCell = null!;
            foreach (var cell in neighborCells)
            {
                /* 前のループでの距離と比べて一番近い位置を調べる */
                if ((cell.transform.position - unit.OnCell.transform.position).sqrMagnitude < distance)
                {
                    searchCell = cell;
                    distance = (cell.transform.position - unit.OnCell.transform.position).sqrMagnitude;
                }
            }

            if (searchCell != null)
            {
                if (IsMovable(route, searchCell))
                {
                    break;
                }
            }
        }

        FieldCell targetCell = searchCell!;
        if (targetCell == null)
        {
            throw new System.InvalidOperationException("investigate err by algorithm");
        }

        return targetCell;
    }

    /// <summary>
    /// 第一引数のセルから見て、第二引数のセルから一番遠いセルを返す
    /// </summary>
    /// <param name="targetCell"></param>
    /// <param name="escapeTargetCell"></param>
    /// <param name="fieldData"></param>
    /// <returns></returns>
    public static FieldCell InvestigateEscapePosition(FieldCell targetCell, FieldCell escapeTargetCell, GameFieldData fieldData)
    {
        if (targetCell == null || escapeTargetCell == null || fieldData == null)
        {
            throw new System.InvalidOperationException("targetCell or escapeTargetCell or fieldData field is null.");
        }

        FieldCell searchCell;
        List<FieldCell> neighborCells;
        float distance = 0f;
        FieldCell farCell = null!;

        searchCell = targetCell;
        while (searchCell != null)
        {
            neighborCells = searchCell.NeighborCells;

            searchCell = null!;
            foreach (var cell in neighborCells)
            {
                /* 前のループでの距離と比べて一番遠い位置を調べる */
                if ((cell.transform.position - escapeTargetCell.transform.position).sqrMagnitude > distance)
                {
                    farCell = cell;
                    searchCell = cell;
                    distance = (cell.transform.position - escapeTargetCell.transform.position).sqrMagnitude;
                }
            }
        }

        if (farCell == null)
        {
            throw new System.InvalidOperationException("InvestigateEscapePosition err by algorithm");
        }

        return farCell;
    }

    private static Dictionary<FieldCell, Unit> InvestigateUnitAttackRangeCell(Unit unit, GameFieldData field)
    {
        if (field == null)
        {
            throw new System.InvalidOperationException("fieldData field is null.");
        }

        Dictionary<FieldCell, Unit> attackRangeCell = new Dictionary<FieldCell, Unit>();
        Stack<FieldCell> searchCells = new Stack<FieldCell>();
        List<FieldCell> neighborCells;

        searchCells.Push(unit.OnCell);
        for (int i = 0; i < unit.AttackRange; i++)
        {
            neighborCells = searchCells.Pop().NeighborCells;
            foreach (var cell in neighborCells)
            {
                if (cell.Movable)
                {
                    attackRangeCell.Add(cell, unit);
                }
                searchCells.Push(cell); // 隣接セルを次の探索対象に入れる
            }

            if (searchCells.Count <= 0) // 探索対象セルが無くなった場合は終了
            {
                break;
            }
        }

        return attackRangeCell;
    }

    public static Dictionary<FieldCell, Unit> InvestigateAttackableCellInRoutes(RouteSearchNode[][] routes, List<Unit> enemies, GameFieldData field)
    {
        Dictionary<FieldCell, Unit> attackableCell = new Dictionary<FieldCell, Unit>();
        Dictionary<FieldCell, Unit> attackRangeCells = new Dictionary<FieldCell, Unit>(); /* enemysに入っている敵への攻撃可能範囲が全て格納される */

        foreach (var enemy in enemies)
        {
            Dictionary<FieldCell, Unit> attackRangeCell = InvestigateUnitAttackRangeCell(enemy, field); /* 敵1ユニット分の攻撃可能範囲を取得 */
            if (attackRangeCell == null)
            {
                continue;
            }
            if (attackRangeCell.Count <= 0)
            {
                continue;
            }

            /* 攻撃可能範囲を一つのDictionaryにまとめる */
            foreach (var (attackCell, unit) in attackRangeCell)
            {
                attackRangeCells.TryAdd(attackCell, unit);
            }
        }

        /* 攻撃可能範囲と移動可能範囲を比較し、移動可能かつ、攻撃が可能なセルを抽出 */
        foreach (var nodes in routes)
        {
            if (nodes == null)
            {
                continue;
            }

            foreach (var node in nodes)
            {
                foreach (var (cell, unit) in attackRangeCells)
                {
                    if (node.Cell == cell)
                    {
                        if (!attackableCell.ContainsKey(cell))
                        {
                            attackableCell.Add(cell, unit);
                        }
                    }
                }
            }
        }

        return attackableCell;
    }
}
