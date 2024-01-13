using System;
using System.Collections.Generic;
using UnityEngine;

public class StrategyAI : MonoBehaviour
{
    public const int MaxNeighborCell = 10;
    public const float SearchRadious = 5.0f;
    public const int DiffBorder = 3;
    public const float DiffHpAveBorder = 30f;

    public event Action UnitActFinishedEvent;

    [SerializeField] private UnitController _unitController;

    private Unit.Team _targetTeam;
    private CharacterAI _characterAI;
    private GameFieldData _field;

    private GameFieldData.Turn _assignTurn;

    private RaycastHit[] _neighborCells;

    private void Update()
    {
        if (_characterAI == null || _field == null)
        {
            return;
        }
        if (_field.CurrentTarget == null)
        {
            return;
        }
        if (_field.CurrentTurn != _assignTurn)
        {
            return; /* 違うプレイヤーのターン */
        }

        switch (_field.CurrentTarget.ControlState)
        {
            case Unit.ActControlState.None:
                break;
            case Unit.ActControlState.ThinkStrategy:
                DecideStrategy(_field.CurrentTarget, _field);
                _field.CurrentTarget.ControlState = Unit.ActControlState.StartAct;
                break;
            case Unit.ActControlState.StartAct:
                if (!_characterAI.ActUnit())
                {
                    _field.CurrentTarget.ControlState = Unit.ActControlState.Finished; /* 行動終了 */
                }
                else
                {
                    _field.CurrentTarget.ControlState = Unit.ActControlState.IsInAct; /* 同期待ちへ入る */
                }
                break;
            case Unit.ActControlState.IsInAct:
                if (_characterAI.IsFinishedAct()) /* 行動終了待ち */
                {
                    _field.CurrentTarget.ControlState = Unit.ActControlState.StartAct; /* 次の行動へ */
                }
                break;
            case Unit.ActControlState.Finished:
                UnitActFinishedEvent.Invoke();
                _field.CurrentTarget.ControlState = Unit.ActControlState.None;
                _field.CurrentTarget.EndTurn();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// このオブジェクト生成時の初期化
    /// </summary>
    /// <param name="team"></param>
    public void Initialize(Unit.Team team, GameFieldData fieldData)
    {
        _targetTeam = team;
        _field = fieldData;
        _characterAI = new CharacterAI();
        _characterAI.Initialize(fieldData, _unitController);

        if (_targetTeam == Unit.Team.Player)
        {
            _assignTurn = GameFieldData.Turn.PlayerTurn;
        }
        else
        {
            _assignTurn = GameFieldData.Turn.EnemyTurn;
        }

        _neighborCells = new RaycastHit[MaxNeighborCell];
    }

    /// <summary>
    /// 戦略指示
    /// </summary>
    /// <param name="unit"></param>
    public void DecideStrategy(Unit unit, GameFieldData fieldData)
    {
        _characterAI.SetTargetUnit(unit);

        int countDiff = CalcDiffUnitsCount(unit.UnitTeam, fieldData);
        
        float hpAveDiff = CalcDiffHpAverage(unit.UnitTeam, fieldData);

        if (Mathf.Abs(countDiff) > DiffBorder || Mathf.Abs(hpAveDiff) > DiffHpAveBorder)
        {
            if (countDiff > 0)
            {
                /* 優勢 */
                _characterAI.SetStrategy(CharacterAI.Strategy.Attack, null, fieldData);
            }
            else
            {
                /* 劣勢 */
                FieldCell escapeCell;

                if (unit.UnitTeam == Unit.Team.Player)
                {
                    escapeCell = NavigationAI.InvestigateEscapePosition(SeekCenter(fieldData, Unit.Team.Enemy), SeekCenter(fieldData, Unit.Team.Player), fieldData);
                }
                else
                {
                    escapeCell = NavigationAI.InvestigateEscapePosition(SeekCenter(fieldData, Unit.Team.Player), SeekCenter(fieldData, Unit.Team.Enemy), fieldData);
                }
                _characterAI.SetStrategy(CharacterAI.Strategy.Recovery, escapeCell, fieldData);
            }
        }
        else
        {
            /* 陣地が遠い場合はそのままバランス戦略を渡す */
            _characterAI.SetStrategy(CharacterAI.Strategy.Balance, null, fieldData);
        }
    }

    /// <summary>
    /// 各ユニットを更新
    /// </summary>
    public bool ActUnits()
    {
        if (_field == null)
        {
            return false;
        }

        /* 各キャラクターAIへ戦略通知 */
        _field.CurrentTarget.ControlState = Unit.ActControlState.ThinkStrategy;

        return true;
    }

    private Vector3 CalcIntCordinateFromFloat(Vector3 cordinate)
    {
        Vector3 retCordinate = Vector3.zero;

        retCordinate.x = Mathf.Round(cordinate.x);
        retCordinate.y = Mathf.Round(cordinate.y);
        retCordinate.z = Mathf.Round(cordinate.z);

        return retCordinate;
    }

    private bool IsNeighborCenter(GameFieldData fieldData, Unit unit)
    {
        FieldCell allyCenterCell = SeekCenter(fieldData, Unit.Team.Player);
        FieldCell enemyCenterCell = SeekCenter(fieldData, Unit.Team.Enemy);

        if (unit.UnitTeam == Unit.Team.Player)
        {
            if (Vector3.SqrMagnitude(enemyCenterCell.transform.position - allyCenterCell.transform.position)
                    < Vector3.SqrMagnitude(allyCenterCell.transform.position - unit.transform.position) + Mathf.Pow(unit.Move, 2f))
            {
                return true;
            }
        }
        else
        {
            if (Vector3.SqrMagnitude(enemyCenterCell.transform.position - allyCenterCell.transform.position)
                    < Vector3.SqrMagnitude(enemyCenterCell.transform.position - unit.transform.position) + Mathf.Pow(unit.Move, 2f))
            {
                return true;
            }
        }

        return false;
    }
    private Vector3 CalcCenterCordinate(List<Unit> units)
    {
        Vector3 centerCordinate = Vector3.zero;

        if (units == null)
        {
            return centerCordinate;
        }
        if (units.Count == 0)
        {
            return centerCordinate;
        }

        foreach (var unit in units)
        {
            if (unit.OnCell != null)
            {
                centerCordinate += unit.OnCell.transform.position;
            }
        }
        centerCordinate = centerCordinate / units.Count;
        centerCordinate = CalcIntCordinateFromFloat(centerCordinate);

        return centerCordinate;
    }

    public FieldCell GetNeighborCell(Vector3 position, float groundCheckRadius)
    {
        FieldCell retCell = null;

        if (_neighborCells == null)
        {
            return null;
        }

        Physics.SphereCastNonAlloc(position, groundCheckRadius, Vector3.forward, _neighborCells);

        foreach (var hitObj in _neighborCells)
        {
            if (hitObj.collider.gameObject.CompareTag(FieldCell.ObjTag))
            {
                retCell = hitObj.collider.gameObject.GetComponent<FieldCell>();
            }
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

        foreach(var unit in units)
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
        FieldCell retCell = null;
        Vector3 center;
        if (team == Unit.Team.Player)
        {
            center = CalcCenterCordinate(fieldData.PlayerUnits);
        }
        else
        {
            center = CalcCenterCordinate(fieldData.EnemyUnits);
        }

        if (fieldData.FieldCells.ContainsKey(center))
        {
            retCell = fieldData.FieldCells[center];
        }
        else
        {
            /* 重心からrayを出して、rayに当たるFieldCellを探す */
            retCell = GetNeighborCell(center, SearchRadious);
        }


        return retCell;
    }
}
