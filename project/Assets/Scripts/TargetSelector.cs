using System.Collections.Generic;

public class TargetSelector
{
    private TurnChanger _turnChanger = default!;

    /* UI関連 */
    private UIPresenter _uiPresenter = default!;
    private TargetPanel _targetPanel = default!;
    private InputReceiver _inputReceiver = default!;

    private StrategyAI _strategyAI = default!;
    private InputStrategyShaper _input = default!;

    private List<Unit> _units = default!;
    private GameFieldData _field = default!;

    public TargetSelector(TurnChanger turnChanger, UIPresenter uiPresenter, TargetPanel targetPanel, 
        InputReceiver inputReceiver, GameFieldData fieldData, UnitController unitController)
    {
        _turnChanger = turnChanger;
        _uiPresenter = uiPresenter;
        _targetPanel = targetPanel;
        _inputReceiver = inputReceiver;

        _field = fieldData;

        _strategyAI = new StrategyAI(Unit.Team.Enemy, fieldData, unitController);
        _input = new InputStrategyShaper(fieldData, GameFieldData.Turn.PlayerTurn, unitController);
        _inputReceiver.Initialize(_input);

        _strategyAI.UnitActFinishedEvent += OnActFinished; /* ユニットの行動が終了した際にOnActFinishedが呼ばれる事で同期 */
        _input.UnitActFinishedEvent += OnActFinished;

        _turnChanger.TurnChangeFinishedEvent += OnTurnAnimationFinished;
    }

    private void SelectNextTarget()
    {
        if (!SetCurrentTarget())
        {
            _uiPresenter.UnDisplayCharacterParam();
            EndTurn();
        }
        else
        {
            _targetPanel.MoveFocusCell(_field!.CurrentTarget.OnCell);
            if (_field.CurrentTarget.UnitTeam == Unit.Team.Player)
            {
                /* 入力待ちへ */
                _uiPresenter.UpdateCharacterParam(_field.CurrentTarget);
                _uiPresenter.DisplayActionPanel();
                _input.StartWaitInput();

            }
            else
            {
                _uiPresenter.UpdateCharacterParam(_field.CurrentTarget);
                _strategyAI.ActUnits();
            }
        }
    }

    public void OnDestroy()
    {
        _input.Dispose();
    }

    /// <summary>
    /// ターン開始時にユニットリストを更新
    /// </summary>
    private void InitializeTurn()
    {
        if (_field.CurrentTurn == GameFieldData.Turn.PlayerTurn)
        {
            _units = _field.PlayerUnits;
        }
        else
        {
            _units = _field.EnemyUnits;
        }

        // 各ユニットのターン前初期化
        foreach (var unit in _units)
        {
            unit.InitializeTurn();
        }
    }

    private bool SetCurrentTarget()
    {
        bool ret = false;

        foreach (var unit in _units)
        {
            if (!unit.IsTurnFinished && unit.CurrentHp > 0)
            {
                _field.CurrentTarget = unit;
                ret = true;
            }
        }

        return ret;
    }

    private void EndTurn()
    {
        _turnChanger.ReceiveEndTurn(_field.CurrentTurn);
    }

    private void OnTurnAnimationFinished()
    {
        InitializeTurn();
        SelectNextTarget();
    }

    private void OnActFinished()
    {
        _field.DeathUnits();
        if (_field.EnemyUnits.Count == 0 || _field.PlayerUnits.Count == 0)
        {
            return; /* game終了のため、次に行動するユニットはない */
        }

        SelectNextTarget();
    }
}
