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

    [LabelText("下落按钮")]
    public Button dropButton;

    [LabelText("倍率按钮")]
    public Button betButton;

    [LabelText("金币文本")]
    public TextMeshProUGUI coinText;

    [LabelText("体力文本")]
    public TextMeshProUGUI staminaText;

    [LabelText("倍率文本")]
    public TextMeshProUGUI betText;

    [LabelText("弹出文本")]
    public TextMeshProUGUI popText;

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
    }

    public void RemoveEventListeners()
    {
        dropButton.onClick.RemoveAllListeners();
        betButton.onClick.RemoveAllListeners();
    }

    public void OnDropButtonClick()
    {
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
}
