using System.Collections.Generic;
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
    [SerializeField] private UIData _uiData;
    [SerializeField] private GameObject _playerPanel;
    [SerializeField] private GameObject _enemyPanel;

    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private GameObject _turnObj;
    [SerializeField] private GameObject _gameFinishObj;

    [SerializeField] private GameObject _targetPanel;

    [SerializeField] private GameObject _viewModeObj;

    [SerializeField] private InputReceiver _receiver;

    private const string HpStr = "HP :";
    private const string MoveStr = "MOVE :";
    private const string PlayerTurnText = "PLAYER TURN";
    private const string EnemyTurnText = "ENEMY TURN";
    private const string TurnEndText = " FINISHED";
    private const string PlayerWinText = "PLAYER WIN!";
    private const string EnemyWinText = "ENEMY WIN!";

    private TextMeshProUGUI _playerName;
    private TextMeshProUGUI _playerHp;
    private TextMeshProUGUI _playerMove;

    private TextMeshProUGUI _enemyName;
    private TextMeshProUGUI _enemyHp;
    private TextMeshProUGUI _enemyMove;

    private TextMeshProUGUI _turnText;
    private TextMeshProUGUI _gameFinishText;

    private Dictionary<ActionIndex, Button> _actionButton;

    // Start is called before the first frame update
    private void Start()
    {
        _uiData = new UIData();

        if (_playerPanel == null || _enemyPanel == null || _turnObj == null || _gameFinishObj == null 
            || _actionPanel == null || _targetPanel == null || _viewModeObj == null)
        {
            Debug.Log("fail attach UI Object");
            return;
        }

        _uiData.Initialize();
        _uiData.UICharacterParamChanged += OnUIDataChanged;

        _playerName = _playerPanel.transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();
        _playerHp = _playerPanel.transform.Find("Hp").gameObject.GetComponent<TextMeshProUGUI>();
        _playerMove = _playerPanel.transform.Find("Move").gameObject.GetComponent<TextMeshProUGUI>();

        _enemyName = _enemyPanel.transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();
        _enemyHp = _enemyPanel.transform.Find("Hp").gameObject.GetComponent<TextMeshProUGUI>();
        _enemyMove = _enemyPanel.transform.Find("Move").gameObject.GetComponent<TextMeshProUGUI>();

        _actionButton = new Dictionary<ActionIndex, Button>();

        Button attackButton = _actionPanel.transform.Find("AttackButton").gameObject.GetComponent<Button>();
        AsigneActionButton(ActionIndex.AttackButton, attackButton);

        Button defenseButton = _actionPanel.transform.Find("DefenseButton").gameObject.GetComponent<Button>();
        AsigneActionButton(ActionIndex.DefenseButton, defenseButton);

        Button balanceButton = _actionPanel.transform.Find("BalanceButton").gameObject.GetComponent<Button>();
        AsigneActionButton(ActionIndex.BalanceButton, balanceButton);

        Button escapeButton = _actionPanel.transform.Find("EscapeButton").gameObject.GetComponent<Button>();
        AsigneActionButton(ActionIndex.EscapeButton, escapeButton);

        _turnText = _turnObj.GetComponent<TextMeshProUGUI>();
        _gameFinishText = _gameFinishObj.GetComponent<TextMeshProUGUI>();

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

    private void Update()
    {
        if (_targetPanel == null)
        {
            return;
        }
    }

    private void AsigneActionButton(ActionIndex actionIndex, Button button)
    {
        button.onClick.AddListener(() => OnClickActionButton(actionIndex));
        _actionButton.Add(actionIndex, button);
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

    public void DispActionPanel()
    {
        _actionPanel.SetActive(true);
    }
    public void UnDispActionPanel()
    {
        _actionPanel.SetActive(false);
    }

    public void DispViewModeObj()
    {
        _viewModeObj.SetActive(true);
    }

    public void UnDispViewModeObj()
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

    public void UnDispCharacterParam()
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

    public void UnDispTurnText()
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
        if (_turnObj == null || _turnText == null)
        {
            return;
        }

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
