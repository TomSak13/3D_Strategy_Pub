using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterAI
{
    public enum Strategy /* 選択戦略 */
    {
        Attack,
        Defense,
        Balance,
        Recovery
    }

    public const float HpRatioEscape = 0.3f;

    private Unit _targetUnit;
    private GameFieldData _field;
    private Sensor _sensor;
    private UnitController _unitController;
    private List<ActionBase> _actions;

    private AttackAction _attackAction;
    private MoveAction _moveAction;
    private DefenseAction _defenseAction;

    public void Initialize(GameFieldData field, UnitController unitController)
    {
        _sensor = new Sensor();

        _sensor.Initialize(field);
        _field = field;
        _unitController = unitController;

        _targetUnit = null;

        _actions = new List<ActionBase>();
    }

    public Unit SeekNeighborEnemy(Unit unit)
    {
        if (_sensor == null)
        {
            return null;
        }

        return _sensor.SeekNeighborEnemy(unit);
    }

    private void SetupAttackStrategy(List<Unit> attackableUnit, GameFieldData fieldData)
    {
        if (attackableUnit.Count > 0)
        {
            /* 攻撃 */
            _attackAction = new AttackAction();
            _attackAction.Initialize(_targetUnit, attackableUnit[0]);
            _actions.Add(_attackAction);
        }
        else
        {
            /* 移動→攻撃 */
            FieldCell destination = null;
            Unit attackTarget = null;
            List<Unit> enemyUnits = null;
            List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            
            if (_targetUnit.UnitTeam == Unit.Team.Enemy)
            {
                enemyUnits = _field.PlayerUnits;
            }
            else
            {
                enemyUnits = _field.EnemyUnits;
            }
            Dictionary<FieldCell, Unit> attakableUnits = NavigationAI.InvestigateAttackableCellInRoutes(routes, enemyUnits, _field);

            if (attakableUnits.Count > 0)
            {
                (destination,attackTarget) = attakableUnits.First();

                _moveAction = new MoveAction();
                _moveAction.initialize(_field, _targetUnit, NavigationAI.InvestigateMovePosition(_targetUnit, destination, fieldData, routes), routes);
                _actions.Add(_moveAction);

                _attackAction = new AttackAction();
                _attackAction.Initialize(_targetUnit, attackTarget);
                _actions.Add(_attackAction);
            }
            else
            {
                attackTarget = _sensor.SeekNeighborEnemy(_targetUnit);
                destination = attackTarget.OnCell;

                _moveAction = new MoveAction();
                _moveAction.initialize(_field, _targetUnit, NavigationAI.InvestigateMovePosition(_targetUnit, destination, fieldData, routes), routes);
                _actions.Add(_moveAction);
            }
           
        }
    }

    private void SetupDefenseStrategy()
    {
        _defenseAction = new DefenseAction();
        _defenseAction.Initialize(_targetUnit);

        _actions.Add(_defenseAction);
    }

    private void SetupBalanceStrategy(List<Unit> attackableUnit, FieldCell destinationCell, GameFieldData fieldData)
    {
        if (_targetUnit.GetCurrentHpRatio() > HpRatioEscape)
        {
            if (destinationCell != null)
            {
                if (attackableUnit.Count > 0)
                {
                    /* 攻撃 → 移動 */
                    _attackAction = new AttackAction();
                    _attackAction.Initialize(_targetUnit, attackableUnit[0]);
                    _actions.Add(_attackAction);

                    List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
                    FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);

                    _moveAction = new MoveAction();
                    _moveAction.initialize(_field, _targetUnit, moveCell, routes);
                    _actions.Add(_moveAction);
                }
                else
                {
                    /* 移動 → 攻撃 */
                    List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
                    FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);

                    _moveAction = new MoveAction();
                    _moveAction.initialize(_field, _targetUnit, moveCell, routes);
                    _actions.Add(_moveAction);

                    List<Unit> afterMoveAttakableUnit = _sensor.GetAttacakbleUnitAfterMoved(_targetUnit,moveCell);

                    if (afterMoveAttakableUnit.Count > 0)
                    {
                        _attackAction = new AttackAction();
                        _attackAction.Initialize(_targetUnit, afterMoveAttakableUnit[0]);
                        _actions.Add(_attackAction);
                    }
                }
            }
            else
            {
                SetupAttackStrategy(attackableUnit, fieldData);
            }
        }
        else
        {
            /* 逃走 */
            Unit escapeTarget = _sensor.SeekNeighborEnemy(_targetUnit);
            FieldCell escapeCell = NavigationAI.InvestigateEscapePosition(_targetUnit.OnCell, escapeTarget.OnCell, fieldData);

            FieldCell moveCell;
            List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, escapeCell, fieldData, routes);

            _moveAction = new MoveAction();
            _moveAction.initialize(_field, _targetUnit, moveCell, routes);
            _actions.Add(_moveAction);

            _defenseAction = new DefenseAction();
            _defenseAction.Initialize(_targetUnit);

            _actions.Add(_defenseAction);
        }
    }

    private void SetupRecoveryStrategy(List<Unit> attackableUnit, FieldCell destinationCell, GameFieldData fieldData)
    {
        if (attackableUnit.Count > 0 && _targetUnit.GetCurrentHpRatio() > HpRatioEscape)
        {
            /* 攻撃 → 移動 */
            if (_targetUnit.GetCurrentHpRatio() > HpRatioEscape)
            {
                _attackAction = new AttackAction();
                _attackAction.Initialize(_targetUnit, attackableUnit[0]);
                _actions.Add(_attackAction);
            }
            _moveAction = new MoveAction();
            List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            if (!NavigationAI.IsMovable(routes, destinationCell))
            {
                /* 陣地へ一回の移動で移動可能でない場合に移動 */
                FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);
                _moveAction.initialize(_field, _targetUnit, moveCell, routes);
                _actions.Add(_moveAction);
            }
        }
        else
        {
            /* 移動 → 攻撃 or 防御 */
            _moveAction = new MoveAction();
            List<RouteSearchNode>[] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            if (!NavigationAI.IsMovable(routes, destinationCell))
            {
                FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);
                _moveAction.initialize(_field, _targetUnit, moveCell, routes);
                _actions.Add(_moveAction);
            }

            if (_targetUnit.GetCurrentHpRatio() > HpRatioEscape)
            {
                Unit attackTarget = _sensor.SeekNeighborEnemy(_targetUnit);
                _attackAction = new AttackAction();
                _attackAction.Initialize(_targetUnit, attackTarget);

                _actions.Add(_attackAction);
            }
            else
            {
                _defenseAction = new DefenseAction();
                _defenseAction.Initialize(_targetUnit);

                _actions.Add(_defenseAction);
            }
        }
    }

    /// <summary>
    /// これから行動させるユニットを設定する
    /// </summary>
    /// <param name="unit"></param>
    public void SetTargetUnit(Unit unit)
    {
        _targetUnit = unit;
    }

    /// <summary>
    /// これから行う行動リストを作成する
    /// </summary>
    /// <param name="starategy"></param>
    /// <param name="destinaitonCell"></param>
    /// <param name="fieldData"></param>
    public void SetStrategy(Strategy starategy, FieldCell destinaitonCell, GameFieldData fieldData)
    {
        List<Unit> attackableUnit =  _sensor.GetAttackableUnits(_targetUnit);
        _actions.Clear();

        switch (starategy)
        {
            case Strategy.Attack:
                /* 一番近い敵へ近づき、攻撃可能範囲であれば攻撃を行う */
                SetupAttackStrategy(attackableUnit, fieldData);
                break;
            case Strategy.Defense:
                /* 移動せずそのまま防御態勢を取る */
                SetupDefenseStrategy();
                break;
            case Strategy.Balance:
                /* 自身のHPを確認し行動 */
                /* 自身のHPが低い場合は、一番近い敵から遠ざかるように移動して防御態勢を取る。自身のHPが低くなければ、攻撃戦略と同じ戦略を取る */
                SetupBalanceStrategy(attackableUnit, destinaitonCell, fieldData);
                break;
            case Strategy.Recovery:
                /* 指定された位置を目標座標にして移動する。HPに余裕がある場合は攻撃 */
                SetupRecoveryStrategy(attackableUnit,destinaitonCell, fieldData);
                break;
            default:
                break;
        }
    }

    public bool ActUnit()
    {
        if (_targetUnit == null)
        {
            return false;
        }

        if (_actions.Count > 0 && _targetUnit.CurrentHp > 0)
        {
            _actions[0].Execute(_unitController);
            return true;
        }
        else
        {
            return false; /* 行動終了 */
        }
    }

    public bool IsFinishedAct()
    {
        if (_actions.Count <= 0)
        {
            Debug.Log("actions have no resource");
            return false;
        }

        if (_actions[0].IsFinishedAction(_unitController))
        {
            _actions.RemoveAt(0);
            return true;
        }
        else
        {
            return false;
        }
    }
}
