using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager instance => _instance;

    [LabelText("UI相机")]
    public Camera uiCamera;

    [LabelText("下落按钮")]
    public Button dropButton;

    private void Awake()
    {
        _instance = this;
        AddEventListeners();
    }

    private void OnDestroy()
    {
        _instance = null;
        RemoveEventListeners();
    }

    public void AddEventListeners()
    {
        dropButton.onClick.AddListener(OnDropButtonClick);
    }

    public void RemoveEventListeners()
    {
        dropButton.onClick.RemoveAllListeners();
    }

    public void OnDropButtonClick()
    {
        GameManager.instance.DropActiveBlock();
    }
}
