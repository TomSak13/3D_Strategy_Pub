using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleDataPresenter : MonoBehaviour
{
    /// <summary>
    /// model
    /// </summary>
    [SerializeField] private TitleData titleData = default!;

    /// <summary>
    /// view
    /// </summary>
    [SerializeField] private Button _gameStartButton = default!;
    [SerializeField] private GameObject _gameStartSelectedMarker = default!;
    [SerializeField] private Button _difficultySettingButton = default!;
    [SerializeField] private GameObject _difficultySettingSelectedMarker = default!;

    [SerializeField] private Button _difficultyEasyButton = default!;
    [SerializeField] private GameObject _difficultyEasySelectedMarker = default!;
    [SerializeField] private Button _difficultyNormalButton = default!;
    [SerializeField] private GameObject _difficultyNormalSelectedMarker = default!;
    [SerializeField] private Button _difficultyDifficultButton = default!;
    [SerializeField] private GameObject _difficultyDifficultSelectedMarker = default!;

    [SerializeField] private TMP_Dropdown _unitNumDropDown = default!;

    [SerializeField] private GameObject _difficultyPanel = default!;
    private Dictionary<Button, GameObject> _menu = default!;

    private void Start()
    {
        if (titleData != null)
        {
            titleData.TitleDataChanged += OnTitleDataChanged;
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
            titleData.TitleDataChanged -= OnTitleDataChanged;
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
        if (currentHierarchy == TitleData.UiHierarchy.StartGame)
        {
            int unitNum = int.Parse(_unitNumDropDown.options[_unitNumDropDown.value].text);
            titleData.StartGame(unitNum);
            return; /* Game Start */
        }
        else if (currentHierarchy == TitleData.UiHierarchy.Difficulty)
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
            foreach (var selectObject in _menu.Values)
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

    public void OnTitleDataChanged()
    {
        UpdateView();
    }
}
