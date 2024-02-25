using UnityEngine;
public class InputReceiver : MonoBehaviour
{
    [SerializeField] private GameFieldData _field = default!;
    [SerializeField] private TargetPanel _cursorObject = default!;
    /// <summary>
    /// UI用
    /// </summary>
    [SerializeField] private UIPresenter _uiPresenter = default!;

    private InputStrategyShaper _inputStrategyShaper = default!;

    private void Update()
    {
        if (_field == null || _uiPresenter == null)
        {
            Debug.Log("field data or _uiPresenter is null");
            return;
        }
        /* mode監視 */
        if (IsInputChangeGameMode(_field))
        {
            ChangeGameMode(_field, _uiPresenter);
        }

        /* viewモード用キー操作 */
        if (_field.CurrentMode == GameFieldData.Mode.View)
        {
            InputKeyInViewMode();
            _uiPresenter.UpdateCharacterParam(_cursorObject.FocusCell.OnUnitData);
        }
    }

    public void OnDestroy()
    {
        _inputStrategyShaper.Dispose();
    }

    public void Initialize(InputStrategyShaper inputStrategyShaper)
    {
        _inputStrategyShaper = inputStrategyShaper;
    }

    public (float distanceCamSqDiff, Vector2 angleAxisDiff) GetInputMouseParam()
    {
        float distanceCamSq;
        /* マウスホイールで注視オブジェクトとの距離更新 */
        distanceCamSq = Input.GetAxis("Mouse ScrollWheel");

        /* 右クリックを押したままマウスを動かした場合に視点移動 */
        Vector2 angleAxis = Vector2.zero;
        if (Input.GetMouseButton(1))
        {
            angleAxis.x = Input.GetAxis("Mouse X"); /* x軸方向の移動量 */
            angleAxis.y = Input.GetAxis("Mouse Y"); /* y軸方向の移動量 */
        }

        return (distanceCamSq, angleAxis);
    }

    public void ReceiveOnButton(UIPresenter.ActionIndex actionIndex)
    {
        CharacterAI.Strategy selectStrategy;

        switch (actionIndex)
        {
            case UIPresenter.ActionIndex.AttackButton:
                selectStrategy = CharacterAI.Strategy.Attack;
                break;
            case UIPresenter.ActionIndex.DefenseButton:
                selectStrategy = CharacterAI.Strategy.Defense;
                break;
            case UIPresenter.ActionIndex.BalanceButton:
                selectStrategy = CharacterAI.Strategy.Balance;
                break;
            case UIPresenter.ActionIndex.EscapeButton:
                selectStrategy = CharacterAI.Strategy.Recovery;
                break;
            default:
                selectStrategy = CharacterAI.Strategy.Defense; // エラー処理。ここには来ない想定
                Debug.LogError("Unexpected Input");
                break;
        }

        _inputStrategyShaper.InputStrategyNotifySubject.OnNext(selectStrategy);
    }

    private void InputKeyInViewMode()
    {
        if (_cursorObject == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.UpArrow);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.DownArrow);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.LeftArrow);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.RightArrow);
        }
    }

    private bool IsInputChangeGameMode(GameFieldData field)
    {
        if (field == null)
        {
            return false;
        }
        // プレイヤーターンかつ、Vキーが押されたときにモード変更
        return Input.GetKeyDown(KeyCode.V) && field.CurrentTurn == GameFieldData.Turn.PlayerTurn;
    }

    private void ChangeGameMode(GameFieldData field, UIPresenter uiPresenter)
    {
        if (field.CurrentMode == GameFieldData.Mode.Game)
        {
            uiPresenter.UnDisplayActionPanel();
            uiPresenter.DisplayViewModeObj();
            field.CurrentMode = GameFieldData.Mode.View;
        }
        else
        {
            uiPresenter.DisplayActionPanel();
            uiPresenter.UnDisplayViewModeObj();
            _cursorObject.MoveFocusCell(field.CurrentTarget.OnCell); // 次に行動予定のユニットへフォーカスを戻す
            uiPresenter.UpdateCharacterParam(_cursorObject.FocusCell.OnUnitData);
            field.CurrentMode = GameFieldData.Mode.Game;
        }
    }
}
