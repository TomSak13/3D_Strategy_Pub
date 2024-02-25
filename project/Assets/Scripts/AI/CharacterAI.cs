using System.Collections.Generic;
using System.Linq;

public class CharacterAI
{
    /// <summary>
    /// 選択戦略
    /// </summary>
    public enum Strategy
    {
        Attack,
        Defense,
        Balance,
        Recovery
    }

    private const float HpRatioEscape = 0.3f;

    
    private GameFieldData _field;
    private Sensor _sensor;
    private UnitController _unitController;
    private List<ActionBase> _actions;

    public Unit TargetUnit { get => _targetUnit; set => _targetUnit = value; }

    private Unit _targetUnit = default!;
    private StrategyAI _strategyAI = default!;
    private InputStrategyShaper _input = default!;

    public CharacterAI(GameFieldData field, UnitController unitController)
    {
        _sensor = new Sensor(field);
        _field = field;
        _unitController = unitController;

        _actions = new List<ActionBase>();
    }

    public Unit SeekNeighborEnemy(Unit unit)
    {
        if (_sensor == null)
        {
            throw new System.InvalidOperationException("_sensor field is null.");
        }

        return _sensor.SeekNeighborEnemy(unit);
    }

    private bool IsEscapeHpRatio()
    {
        return _targetUnit.GetCurrentHpRatio() > HpRatioEscape;
    }

    private void SetupAttackAction(Unit attackTarget)
    {
        var attackAction = new AttackAction(_targetUnit, attackTarget);
        _actions.Add(attackAction);
    }

    private void SetupAttackStrategy(Unit[] attackableUnits, GameFieldData fieldData)
    {
        if (attackableUnits.Length > 0)
        {
            /* 攻撃 */
            SetupAttackAction(attackableUnits[0]);
        }
        else
        {
            /* 移動→攻撃 */
            FieldCell destination;
            Unit attackTarget;

            if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
            {
                /* 移動できないので待機 */
                return;
            }

            RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);

            var enemyUnits = _targetUnit.UnitTeam == Unit.Team.Enemy ? _field.PlayerUnits : _field.EnemyUnits;
            Dictionary<FieldCell, Unit> afterMovedAttackableUnits = NavigationAI.InvestigateAttackableCellInRoutes(routes, enemyUnits, _field);

            if (afterMovedAttackableUnits.Count > 0)
            {
                (destination, attackTarget) = afterMovedAttackableUnits.First();

                var moveAction = new MoveAction(_field, _targetUnit, NavigationAI.InvestigateMovePosition(_targetUnit, destination, fieldData, routes), routes);
                _actions.Add(moveAction);

                SetupAttackAction(attackTarget);
            }
            else
            {
                attackTarget = _sensor.SeekNeighborEnemy(_targetUnit);
                destination = attackTarget.OnCell;

                var moveAction = new MoveAction(_field, _targetUnit, NavigationAI.InvestigateMovePosition(_targetUnit, destination, fieldData, routes), routes);
                _actions.Add(moveAction);
            }

        }
    }

    private void SetupDefenseAction()
    {
        var defenseAction = new DefenseAction(_targetUnit);

        _actions.Add(defenseAction);
    }

    private void SetupBalanceStrategy(Unit[] attackableUnits, FieldCell destinationCell, GameFieldData fieldData)
    {
        if (IsEscapeHpRatio())
        {
            if (destinationCell != null)
            {
                if (attackableUnits.Length > 0)
                {
                    /* 攻撃 → 移動 */
                    SetupAttackAction(attackableUnits[0]);

                    if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
                    {
                        /* 移動できないので待機 */
                        return;
                    }

                    RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
                    FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);

                    var moveAction = new MoveAction(_field, _targetUnit, moveCell, routes);
                    _actions.Add(moveAction);
                }
                else
                {
                    /* 移動 → 攻撃 */
                    if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
                    {
                        /* 移動できないので待機 */
                        return;
                    }

                    RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
                    FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);

                    var moveAction = new MoveAction(_field, _targetUnit, moveCell, routes);
                    _actions.Add(moveAction);

                    Unit[] afterMoveAttakableUnit = _sensor.GetAttacakbleUnitAfterMoved(_targetUnit, moveCell);

                    if (afterMoveAttakableUnit.Length > 0)
                    {
                        SetupAttackAction(afterMoveAttakableUnit[0]);
                    }
                }
            }
            else
            {
                SetupAttackStrategy(attackableUnits.ToArray(), fieldData);
            }
        }
        else
        {
            /* 逃走 */
            Unit escapeTarget = _sensor.SeekNeighborEnemy(_targetUnit);
            FieldCell escapeCell = NavigationAI.InvestigateEscapePosition(_targetUnit.OnCell, escapeTarget.OnCell, fieldData);

            if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
            {
                /* 移動できないので待機 */
                return;
            }

            RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, escapeCell, fieldData, routes);

            var moveAction = new MoveAction(_field, _targetUnit, moveCell, routes);
            _actions.Add(moveAction);

            SetupDefenseAction();
        }
    }

    private void SetupRecoveryStrategy(Unit[] attackableUnit, FieldCell destinationCell, GameFieldData fieldData)
    {
        if (attackableUnit.Length > 0 && IsEscapeHpRatio())
        {
            /* 攻撃 → 移動 */
            if (IsEscapeHpRatio())
            {
                SetupAttackAction(attackableUnit[0]);
            }

            if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
            {
                /* 移動できないので待機 */
                return;
            }

            RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            if (!NavigationAI.IsMovable(routes, destinationCell))
            {
                /* 陣地へ一回の移動で移動可能でない場合に移動 */
                FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);
                var moveAction = new MoveAction(_field, _targetUnit, moveCell, routes);
                _actions.Add(moveAction);
            }
        }
        else
        {
            /* 移動 → 攻撃 or 防御 */
            if (!NavigationAI.IsExistMovablePosition(fieldData, _targetUnit.OnCell))
            {
                /* 移動できないので待機 */
                return;
            }

            RouteSearchNode[][] routes = NavigationAI.InvestigateMovablePosition(fieldData, _targetUnit.OnCell, _targetUnit.Move);
            if (!NavigationAI.IsMovable(routes, destinationCell))
            {
                FieldCell moveCell = NavigationAI.InvestigateMovePosition(_targetUnit, destinationCell, fieldData, routes);
                var moveAction = new MoveAction(_field, _targetUnit, moveCell, routes);
                _actions.Add(moveAction);
            }

            if (IsEscapeHpRatio())
            {
                Unit attackTarget = _sensor.SeekNeighborEnemy(_targetUnit);
                SetupAttackAction(attackTarget);
            }
            else
            {
                SetupDefenseAction();
            }
        }
    }

    /// <summary>
    /// これから行う行動リストを作成する
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="destinationCell"></param>
    /// <param name="fieldData"></param>
    public void SetStrategy(Strategy strategy, FieldCell destinationCell, GameFieldData fieldData)
    {
        if (_targetUnit == null)
        {
            return;
        }

        Unit[] attackableUnit = _sensor.GetAttackableUnits(_targetUnit);

        if (_actions.Count > 0)
        {
            _actions.Clear();
        }

        switch (strategy)
        {
            case Strategy.Attack:
                /* 一番近い敵へ近づき、攻撃可能範囲であれば攻撃を行う */
                SetupAttackStrategy(attackableUnit.ToArray(), fieldData);
                return;
            case Strategy.Defense:
                /* 移動せずそのまま防御態勢を取る */
                SetupDefenseAction();
                return;
            case Strategy.Balance:
                /* 自身のHPを確認し行動 */
                /* 自身のHPが低い場合は、一番近い敵から遠ざかるように移動して防御態勢を取る。自身のHPが低くなければ、攻撃戦略と同じ戦略を取る */
                SetupBalanceStrategy(attackableUnit.ToArray(), destinationCell, fieldData);
                return;
            case Strategy.Recovery:
                /* 指定された位置を目標座標にして移動する。HPに余裕がある場合は攻撃 */
                SetupRecoveryStrategy(attackableUnit.ToArray(), destinationCell, fieldData);
                return;
        }
    }

    public void Act(StrategyAI ai)
    {
        _strategyAI = ai;
        if (_actions.Count > 0)
        {
            _unitController.ActStart(_actions[0], this);
        }
        else
        {
            _strategyAI.FinishCurrentTargetTurn();
        }
    }

    public void Act(InputStrategyShaper input)
    {
        _input = input;
        if (_actions.Count > 0)
        {
            _unitController.ActStart(_actions[0], this);
        }
        else
        {
            _input.FinishCurrentTargetTurn();
        }
    }

    public void FinishAct(Unit unit)
    {
        if (_actions.Count > 0)
        {
            _actions?.RemoveAt(0);
        }

        if (_actions?.Count > 0 && _targetUnit.CurrentHp > 0)
        {
            _unitController.ActStart(_actions[0], this);
        }
        else
        {
            /*StrategyAIもしくはInputStrategyShaperに通知*/
            if (unit.UnitTeam == Unit.Team.Enemy)
            {
                _strategyAI.FinishCurrentTargetTurn();
            }
            else
            {
                _input.FinishCurrentTargetTurn();
            }
        }
    }
}
