using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleDataPresenter : MonoBehaviour
{
    

    /* model */
    [SerializeField] private TitleData titleData;

    /* view */
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private GameObject _gameStartSelectedMarker;
    [SerializeField] private Button _difficultySettingButton;
    [SerializeField] private GameObject _difficultySettingSelectedMarker;

    [SerializeField] private Button _difficultyEasyButton;
    [SerializeField] private GameObject _difficultyEasySelectedMarker;
    [SerializeField] private Button _difficultyNormalButton;
    [SerializeField] private GameObject _difficultyNormalSelectedMarker;
    [SerializeField] private Button _difficultyDifficultButton;
    [SerializeField] private GameObject _difficultyDifficultSelectedMarker;

    [SerializeField] private TMP_Dropdown _unitNumDropDown;

    [SerializeField] private GameObject _difficultyPanel;
    private Dictionary<Button, GameObject> _menu;

    [SerializeField] private SceneChanger _sceneChanger;

    // Start is called before the first frame update
    private void Start()
    {
        if (titleData != null)
        {
            titleData.TitleDataChanged += OnTitleDatahChanged;
        }

        _menu = new Dictionary<Button, GameObject>
        { 
            {_gameStartButton, _gameStartSelectedMarker},
            {_difficultySettingButton, _difficultySettingSelectedMarker},
            {_difficultyEasyButton, _difficultyEasySelectedMarker},
            {_difficultyNormalButton, _difficultyNormalSelectedMarker},
            {_difficultyDifficultButton, _difficultyDifficultSelectedMarker}
        };

        if (_difficultyPanel != null)
        {
            _difficultyPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (titleData != null)
        {
            titleData.TitleDataChanged -= OnTitleDatahChanged;
        }
    }

    private void StartGame()
    {
        if (_sceneChanger != null)
        {
            int unitNum = int.Parse(_unitNumDropDown.options[_unitNumDropDown.value].text);
            titleData.SetCommonParamUnitNum(unitNum);
            _sceneChanger.LoadGameScene();
        }
    }

    public void InputKey(KeyCode keyCode)
    {
        if (titleData == null)
        {
            return;
        }

        switch (keyCode)
        {
            case KeyCode.UpArrow:
                titleData.ReceiveInputDirection(TitleData.InputDirection.Upper);
                break;
            case KeyCode.DownArrow:
                titleData.ReceiveInputDirection(TitleData.InputDirection.Down);
                break;
            case KeyCode.LeftArrow:
                titleData.ReceiveInputDirection(TitleData.InputDirection.Left);
                break;
            case KeyCode.RightArrow:
                titleData.ReceiveInputDirection(TitleData.InputDirection.Right);
                break;
            case KeyCode.Return:
                titleData.ReceiveInputHierarchy(TitleData.InputSelect.Enter);
                break;
            case KeyCode.Escape:
                titleData.ReceiveInputHierarchy(TitleData.InputSelect.Back);
                break;
            default:
                break;
        }
    }

    public void UpdateView()
    {
        if (titleData == null)
        {
            return;
        }

        /* view更新(現在選択中の設定更新) */

        /* UI表示している階層の更新 */
        TitleData.UiHierarchy currentHierarchy = titleData.CurrentHierarchy;
        /* TODO: ここでviewのヒエラルキー更新 */
        if (currentHierarchy == TitleData.UiHierarchy.StartGame)
        {
            StartGame();
            return; /* Game Start */
        }
        else if(currentHierarchy == TitleData.UiHierarchy.Difficulty)
        {
            _difficultyPanel.SetActive(true);
        }
        else 
        {
            _difficultyPanel.SetActive(false);
        }

        /* 選択中のボタンを示すマーカーの表示 */
        TitleData.ContentCombo selectCombo = titleData.GetCurrentSelectedCombo();
        if (_menu.ContainsKey(selectCombo.ContentButton))
        {
            /* いったんすべて非表示 */
            foreach(var selectObject in _menu.Values)
            {
                if (selectObject != null)
                {
                    selectObject.SetActive(false);
                }
            }
            /* 現在選択中のものを表示 */
            _menu[selectCombo.ContentButton].SetActive(true);
        }
    }

    public void OnTitleDatahChanged()
    {
        UpdateView();
    }
}
