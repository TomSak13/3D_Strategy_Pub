using System.Collections.Generic;
using UnityEngine;


    public class TitleInputReceiver : MonoBehaviour
    {
        [SerializeField] private TitleDataPresenter titlePresenter;


        private List<KeyCode> _asignKeys;

        private void Start()
        {
            _asignKeys = new List<KeyCode>()
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
                foreach (var inputAsignKey in _asignKeys)
                {
                    if (Input.GetKeyDown(inputAsignKey))
                    {
                        titlePresenter.InputKey(inputAsignKey);
                    }
                }
            }
        }
    }
