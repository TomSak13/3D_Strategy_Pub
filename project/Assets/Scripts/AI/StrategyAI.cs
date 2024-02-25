using System;
using System.Collections.Generic;
using UnityEngine;

public class StrategyAI
{
    private const int MaxNeighborCell = 10;
    private const float SearchRadius = 5.0f;
    private const int DiffBorder = 3;
    private const float DiffHpAveBorder = 30f;

    public event Action UnitActFinishedEvent = default!;

    private UnitController _unitController;

    private Unit.Team _targetTeam;
    private CharacterAI _characterAI;
    private GameFieldData _field;


    /// <summary>
    /// このオブジェクト生成時の初期化
    /// </summary>
    /// <param name="team"></param>
    /// <param name="fieldData"></param>
    /// <param name="unitController"></param>
    public StrategyAI(Unit.Team team, GameFieldData fieldData, UnitController unitController)
    {
        _targetTeam = team;
        _field = fieldData;
        _unitController = unitController;

        _characterAI = new CharacterAI(fieldData, _unitController);
    }

    /// <summary>
    /// 戦略指示
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="fieldData"></param>
    private void DecideStrategy(Unit unit, GameFieldData fieldData)
    {
        _characterAI.TargetUnit = unit;

        int countDiff = CalcDiffUnitsCount(unit.UnitTeam, fieldData);

        float hpAveDiff = CalcDiffHpAverage(unit.UnitTeam, fieldData);

        if (Mathf.Abs(countDiff) > DiffBorder || Mathf.Abs(hpAveDiff) > DiffHpAveBorder)
        {
            if (countDiff > 0)
            {
                /* 優勢 */
                _characterAI.SetStrategy(CharacterAI.Strategy.Attack, null!, fieldData);
            }
            else
            {
                /* 劣勢 */
                var escapeCell = unit.UnitTeam switch
                {
                    Unit.Team.Player => NavigationAI.InvestigateEscapePosition(SeekCenter(fieldData, Unit.Team.Enemy),
                        SeekCenter(fieldData, Unit.Team.Player), fieldData),
                    _ => NavigationAI.InvestigateEscapePosition(SeekCenter(fieldData, Unit.Team.Player),
                        SeekCenter(fieldData, Unit.Team.Enemy), fieldData)
                };
                _characterAI.SetStrategy(CharacterAI.Strategy.Recovery, escapeCell, fieldData);
            }
        }
        else
        {
            /* 陣地が遠い場合はそのままバランス戦略を渡す */
            _characterAI.SetStrategy(CharacterAI.Strategy.Balance, null!, fieldData);
        }
    }

    /// <summary>
    /// 行動指示をしたユニットの行動が終了
    /// </summary>
    public void FinishCurrentTargetTurn()
    {   
        _field.CurrentTarget.ControlState = Unit.ActControlState.None;
        _field.CurrentTarget.EndTurn();
        UnitActFinishedEvent?.Invoke();
    }

    /// <summary>
    /// 各ユニットを更新
    /// </summary>
    public void ActUnits()
    {
        DecideStrategy(_field.CurrentTarget, _field);
        _characterAI.Act(this); // CharacterAIに行動通知
    }

    private Vector3 CalcIntCoordinateFromFloat(Vector3 coordinate)
    {
        Vector3 retCoordinate = Vector3.zero;

        retCoordinate.x = Mathf.Round(coordinate.x);
        retCoordinate.y = Mathf.Round(coordinate.y);
        retCoordinate.z = Mathf.Round(coordinate.z);

        return retCoordinate;
    }

    private Vector3 CalcCenterCoordinate(List<Unit> units)
    {
        Vector3 centerCoordinate = Vector3.zero;

        if (units == null)
        {
            return centerCoordinate;
        }
        if (units.Count == 0)
        {
            return centerCoordinate;
        }

        foreach (var unit in units)
        {
            if (unit.OnCell != null)
            {
                centerCoordinate += unit.OnCell.transform.position;
            }
        }
        centerCoordinate = centerCoordinate / units.Count;
        centerCoordinate = CalcIntCoordinateFromFloat(centerCoordinate);

        return centerCoordinate;
    }

    /// <summary>
    /// CalcCenterCoordinate()で指定した座標のセルがない場合に呼ばれる。
    /// </summary>
    /// <param name="position"></param>
    /// <param name="groundCheckRadius"></param>
    /// <returns></returns>
    private FieldCell GetNeighborCell(Vector3 position, float groundCheckRadius)
    {
        Debug.Log("not found Center Cell from CalcCenterCoordinate()");
        FieldCell retCell = null!;
        RaycastHit[] neighborCells = new RaycastHit[MaxNeighborCell];

        Physics.SphereCastNonAlloc(position, groundCheckRadius, Vector3.forward, neighborCells);

        foreach (var hitObj in neighborCells)
        {
            if (hitObj.collider.gameObject.CompareTag(FieldCell.ObjTag))
            {
                retCell = hitObj.collider.gameObject.GetComponent<FieldCell>();
            }
        }

        if (retCell == null)
        {
            throw new System.InvalidOperationException("GetNeighborCell err by algorithm");
        }

        return retCell;
    }

    private float CalcDiffHpAverage(Unit.Team team, GameFieldData fieldData)
    {
        float allyHpAve = 0;
        float enemyHpAve = 0;

        if (team == Unit.Team.Player)
        {
            allyHpAve = CalcHpAverage(fieldData.PlayerUnits);
            enemyHpAve = CalcHpAverage(fieldData.EnemyUnits);
        }
        else
        {
            allyHpAve = CalcHpAverage(fieldData.EnemyUnits);
            enemyHpAve = CalcHpAverage(fieldData.PlayerUnits);
        }

        return enemyHpAve - allyHpAve;
    }

    private float CalcHpAverage(List<Unit> units)
    {
        float ave = 0;

        if (units == null)
        {
            return 0;
        }
        if (units.Count == 0)
        {
            return 0;
        }

        foreach (var unit in units)
        {
            ave += unit.CurrentHp;
        }

        ave = ave / units.Count;

        return ave;
    }

    private int CalcDiffUnitsCount(Unit.Team team, GameFieldData fieldData)
    {
        int diffCount = 0;

        if (fieldData == null)
        {
            return diffCount;
        }

        if (team == Unit.Team.Enemy)
        {
            diffCount = fieldData.EnemyUnits.Count - fieldData.PlayerUnits.Count;
        }
        else
        {
            diffCount = fieldData.PlayerUnits.Count - fieldData.EnemyUnits.Count;
        }

        return diffCount;
    }

    /// <summary>
    /// 陣地の位置を計算する
    /// </summary>
    /// <param name="fieldData"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    private FieldCell SeekCenter(GameFieldData fieldData, Unit.Team team)
    {
        Vector3 center;

        if (team == Unit.Team.Player)
        {
            center = CalcCenterCoordinate(fieldData.PlayerUnits);
        }
        else
        {
            center = CalcCenterCoordinate(fieldData.EnemyUnits);
        }

        var retCell = fieldData.FieldCells.TryGetValue(center, out var cell) ? cell :
            /* 重心からrayを出して、rayに当たるFieldCellを探す */
            GetNeighborCell(center, SearchRadius);

        return retCell;
    }
}
