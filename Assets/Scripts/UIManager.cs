using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager instance => _instance;

    [LabelText("UI相机")]
    public Camera uiCamera;

    [LabelText("主层级")]
    public Transform mainLayer;

    [LabelText("特效层级")]
    public Transform fxLayer;

    [LabelText("弹窗层级")]
    public Transform popupLayer;

    [LabelText("盖楼界面")]
    public GameObject blockUI;

    [LabelText("城建界面")]
    public GameObject buildUI;

    [LabelText("城建按钮")]
    public Button buildButton;

    [LabelText("下落按钮")]
    public Button dropButton;

    [LabelText("倍率按钮")]
    public Button betButton;

    [LabelText("主等级文本")]
    public TextMeshProUGUI lvText;

    [LabelText("金币文本")]
    public TextMeshProUGUI coinText;

    [LabelText("体力文本")]
    public TextMeshProUGUI staminaText;

    [LabelText("倍率文本")]
    public TextMeshProUGUI betText;

    [LabelText("弹出文本")]
    public TextMeshProUGUI popText;

    [LabelText("购买体力面板")]
    public GameObject buyStaminaPanel;

    [LabelText("购买体力按钮")]
    public Button buyStaminaButton;

    [LabelText("购买体力数量文本")]
    public TextMeshProUGUI buyStaminaAmount;

    [LabelText("购买金币面板")]
    public GameObject buyCoinPanel;

    [LabelText("购买金币按钮")]
    public Button buyCoinButton;

    [LabelText("购买金币数量文本")]
    public TextMeshProUGUI buyCoinAmount;

    [LabelText("城建返回按钮")]
    public Button buildBackButton;

    [LabelText("星星Prefab")]
    public GameObject starPrefab;

    [LabelText("星星飞行时间 (秒)")]
    public float starFlyDuration = 1f;

    [LabelText("星星飞行目标")]
    public Transform starFlyTarget;

    private void Awake()
    {
        _instance = this;
        AddEventListeners();
        GameManager.instance.InitGame();
    }

    private void OnDestroy()
    {
        _instance = null;
        RemoveEventListeners();
    }

    public void AddEventListeners()
    {
        dropButton.onClick.AddListener(OnDropButtonClick);
        betButton.onClick.AddListener(OnBetButtonClick);
        buyStaminaButton.onClick.AddListener(OnBuyStaminaButtonClick);
        buyCoinButton.onClick.AddListener(OnBuyCoinButtonClick);
        buildButton.onClick.AddListener(OnBuildButtonClick);
        buildBackButton.onClick.AddListener(OnBuildBackButtonClick);
    }

    public void RemoveEventListeners()
    {
        dropButton.onClick.RemoveAllListeners();
        betButton.onClick.RemoveAllListeners();
        buyStaminaButton.onClick.RemoveAllListeners();
        buyCoinButton.onClick.RemoveAllListeners();
        buildButton.onClick.RemoveAllListeners();
        buildBackButton.onClick.RemoveAllListeners();
    }

    public void OnDropButtonClick()
    {
        if (GameManager.instance.stamina <= 0)
        {
            buyStaminaAmount.text = $"x {GameManager.instance.initStamina}";
            buyStaminaPanel.SetActive(true);
            return;
        }
        GameManager.instance.SetStamina(GameManager.instance.stamina - GameManager.instance.bet);
        GameManager.instance.SetBet(GameManager.instance.bet);
        GameManager.instance.DropActiveBlock();
    }

    public void OnBetButtonClick()
    {
        var curBet = GameManager.instance.bet;
        var curStamina = GameManager.instance.stamina;
        var maxBet = Mathf.Min(curStamina, GameManager.instance.maxBet);
        if (curBet >= maxBet) curBet = 1;
        else curBet++;
        GameManager.instance.SetBet(curBet);
    }

    public void SetPopText(string text)
    {
        popText.text = text;
        popText.transform.DOKill();
        popText.DOKill();
        popText.transform.localScale = 0.8f * Vector3.one;
        popText.color = new Color(1f, 1f, 0f, 0f);
        popText.DOColor(Color.yellow, 0.2f).OnComplete(() =>
        {
            popText.DOColor(Color.yellow, 0.5f).OnComplete(() =>
            {
                popText.DOColor(new Color(1f, 1f, 0f, 0f), 0.3f);
            });
        });
        popText.transform.DOScale(Vector3.one, 0.2f).OnComplete(() =>
        {
            popText.transform.DOScale(Vector3.one, 0.5f).OnComplete(() =>
            {
                popText.transform.DOScale(0.3f * Vector3.one, 0.2f);
            });
        });
    }

    public void OnBuyStaminaButtonClick()
    {
        GameManager.instance.SetStamina(GameManager.instance.stamina + GameManager.instance.initStamina);
        buyStaminaPanel.SetActive(false);
    }

    public void OnBuyCoinButtonClick()
    {
        GameManager.instance.SetCoin(GameManager.instance.coin + GameManager.instance.initCoin);
        buyCoinPanel.SetActive(false);
    }

    public void OnBuildButtonClick()
    {
        GameManager.instance.SwitchToBuild();
        SwitchToBuildUI();
    }

    public void OnBuildBackButtonClick()
    {
        GameManager.instance.SwitchToBlock();
        SwitchToBlockUI();
    }

    public void DisableButtons()
    {
        dropButton.interactable = false;
        betButton.interactable = false;
        buildButton.interactable = false;
    }

    public void EnableButtons()
    {
        dropButton.interactable = true;
        betButton.interactable = GameManager.instance.stamina > 0;
        buildButton.interactable = true;
    }

    /// <summary>
    /// 切换到城建UI
    /// </summary>
    public void SwitchToBuildUI()
    {
        blockUI.SetActive(false);
        buildUI.SetActive(true);
    }

    /// <summary>
    /// 切换到盖楼UI
    /// </summary>
    public void SwitchToBlockUI()
    {
        blockUI.SetActive(true);
        buildUI.SetActive(false);
    }

    /// <summary>
    /// 飞星星
    /// </summary>
    /// <param name="fromWorld">世界起点</param>
    /// <param name="easing">缓动</param>
    public void StarFly(Transform fromWorld, Ease easing = Ease.OutCubic)
    {
        if (!starPrefab) return;
        var star = Instantiate(starPrefab);
        var fromPos = Utils.WorldToUI(fromWorld.position, GameManager.instance.mainCamera, uiCamera);
        star.transform.SetParent(fxLayer, true);
        star.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        star.transform.localScale = Vector3.one;
        star.transform.position = fromPos;

        star.transform.DOKill();
        star.transform.DOMove(starFlyTarget.position, starFlyDuration).SetEase(easing).OnComplete(() => 
        { 
            Destroy(star);
        });
    }
}
