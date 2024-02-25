using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ターン制御用 Presenter(Controller)
/// </summary>
public class TurnChanger : MonoBehaviour
{
    [SerializeField] private UIPresenter _uiPresenter = default!;

    [SerializeField] private GameFieldData _field = default!;

    public event Action TurnChangeFinishedEvent = default!;

    IEnumerator TurnStart(GameFieldData.Turn turn)
    {
        yield return new WaitForSeconds(1);

        _uiPresenter.SetTurnStartText(turn);

        yield return new WaitForSeconds(1);

        _uiPresenter.UnDisplayTurnText();

        TurnChangeFinishedEvent?.Invoke();
    }

    IEnumerator TurnEnd(GameFieldData.Turn turn)
    {
        _uiPresenter.SetTurnEndText(turn);

        yield return new WaitForSeconds(1);

        _uiPresenter.UnDisplayTurnText();

        ChangeTurn();

        yield return StartCoroutine(TurnStart(_field.CurrentTurn));
    }

    public void Initialize(GameFieldData field)
    {
        _field = field;
    }

    public void ReceiveEndTurn(GameFieldData.Turn turn)
    {
        StartCoroutine(TurnEnd(turn));
    }

    public void SendStartTurn()
    {
        _field!.CurrentTurn = GameFieldData.Turn.PlayerTurn;
        StartCoroutine(TurnStart(_field.CurrentTurn));
    }

    private void ChangeTurn()
    {
        if (_field?.CurrentTurn == GameFieldData.Turn.PlayerTurn)
        {
            _field!.CurrentTurn = GameFieldData.Turn.EnemyTurn;
        }
        else
        {
            _field!.CurrentTurn = GameFieldData.Turn.PlayerTurn;
        }
    }
}
