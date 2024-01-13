using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ターン制御用 Presenter(Controller)
/// </summary>
public class TurnChanger : MonoBehaviour
{
    [SerializeField] private UIPresenter _uiPresenter;

    private GameFieldData _field;
    private bool _isTurnChanged;

    public event Action TurnChangeFinishedEvent;

    private void Update()
    {
        if (_field != null)
        {
            if (_field.PlayerUnits.Count == 0 || _field.EnemyUnits.Count == 0)
            {
                return; /* ゲーム終了。終了処理はMetaAIが行う */
            }
        }

        if (_isTurnChanged)
        {
            if (_field.CurrentTurn == GameFieldData.Turn.PlayerTurn)
            {
                _field.CurrentTurn = GameFieldData.Turn.EnemyTurn;
            }
            else
            {
                _field.CurrentTurn = GameFieldData.Turn.PlayerTurn;
            }
            _isTurnChanged = false;
            StartCoroutine(TurnStart(_field.CurrentTurn));
        }
    }

    IEnumerator TurnStart(GameFieldData.Turn turn)
    {
        _uiPresenter.SetTurnStartText(turn);

        yield return new WaitForSeconds(1);

        _uiPresenter.UnDispTurnText();

        TurnChangeFinishedEvent.Invoke();
    }

    IEnumerator TurnEnd(GameFieldData.Turn turn)
    {
        _uiPresenter.SetTurnEndText(turn);

        yield return new WaitForSeconds(1);

        _uiPresenter.UnDispTurnText();

        ChangeTurn();
    }

    public void Initialize(GameFieldData field)
    {
        _isTurnChanged = false;
        _field = field;
    }

    public void ReceiveEndTurn(GameFieldData.Turn turn)
    {
        StartCoroutine(TurnEnd(turn));
    }

    public void SendStartTurn()
    {
        /* ゲーム開始時のみ呼ばれる。初期状態は敵ターンとなっており、ChangeTurnで味方ターンに変ええることでターンを開始 */
        _isTurnChanged = true;
    }

    private void ChangeTurn()
    {
        _isTurnChanged = true;
    }
}
