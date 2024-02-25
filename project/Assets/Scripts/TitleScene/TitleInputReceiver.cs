using System.Collections.Generic;
using UnityEngine;

public class TitleInputReceiver : MonoBehaviour
{
    [SerializeField] private TitleDataPresenter titlePresenter = default!;

    private List<KeyCode> _assignKeys = default!;

    private void Start()
    {
        _assignKeys = new List<KeyCode>()
    {
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.Return,
        KeyCode.Escape
    };
    }

    private void Update()
    {
        if (titlePresenter == null)
        {
            return;
        }

        if (Input.anyKeyDown)
        {
            foreach (var inputAssignKey in _assignKeys)
            {
                if (Input.GetKeyDown(inputAssignKey))
                {
                    titlePresenter.InputKey(inputAssignKey);
                }
            }
        }
    }
}
