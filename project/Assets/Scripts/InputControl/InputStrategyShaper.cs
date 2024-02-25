using System;
using UniRx;
using UnityEngine;

public class InputStrategyShaper : IDisposable
{
    private CharacterAI _characterAI;
    private GameFieldData _field;

    private GameFieldData.Turn _assignTurn;

    private bool IsAvailableInput;

    public event Action UnitActFinishedEvent = default!;

    // inputReceiverから受け取った戦略をもらうためのSubject
    public readonly Subject<CharacterAI.Strategy> InputStrategyNotifySubject = new Subject<CharacterAI.Strategy>();

    public InputStrategyShaper(GameFieldData fieldData, GameFieldData.Turn assignTurn, UnitController unitController)
    {
        _field = fieldData;
        _assignTurn = assignTurn;
        _characterAI = new CharacterAI(fieldData, unitController);

        IsAvailableInput = false;

        InputStrategyNotifySubject.Subscribe(
            strategy =>
            {
                if (!IsAvailableInput)
                {
                    return;
                }

                IsAvailableInput = false;

                FieldCell destinationCell = null!;
                switch (strategy)
                {
                    case CharacterAI.Strategy.Attack:
                    case CharacterAI.Strategy.Defense:
                    case CharacterAI.Strategy.Balance:
                        break;
                    case CharacterAI.Strategy.Recovery:
                        Unit neighborEnemy = _characterAI.SeekNeighborEnemy(_field.CurrentTarget);
                        destinationCell = NavigationAI.InvestigateEscapePosition(_field.CurrentTarget.OnCell, neighborEnemy.OnCell, _field);
                        break;
                    default:
                        break;
                }
                _characterAI.SetStrategy(strategy, destinationCell, _field);
                _characterAI.Act(this); // CharacterAIに行動開始通知
            },

            error =>
            {
                Debug.LogError("Error");
            },
            () =>
            {
                
            });
    }

    public void StartWaitInput()
    {
        _characterAI.TargetUnit = _field.CurrentTarget;
        IsAvailableInput = true;
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

    public void Dispose()
    {
        InputStrategyNotifySubject.Dispose();
    }
}
