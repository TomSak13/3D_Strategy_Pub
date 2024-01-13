using System.Collections.Generic;
using UnityEngine;
public class InputReceiver : MonoBehaviour
{
    [SerializeField] private GameFieldData _field;
    [SerializeField] private TargetPanel _cursorObject;
    /* UI関連 */
    [SerializeField] private UIPresenter _uiPresenter;

    private List<CharacterAI.Strategy> _selectStartegies;

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

    public void UpdateLatestCameraDistance(ref MainCamera.DistanceParam distanceParam, float cameraAngleSensitivity)
    {
        /* マウスホイールで注視オブジェクトとの距離更新 */
        distanceParam.DistanceCamSq -= Input.GetAxis("Mouse ScrollWheel");

        /* 右クリックを押したままマウスを動かした場合に視点移動 */
        if (Input.GetMouseButton(1))
        {
            distanceParam.XAngleAxis -= Input.GetAxis("Mouse X") * cameraAngleSensitivity; /* x軸方向の移動量 */
            distanceParam.YAngleAxis -= Input.GetAxis("Mouse Y") * cameraAngleSensitivity; /* y軸方向の移動量 */
        }
    }

    public bool IsInputStrategy()
    {
        if (_selectStartegies == null)
        {
            return false;
        }

        if (_selectStartegies.Count <= 0)
        {
            return false;
        }

        return true;
    }

    public CharacterAI.Strategy PopNextStrategy()
    {
        if (_selectStartegies == null)
        {
            return CharacterAI.Strategy.Defense; // エラー処理
        }
        if (_selectStartegies.Count <= 0)
        {
            return CharacterAI.Strategy.Defense; // エラー処理
        }

        CharacterAI.Strategy strategy = _selectStartegies[0];
        _selectStartegies.RemoveAt(0);
        return strategy;
    }

    public void ReceiveOnButton(UIPresenter.ActionIndex actionIndex)
    {
        CharacterAI.Strategy selectStrategy;

        if (_selectStartegies == null)
        {
            _selectStartegies = new List<CharacterAI.Strategy>();
        }

        switch(actionIndex)
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
                break;
        }

        _selectStartegies.Add(selectStrategy);
    }

    private void InputKeyInViewMode()
    {
        if (_cursorObject == null)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.UpArrow,_field);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.DownArrow, _field);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.LeftArrow, _field);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _cursorObject.MoveNeighborFocusCell(KeyCode.RightArrow, _field);
        }
    }

    private bool IsInputChangeGameMode(GameFieldData field)
    {
        if (field == null)
        {
            return false;
        }
        // プレイヤーターンかつ、Vキーが押されたときにモード変更
        if (Input.GetKeyDown(KeyCode.V) && field.CurrentTurn == GameFieldData.Turn.PlayerTurn)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeGameMode(GameFieldData field, UIPresenter uiPresenter)
    {
        if (field.CurrentMode == GameFieldData.Mode.Game)
        {
            uiPresenter.UnDispActionPanel();
            uiPresenter.DispViewModeObj();
            field.CurrentMode = GameFieldData.Mode.View;
        }
        else
        {
            uiPresenter.DispActionPanel();
            uiPresenter.UnDispViewModeObj();
            _cursorObject.MoveFocusCell(field.CurrentTarget.OnCell); // 次に行動予定のユニットへフォーカスを戻す
            _uiPresenter.UpdateCharacterParam(_cursorObject.FocusCell.OnUnitData);
            field.CurrentMode = GameFieldData.Mode.Game;
        }
    }
}
