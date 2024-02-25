using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TitleData : MonoBehaviour
{
    public enum SettingContents
    {
        GameStart,
        Difficulty
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Difficult
    }

    public enum InputDirection
    {
        Upper,
        Down,
        Left,
        Right
    }

    public enum InputSelect
    {
        Enter,
        Back
    }

    public enum UiHierarchy
    {
        Route,
        Difficulty,
        StartGame /* ゲームスタート遷移用 */
    }

    public class ContentCombo
    {
        public Button ContentButton = default!;
        public Dictionary<InputDirection, ContentCombo> Neighbor = default!;
        public Dictionary<InputSelect, UiHierarchy> NeighborHierarchy = default!;
    }

    public event Action TitleDataChanged = default!;

    private UiHierarchy _currentHierarchy;

    private ContentCombo _selectedContentCombo = default!;
    private ContentCombo _selectedDifficultyCombo = default!;
    private Dictionary<ContentCombo, Difficulty> _difficultyContents = default!;

    [SerializeField]
    Button _startButton = default!;
    [SerializeField]
    Button _difficultyButton = default!;
    [SerializeField]
    Button _easyButton = default!;
    [SerializeField]
    Button _normalButton = default!;
    [SerializeField]
    Button _difficultButton = default!;

    public UiHierarchy CurrentHierarchy  => _currentHierarchy;


    private void Start()
    {
        InitButton();

        _currentHierarchy = UiHierarchy.Route;
        GameSettingParam.GameDifficulty = Difficulty.Normal;
    }

    private void InitButton()
    {
        ContentCombo startCombo = CreateContentCombo(_startButton, UiHierarchy.StartGame, UiHierarchy.Route);
        ContentCombo difficultyCombo = CreateContentCombo(_difficultyButton, UiHierarchy.Difficulty, UiHierarchy.Route);
        ContentCombo easyCombo = CreateContentCombo(_easyButton, UiHierarchy.Route, UiHierarchy.Route);
        ContentCombo normalCombo = CreateContentCombo(_normalButton, UiHierarchy.Route, UiHierarchy.Route);
        ContentCombo difficultCombo = CreateContentCombo(_difficultButton, UiHierarchy.Route, UiHierarchy.Route);

        SetContentComboNeighbor(startCombo, startCombo, difficultyCombo, startCombo, startCombo);
        SetContentComboNeighbor(difficultyCombo, startCombo, difficultyCombo, difficultyCombo, difficultyCombo);

        SetContentComboNeighbor(easyCombo, easyCombo, normalCombo, easyCombo, easyCombo);
        SetContentComboNeighbor(normalCombo, easyCombo, difficultCombo, normalCombo, normalCombo);
        SetContentComboNeighbor(difficultCombo, normalCombo, difficultCombo, difficultCombo, difficultCombo);

        _difficultyContents = new Dictionary<ContentCombo, Difficulty>()
        {
            {easyCombo, Difficulty.Easy},
            {normalCombo, Difficulty.Normal},
            {difficultCombo, Difficulty.Difficult}
        };
        _selectedContentCombo = startCombo;
        _selectedDifficultyCombo = normalCombo;
    }

    private ContentCombo CreateContentCombo(Button target, UiHierarchy enter, UiHierarchy back)
    {
        ContentCombo retCombo = new ContentCombo
        {
            ContentButton = target,
            NeighborHierarchy = new Dictionary<InputSelect, UiHierarchy>()
            {
                {InputSelect.Enter, enter},
                {InputSelect.Back, back}
            }
        };

        return retCombo;
    }

    private void SetContentComboNeighbor(ContentCombo target, ContentCombo upper, ContentCombo down, ContentCombo left, ContentCombo right)
    {
        target.Neighbor = new Dictionary<InputDirection, ContentCombo>()
        {
            {InputDirection.Upper, upper},
            {InputDirection.Down, down},
            {InputDirection.Left, left},
            {InputDirection.Right, right}
        };
    }

    public ContentCombo GetCurrentSelectedCombo()
    {
        return _currentHierarchy switch
        {
            UiHierarchy.Route => _selectedContentCombo,
            UiHierarchy.Difficulty => _selectedDifficultyCombo,
            _ => throw new System.InvalidOperationException("choice no exist UiHierarchy")
        };
    }

    private void SetCommonParamUnitNum(int unitNum)
    {
        GameSettingParam.UnitNum = unitNum;
    }

    public void ReceiveInputHierarchy(InputSelect select)
    {
        switch (_currentHierarchy)
        {
            case UiHierarchy.Route:
                if (_selectedContentCombo != null)
                {
                    _selectedContentCombo.NeighborHierarchy.TryGetValue(select, out _currentHierarchy);
                }
                break;
            case UiHierarchy.Difficulty:
                if (_selectedDifficultyCombo != null)
                {
                    _selectedDifficultyCombo.NeighborHierarchy.TryGetValue(select, out _currentHierarchy);
                }
                break;
            default:
                break;
        }

        UpdateData();
    }

    /// <summary>
    /// 入力された方向に合せ、選択中のコンテンツを変更
    /// </summary>
    /// <param name="inputDirection"></param>
    public void ReceiveInputDirection(InputDirection inputDirection)
    {
        switch (_currentHierarchy)
        {
            case UiHierarchy.Route:
                if (_selectedContentCombo != null)
                {
                    if (_selectedContentCombo.Neighbor.ContainsKey(inputDirection))
                    {
                        _selectedContentCombo = _selectedContentCombo.Neighbor[inputDirection];
                    }
                }
                break;
            case UiHierarchy.Difficulty:
                if (_selectedDifficultyCombo != null)
                {
                    if (_selectedDifficultyCombo.Neighbor.ContainsKey(inputDirection))
                    {
                        _selectedDifficultyCombo = _selectedDifficultyCombo.Neighbor[inputDirection];
                        GameSettingParam.GameDifficulty = _difficultyContents[_selectedDifficultyCombo]; /* 現在選択中の難易度を設定 */
                    }
                }
                break;
            default:
                break;
        }

        UpdateData();
    }

    public void UpdateData()
    {
        TitleDataChanged?.Invoke();
    }

    public void StartGame(int unitNum)
    {
        SetCommonParamUnitNum(unitNum);
        SceneChanger.LoadGameScene(SceneChanger.GameScene);
    }
}
