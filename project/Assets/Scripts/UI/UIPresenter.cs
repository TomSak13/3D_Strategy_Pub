using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPresenter : MonoBehaviour
{
    public enum ActionIndex
    {
        AttackButton = 0,
        DefenseButton,
        BalanceButton,
        EscapeButton,
        ButtonCount
    }
    
    [SerializeField] private GameObject _playerPanel = default!;
    [SerializeField] private GameObject _enemyPanel = default!;

    [SerializeField] private GameObject _actionPanel = default!;
    [SerializeField] private GameObject _turnObj = default!;
    [SerializeField] private GameObject _gameFinishObj = default!;

    [SerializeField] private GameObject _targetPanel = default!;

    [SerializeField] private GameObject _viewModeObj = default!;

    [SerializeField] private InputReceiver _receiver = default!;

    private const string HpStr = "HP :";
    private const string MoveStr = "MOVE :";
    private const string PlayerTurnText = "PLAYER TURN";
    private const string EnemyTurnText = "ENEMY TURN";
    private const string TurnEndText = " FINISHED";
    private const string PlayerWinText = "PLAYER WIN!";
    private const string EnemyWinText = "ENEMY WIN!";

    [SerializeField] private TextMeshProUGUI _playerName = default!;
    [SerializeField] private TextMeshProUGUI _playerHp = default!;
    [SerializeField] private TextMeshProUGUI _playerMove = default!;

    [SerializeField] private TextMeshProUGUI _enemyName = default!;
    [SerializeField] private TextMeshProUGUI _enemyHp = default!;
    [SerializeField] private TextMeshProUGUI _enemyMove = default!;

    [SerializeField] private Button _attackButton = default!;
    [SerializeField] private Button _defenseButton = default!;
    [SerializeField] private Button _balanceButton = default!;
    [SerializeField] private Button _escapeButton = default!;

    private UIData _uiData = default!;

    [SerializeField] private TextMeshProUGUI _turnText = default!;
    [SerializeField] private TextMeshProUGUI _gameFinishText = default!;

    private void Start()
    {
        _uiData = new UIData();

        if (_playerPanel == null || _enemyPanel == null || _turnObj == null || _gameFinishObj == null
            || _actionPanel == null || _targetPanel == null || _viewModeObj == null)
        {
            Debug.Log("fail attach UI Object");
            return;
        }

        _uiData.UICharacterParamChanged += OnUIDataChanged;

        AssignActionButton(ActionIndex.AttackButton, _attackButton);
        AssignActionButton(ActionIndex.DefenseButton, _defenseButton);
        AssignActionButton(ActionIndex.BalanceButton, _balanceButton);
        AssignActionButton(ActionIndex.EscapeButton, _escapeButton);

        _playerName.text = "";
        _playerHp.text = HpStr + "";
        _playerMove.text = MoveStr + "";

        _enemyName.text = "";
        _enemyHp.text = HpStr + "";
        _enemyMove.text = MoveStr + "";

        _playerPanel.SetActive(false);
        _enemyPanel.SetActive(false);
        _turnObj.SetActive(false);
        _actionPanel.SetActive(false);
        _gameFinishObj.SetActive(false);
        _viewModeObj.SetActive(false);
    }

    private void AssignActionButton(ActionIndex actionIndex, Button button)
    {
        button.onClick.AddListener(() => OnClickActionButton(actionIndex));
    }

    private void OnClickActionButton(ActionIndex buttonKey)
    {
        if (_receiver == null)
        {
            return;
        }

        _actionPanel.SetActive(false);
        _receiver.ReceiveOnButton(buttonKey);
    }

    public void DisplayActionPanel()
    {
        _actionPanel.SetActive(true);
    }
    public void UnDisplayActionPanel()
    {
        _actionPanel.SetActive(false);
    }

    public void DisplayViewModeObj()
    {
        _viewModeObj.SetActive(true);
    }

    public void UnDisplayViewModeObj()
    {
        _viewModeObj.SetActive(false);
    }

    public void UpdateCharacterParam(Unit unit)
    {
        if (unit == null)
        {
            _playerPanel.SetActive(false);
            _enemyPanel.SetActive(false);
            return;
        }

        _uiData.UpdateCharacterParam(unit);
        if (unit.UnitTeam == Unit.Team.Player)
        {
            _playerPanel.SetActive(true);
            _enemyPanel.SetActive(false);
        }
        else
        {
            _playerPanel.SetActive(false);
            _enemyPanel.SetActive(true);
        }
    }

    public void UpdateBattleResult(Unit attackUnit, Unit defUnit)
    {
        _uiData.UpdateCharacterParam(attackUnit);
        _uiData.UpdateCharacterParam(defUnit);

        _playerPanel.SetActive(true);
        _enemyPanel.SetActive(true);
    }

    public void UnDisplayCharacterParam()
    {
        _playerPanel.SetActive(false);
        _enemyPanel.SetActive(false);
    }

    public void UpdateView()
    {
        if (_uiData == null || _playerName == null || _playerHp == null || _playerMove == null
            || _enemyName == null || _enemyHp == null || _enemyMove == null)
        {
            return;
        }

        _playerName.text = _uiData.PlayerCharacter.Name;
        _playerHp.text = HpStr + _uiData.PlayerCharacter.Hp.ToString();
        _playerMove.text = MoveStr + _uiData.PlayerCharacter.Move;

        _enemyName.text = _uiData.EnemyCharacter.Name;
        _enemyHp.text = HpStr + _uiData.EnemyCharacter.Hp.ToString();
        _enemyMove.text = MoveStr + _uiData.EnemyCharacter.Move;
    }

    public void OnUIDataChanged()
    {
        if (_uiData == null)
        {
            return;
        }

        UpdateView();
    }

    public void UnDisplayTurnText()
    {
        if (_turnObj == null)
        {
            return;
        }

        _turnObj.SetActive(false);
    }

    public void SetGameFinishText(Unit.Team winTeam)
    {
        if (_gameFinishObj == null || _gameFinishText == null)
        {
            return;
        }

        if (winTeam == Unit.Team.Player)
        {
            _gameFinishText.text = PlayerWinText;
            _gameFinishText.color = Color.blue;
        }
        else
        {
            _gameFinishText.text = EnemyWinText;
            _gameFinishText.color = Color.red;
        }

        _gameFinishObj.SetActive(true);
    }

    public void SetTurnStartText(GameFieldData.Turn turn)
    {
        if (turn == GameFieldData.Turn.PlayerTurn)
        {
            _turnText.text = PlayerTurnText;
            _turnText.color = Color.blue;
        }
        else
        {
            _turnText.text = EnemyTurnText;
            _turnText.color = Color.red;
        }

        _turnObj.SetActive(true);
    }

    public void SetTurnEndText(GameFieldData.Turn turn)
    {
        if (_turnObj == null || _turnText == null)
        {
            return;
        }

        if (turn == GameFieldData.Turn.PlayerTurn)
        {
            _turnText.text = PlayerTurnText + TurnEndText;
            _turnText.color = Color.blue;
        }
        else
        {
            _turnText.text = EnemyTurnText + TurnEndText;
            _turnText.color = Color.red;
        }

        _turnObj.SetActive(true);
    }
}
