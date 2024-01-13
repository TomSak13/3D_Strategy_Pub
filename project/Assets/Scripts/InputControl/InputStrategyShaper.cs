using System;
using System.Collections.Generic;
using UnityEngine;

public class InputStrategyShaper : MonoBehaviour
{
    [SerializeField] private UnitController _unitController;
    [SerializeField] private NavigationAI _navigationAI;
    private CharacterAI _characterAI;
    [SerializeField] private InputReceiver _receiver;
    private GameFieldData _field;

    private GameFieldData.Turn _assignTurn;

    public event Action UnitActFinishedEvent;

    private void Update()
    {
        if (_receiver == null || _characterAI == null)
        {
            return;
        }
        if (_field == null)
        {
            return;
        }

        if (_field.CurrentTarget == null 
            || _field.CurrentTurn != _assignTurn)
        {
            return;
        }

        if(_receiver.IsInputStrategy() && _field.CurrentTarget.ControlState == Unit.ActControlState.None)
        {
            _field.CurrentTarget.ControlState = Unit.ActControlState.ThinkStrategy;
        }

        switch (_field.CurrentTarget.ControlState)
        {
            case Unit.ActControlState.None:
                break;
            case Unit.ActControlState.ThinkStrategy:
                ConvertInputToStrategy();
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
                NotifyInputActionFinished();
                _field.CurrentTarget.ControlState = Unit.ActControlState.None;
                _field.CurrentTarget.EndTurn();
                break;
            default:
                break;
        }
    }

    public void Initialize(GameFieldData fieldData, GameFieldData.Turn assignTurn)
    {
        _characterAI = new CharacterAI();
        _characterAI.Initialize(fieldData, _unitController);

        _field = fieldData;

        _assignTurn = assignTurn;
    }

    public void StartWaitInput()
    {
        _characterAI.SetTargetUnit(_field.CurrentTarget);
    }

    private void ConvertInputToStrategy()
    {
        FieldCell destinationCell = null;

        CharacterAI.Strategy nextStrategy = _receiver.PopNextStrategy();

        switch (nextStrategy)
        {
            case CharacterAI.Strategy.Attack:
            case CharacterAI.Strategy.Defense:
            case CharacterAI.Strategy.Balance:
                break;
            case CharacterAI.Strategy.Recovery:
                Unit NeighborEnemy = _characterAI.SeekNeighborEnemy(_field.CurrentTarget);
                destinationCell = NavigationAI.InvestigateEscapePosition(_field.CurrentTarget.OnCell, NeighborEnemy.OnCell, _field);
                break;
            default:
                break;
        }
        _characterAI.SetStrategy(nextStrategy, destinationCell, _field);
    }

    public void NotifyInputActionFinished()
    {
        UnitActFinishedEvent.Invoke(); // 入力待ち＋行動終了待ちの終了通知
    }
}
