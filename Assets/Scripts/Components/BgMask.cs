using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BgMask : MonoBehaviour
{
    [LabelText("点击关闭")]
    public bool clickToClose = true;

    [LabelText("关闭目标")]
    public GameObject closeTarget;

    public Action onClose;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (!_button)
        {
            _button = gameObject.AddComponent<Button>();
            _button.transition = Selectable.Transition.None;
        }
        _button.interactable = clickToClose;
        if (!closeTarget && transform.parent) closeTarget = transform.parent.gameObject;
        _button.onClick.AddListener(OnCloseClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    private void OnCloseClick()
    {
        if (clickToClose && closeTarget) closeTarget.SetActive(false);
    }
}
