using System.Collections.Generic;
using UnityEngine;

public class NavigationAI
{
    /// <summary>
    /// 移動ルートの取得
    /// </summary>
    /// <param name="goalCell"></param>
    /// <param name="routeNode"></param>
    /// <returns></returns>
    public static List<FieldCell> GetRoute(FieldCell goalCell, List<RouteSearchNode>[] routeNode)
    {
        List<FieldCell> routeList = new List<FieldCell>();
        int targetNodeIndex;
        int targetListIndex;
        RouteSearchNode node = null;

        if (routeNode == null)
        {
            return null;
        }

        for (targetNodeIndex = 0; targetNodeIndex < (routeNode.Length); targetNodeIndex++)
        {
            for (targetListIndex = 0; targetListIndex < routeNode[targetNodeIndex].Count; targetListIndex++)
            {
                if (routeNode[targetNodeIndex][targetListIndex].Cell == goalCell)
                {
                    node = routeNode[targetNodeIndex][targetListIndex]; /* ターゲットノード発見 */
                    break;
                }
            }

            if (node != null)
            {
                break;
            }
        }

        /* 移動予定地からさかのぼる形にルートリスト変換 */
        while (node != null)
        {
            routeList.Add(node.Cell);
            node = node.PrevNode;
        }

        routeList.Reverse(); /* index:0が次の移動位置になるようリスト逆転 */

        return routeList;
    }

    /// <summary>
    /// 移動可能範囲を取得
    /// </summary>
    /// <param name="fieldData"></param>
    /// <param name="targetCell"></param>
    /// <param name="movePower"></param>
    /// <returns></returns>
    public static List<RouteSearchNode>[] InvestigateMovablePosition(GameFieldData fieldData, FieldCell targetCell, int movePower)
    {
        int needCostForGoal;
        List<RouteSearchNode>[] routeNode;


        if (movePower <= 0 || fieldData == null)
        {
            return null;
        }

        routeNode = new List<RouteSearchNode>[movePower + 1];

        /* needCostForGoal を計算 */
        needCostForGoal = movePower;

        /* 初期ノードを格納 */
        RouteSearchNode startNode = new RouteSearchNode(targetCell, needCostForGoal, movePower, null);
        routeNode[0] = new List<RouteSearchNode>();
        routeNode[0].Add(startNode);

        /* ルート探索 */
        for (int i = 0; i < (routeNode.Length - 1); i++)
        {
            /* 次のノードを入れる用のリストを作成 */
            routeNode[i + 1] = new List<RouteSearchNode>();
            foreach (var node in routeNode[i])
            {
                /* リスト内のノードを確認し、四近傍内の移動可能エリアリストを作成 */
                List<RouteSearchNode> nextRouteList = node.SearchNeighbor(fieldData);
                /* 次のノードを入れる用のリストに格納 */
                routeNode[i + 1].AddRange(nextRouteList);
            }

            /* 次のリストに何も入っていない場合は終了 */
            if (routeNode[i + 1].Count <= 0)
            {
                break;
            }
        }

        return routeNode;
    }

    public static bool IsMovable(List<RouteSearchNode>[] routeNode, FieldCell targetCell)
    {
        bool ret = false;
        if (routeNode == null)
        {
            return false;
        }

        foreach (var nodes in routeNode)
        {
            if (nodes == null)
            {
                break;
            }

            foreach(var node in nodes)
            {
                if (node.Cell == targetCell)
                {
                    ret = true;
                    break;
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// 第一引数に入れたユニットが第二引数を目標に移動する際に、移動可能範囲内の目標に近い位置を返す
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="DestinationCell"></param>
    /// <param name="fieldData"></param>
    /// <returns></returns>
    public static FieldCell InvestigateMovePosition(Unit unit, FieldCell DestinationCell, GameFieldData fieldData, List<RouteSearchNode>[] route)
    {
        if (unit == null || DestinationCell == null || fieldData == null || route == null)
        {
            return null;
        }
        
        /* 目標セルが移動可能範囲にある場合 */
        if (IsMovable(route, DestinationCell))
        {
            return DestinationCell;
        }
        /* 目的地が移動不可能ならば探索 */

        FieldCell targetCell = null;
        FieldCell searchCell = DestinationCell;
        List<FieldCell> neighborCells;
        float distance = float.MaxValue;

        while (searchCell != null)
        {
            neighborCells = searchCell.NeighborCells;

            searchCell = null;
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
                    targetCell = searchCell;
                    break;
                }
            }
        }

        if (targetCell == null)
        {
            Debug.Log("investigate err by algorithm"); /* デバッグ用 */
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
            return null;
        }

        FieldCell searchCell;
        List<FieldCell> neighborCells;
        float distance = 0f;
        FieldCell farCell = null;

        searchCell = targetCell;
        while (searchCell != null)
        {
            neighborCells = searchCell.NeighborCells;

            searchCell = null;
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

        return farCell;
    }

    private static Dictionary<FieldCell,Unit> InvestigateUnitAttackRangeCell(Unit unit, GameFieldData field)
    {
        if (field == null)
        {
            return null;
        }

        Dictionary<FieldCell, Unit> attackRangeCell = new Dictionary<FieldCell, Unit>();
        Stack<FieldCell> searchCells = new Stack<FieldCell>();
        List<FieldCell> neighborCells;

        searchCells.Push(unit.OnCell);
        for (int i = 0; i < unit.AttackRange; i++)
        {
            neighborCells = searchCells.Pop().NeighborCells;
            foreach(var cell in neighborCells)
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

    public static Dictionary<FieldCell, Unit> InvestigateAttackableCellInRoutes(List<RouteSearchNode>[] routes, List<Unit> enemys, GameFieldData field)
    {
        Dictionary<FieldCell, Unit> attackableCell = new Dictionary<FieldCell, Unit>();
        Dictionary<FieldCell, Unit> attackRangeCells = new Dictionary<FieldCell, Unit>(); /* enemysに入っている敵への攻撃可能範囲が全て格納される */

        foreach (var enemy in enemys)
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
            foreach(var (attackCell,unit) in attackRangeCell)
            {
                if (!attackRangeCells.ContainsKey(attackCell))
                {
                    attackRangeCells.Add(attackCell, unit);
                }
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
