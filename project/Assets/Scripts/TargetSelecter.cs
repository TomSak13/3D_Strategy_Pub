using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelecter : MonoBehaviour
{
    [SerializeField] private TurnChanger _turnChanger;
    [SerializeField] private StrategyAI _strategyAI;

    /* UI関連 */
    [SerializeField] private UIPresenter _uiPresenter;
    [SerializeField] private InputStrategyShaper _input;
    [SerializeField] private TargetPanel _targetPanel;

    private bool _isTurn;
    private bool _isInAct;

    private List<Unit> _units;
    private GameFieldData _field;

    private void Update()
    {
        if (_turnChanger == null || _uiPresenter == null || _strategyAI == null || _targetPanel == null)
        {
            Debug.Log("attach objs are none:" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            return;
        }

        if (_field != null)
        {
            if (_field.PlayerUnits.Count == 0 || _field.EnemyUnits.Count == 0)
            {
                return; /* ゲーム終了。終了処理はMetaAIが行う */
            }
        }

        if (!_isTurn || _isInAct)
        {
            return; /* ターン制御中、ユニット行動中はリターンする */
        }
        
        if (!SetCurrentTarget())
        {
            _uiPresenter.UnDispCharacterParam();
            EndTurn();
        }
        else
        {
            _targetPanel.MoveFocusCell(_field.CurrentTarget.OnCell);
            if (_field.CurrentTarget.UnitTeam == Unit.Team.Player)
            {
                /* 入力待ちへ */
                _uiPresenter.UpdateCharacterParam(_field.CurrentTarget);
                _uiPresenter.DispActionPanel();
                _input.StartWaitInput();
                _isInAct = true;
            }
            else
            {
                _strategyAI.ActUnits();
                _isInAct = true;
                _uiPresenter.UpdateCharacterParam(_field.CurrentTarget);
            }
            
        }
    }

    public void Initialize(GameFieldData fieldData)
    {
        _units = null;
        _field = fieldData;

        _isTurn = false;
        _isInAct = false;


        _strategyAI.UnitActFinishedEvent += OnActFinished; /* ユニットの行動が終了した際にOnActFinishedが呼ばれる事で同期 */
        _input.UnitActFinishedEvent += OnActFinished;

        _turnChanger.TurnChangeFinishedEvent += OnTurnAnimationFinished;
    }

    /// <summary>
    /// ターン開始時にユニットリストを更新
    /// </summary>
    public void InitializeTurn()
    {
        _field.CurrentTarget = null;

        if (_field.CurrentTurn == GameFieldData.Turn.PlayerTurn)
        {
            _units = _field.PlayerUnits;
        }
        else
        {
            _units = _field.EnemyUnits;
        }

        // 各ユニットのターン前初期化
        foreach(var unit in _units)
        {
            unit.InitializeTurn();
        }
    }

    public bool SetCurrentTarget()
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

    public void EndTurn()
    {
        _turnChanger.ReceiveEndTurn(_field.CurrentTurn);
        _isTurn = false;
    }

    public void OnTurnAnimationFinished()
    {
        InitializeTurn();
        _isTurn = true;
    }

    public void OnActFinished()
    {
        _field.DeathUnits();
        _isInAct = false;
    }
}
