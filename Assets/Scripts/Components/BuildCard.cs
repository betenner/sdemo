using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// 建筑卡片
/// </summary>
public class BuildCard : MonoBehaviour
{
    private static readonly Color COLOR_DARK_GREEN = new Color(0f, 0.5f, 0f);
    private static readonly Color COLOR_DARK_RED = new Color(0.5f, 0f, 0f);

    [Title("配置")]
    [LabelText("ID")]
    public int id;

    [LabelText("名称"), OnValueChanged("UpdateName")]
    public string buildName;

    [LabelText("最大等级"), Range(1, 6), OnValueChanged("@UpdateLevel(true)")]
    public int maxLevel = 3;

    [LabelText("当前等级"), Min(0), OnValueChanged("@UpdateLevel(true)")]
    public int level = 0;

    [LabelText("各等级缩放")]
    public Vector3[] levelScale;

    [LabelText("各等级花费")]
    public int[] levelCost;

    [Title("引用")]
    [LabelText("建筑对象")]
    public GameObject buildingObj;

    [LabelText("等级节点")]
    public ToggleNode[] levelNodes;

    [LabelText("名称文本")]
    public TextMeshProUGUI nameText;

    [LabelText("购买节点")]
    public GameObject buyNode;

    [LabelText("满级节点")]
    public GameObject maxNode;

    [LabelText("花费文本")]
    public TextMeshProUGUI costText;

    public Action<BuildCard> onClick;

    private Button _button;

    public long cost { get; private set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button)
        {
            _button.onClick.AddListener(OnClick);
        }
        UpdateButton();
    }

    private void OnDestroy()
    {
        if (_button) _button.onClick.RemoveAllListeners();
    }

    private void OnClick()
    {
        onClick?.Invoke(this);
    }

    public void UpdateCost()
    {
        if (level >= maxLevel)
        {
            maxNode.SetActive(true);
            buyNode.SetActive(false);
            return;
        }
        maxNode.SetActive(false);
        buyNode.SetActive(true);
        cost = 1L;
        if (levelCost != null && levelCost.Length > level)
        {
            cost = levelCost[level];
        }
        costText.text = Utils.ConvertToKMBT(cost);
        if (Application.isPlaying)
        {
            costText.color = GameManager.instance.coin >= cost ? COLOR_DARK_GREEN : COLOR_DARK_RED;
        }
    }

    public Vector3 GetScale()
    {
        var scale = Vector3.one;
        if (levelScale != null && levelScale.Length > level)
        {
            scale = levelScale[level];
        }
        return scale;
    }

    public void UpdateScale()
    {
        if (buildingObj) buildingObj.transform.localScale = GetScale();
    }

    private bool _updatingLevel = false;
    public void UpdateLevel(bool updateScale = true)
    {
        if (_updatingLevel) return;
        _updatingLevel = true;
        if (levelNodes == null)
        {
            maxLevel = 0;
        }
        maxLevel = Mathf.Max(maxLevel, 0);
        maxLevel = Mathf.Min(levelNodes.Length, maxLevel);
        level = Mathf.Min(level, maxLevel);
        for (int i = 0; i < levelNodes.Length; i++)
        {
            levelNodes[i].gameObject.SetActive(i < maxLevel);
            levelNodes[i].on = i < level;
            levelNodes[i].UpdateState();
        }
        if (Application.isPlaying)
        {
            GameManager.instance.UpdateLevel();
        }
        UpdateCost();
        if (updateScale) UpdateScale();
        UpdateButton();
        _updatingLevel = false;
    }

    public void UpdateName()
    {
        nameText.text = buildName;
    }

    public void UpdateButton()
    {
        if (_button) _button.interactable = level < maxLevel;
    }

    public void UpdateInfo()
    {
        UpdateLevel();
        UpdateName();
    }
}
