using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using BEGroup.Utility;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    private const float VCAM_BLEND_TIME_SLOW = 2f;
    private const float VCAM_BLEND_TIME_FAST = 0.3f;
    private const int VCAM_PRIORITY_HIGH = 10;
    private const int VCAM_PRIORITY_MIDDLE = 7;
    private const int VCAM_PRIORITY_LOW = 5;
    private const float GROUND_HEIGHT = 1.75f;

    private const float BUFF_DOUBLE_COIN_MULTIPLIER = 2f;
    private const int BUFF_SUPER_BET_MAX_BET = 20;
    private const int BUFF_SUPER_BET_ADD_BET = 10;

    private static GameManager _instance;
    public static GameManager instance => _instance;

    private WeightList<int> _slotWeights = new();
    private Dictionary<int, SlotItem> _slots = new();
    private Dictionary<BonusItem.BuffType, float> _buffRemainTime = new();
    private Dictionary<BonusItem.BuffType, float> _buffLastTime = new();
    private bool _uiManagerInit = false;
    private bool _soundManagerInit = false;
    private Dictionary<BuildCard, bool> _buildingUpgrading = new();

    public bool isQuitting { get; set; }

    #region 全局数值
    [Title("全局数值")]
    [LabelText("重力"), Range(0.01f, 500f)]
    public float gravity = 50f;
    private float _lastGravity = 0f;

    [LabelText("绳子长度"), Range(0.1f, 50f), OnValueChanged("SetupRope")]
    public float ropeLength = 10f;

    [LabelText("绳子末端高度"), Range(0f, 20f), OnValueChanged("SetupRope")]
    public float ropeEndY = 5f;

    [LabelText("楼层高度"), Range(0.1f, 10f)]
    public float blockHeight = 3.4f;

    [LabelText("滚轮奖励锁定后停顿时间 (秒)"), Range(0f, 5f)]
    public float slotDoneDelay = 1f;
    #endregion

    #region 资源数值
    [Title("资源数值")]
    [LabelText("初始体力"), Range(1, 100)]
    public int initStamina = 100;
    public int stamina { get; private set; }

    [LabelText("初始金币"), Min(1000)]
    public int initCoin = 1000000;
    public long coin { get; private set; }

    [LabelText("完美下落额外倍率")]
    public float perfectMultiplier = 10f;

    [LabelText("正常最大倍率"), Range(1, 20)]
    public int maxBetNormal = 5;

    [LabelText("Buff时最大倍率1"), Range(1, 50)]
    public int maxBetBuff1 = 10;

    [LabelText("Buff时最大倍率2"), Range(1, 50)]
    public int maxBetBuff2 = 20;

    [LabelText("倍率按钮样式"), PreviewField]
    public Sprite betButtonStyle;

    [LabelText("正常最大倍率按钮样式"), PreviewField]
    public Sprite maxBetNormalButtonStyle;

    [LabelText("Buff时最大倍率按钮样式"), PreviewField]
    public Sprite maxBetBuffButtonStyle;

    public int bet { get; private set; }

    #endregion

    #region 内网数值

    [LabelText("购买金币数量")]
    public long buyCoinAmount = 1000000L;

    [LabelText("购买体力数量")]
    public int buyStaminaAmount = 100;

    #endregion

    #region Slot配置

    [Title("Slot配置")]
    [LabelText("Slot列表")]
    public SlotItem[] slotItems;

    [LabelText("预定义好的Slot结果 (填写Slot ID)")]
    public int[] predefinedSlotResults;

    /// <summary>
    /// 滚轮次数
    /// </summary>
    public int slotRollTimes { get; private set; } = 0;

    public void IncSlotRollTime(bool save = true)
    {
        SetSlotRollTime(slotRollTimes + 1, save);
    }

    public void SetSlotRollTime(int time, bool save = true)
    {
        slotRollTimes = time;
        if (save) SaveDataManager.SaveSlotRollTime();
    }

    #endregion

    #region 建筑数值
    [Title("建筑数值")]
    [LabelText("建筑列表")]
    public BuildCard[] buildingList;

    [LabelText("建造时间 (秒)")]
    public float buildingDuration = 1.5f;
    #endregion

    #region Bonus配置
    [Title("Bonus配置")]
    [LabelText("Bonus列表")]
    public BonusItem[] bonusList;

    public void SetBonusIndex(int index, bool save = true)
    {
        bonusIndex = index;
        if (save) SaveDataManager.SaveBonusIndex();
    }

    public int bonusIndex { get; private set; } = 0;
    
    public void SetBonusPrg(int prg, bool save = true)
    {
        bonusPrg = prg;
        if (save) SaveDataManager.SaveBonusPrg();
    }
    public int bonusPrg { get; private set; } = 0;

    public float GetBuffRemainTime(BonusItem.BuffType type)
    {
        if (_buffRemainTime.TryGetValue(type, out var time)) return time;
        return 0f;
    }

    public void SetBuffRemainTime(BonusItem.BuffType type, float time, bool save = true)
    {
        _buffRemainTime[type] = time;
        _buffLastTime[type] = -1f;
        if (save) SaveDataManager.SaveBuffRemainTime();
    }

    #endregion

    #region 单摆数值
    [Title("单摆数值")]
    [LabelText("最大摆角 (度数)"), Range(1f, 179f), OnValueChanged("SetupPendulum")]
    public float pendulumMaxAngle = 30f;

    [LabelText("最小摆角 (度数)"), Range(1f, 179f), OnValueChanged("SetupPendulum")]
    public float pendulumMinAngle = 15f;

    [LabelText("摆动速率"), Range(0.1f, 5f), OnValueChanged("SetupPendulum")]
    public float pendulumSpeed = 2f;

    [LabelText("摆动力量"), Range(1f, 1000f), OnValueChanged("SetupPendulum")]
    public float pendulumForce = 300f;
    #endregion

    #region 碰撞数值

    [Title("碰撞数值")]
    [LabelText("碰撞反弹比例"), Range(0.01f, 5f)]
    public float hitBounceForce = 0.3f;

    [LabelText("反弹次数"), Range(0, 5)]
    public int hitMaxBounceTimes = 1;

    [LabelText("完美下落阈值 (越大越简单)"), Range(0.001f, 0.5f)]
    public float hitPerfectThreshold = 0.03f;

    #endregion

    #region Slot数值

    #region 高速Slot

    [Title("高速Slot数值")]
    [LabelText("一阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float hs_firstSpeed = 20f;

    [LabelText("一阶段速度滚动张数"), Range(0, 100)]
    public int hs_firstSpeedSlotCount = 20;

    [LabelText("一阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float hs_firstDecSpeed = 20f;

    [LabelText("二阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float hs_secondSpeed = 15f;

    [LabelText("二阶段速度滚动张速"), Range(0, 100)]
    public int hs_secondSpeedSlotCount = 10;

    [LabelText("二阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float hs_secondDecSpeed = 20f;

    [LabelText("三阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float hs_thirdSpeed = 10f;

    [LabelText("三阶段速度滚动张速"), Range(0, 100)]
    public int hs_thirdSpeedSlotCount = 5;

    [LabelText("三阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float hs_thirdDecSpeed = 20f;

    [LabelText("回弹速度 (张/秒)"), Range(0.1f, 100f)]
    public float hs_reboundSpeed = 10f;

    [LabelText("回弹偏移 (张数)")]
    public float hs_reboundOffset = 0.5f;

    [LabelText("启用回弹")]
    public bool hs_rebound = false;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 50f)]
    public float hs_stopSpeed = 3f;

    #endregion

    #region 中速Slot

    [Title("中速Slot数值")]
    [LabelText("一阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ms_firstSpeed = 20f;

    [LabelText("一阶段速度滚动张数"), Range(0, 100)]
    public int ms_firstSpeedSlotCount = 20;

    [LabelText("一阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ms_firstDecSpeed = 20f;

    [LabelText("二阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ms_secondSpeed = 15f;

    [LabelText("二阶段速度滚动张速"), Range(0, 100)]
    public int ms_secondSpeedSlotCount = 10;

    [LabelText("二阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ms_secondDecSpeed = 20f;

    [LabelText("三阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ms_thirdSpeed = 10f;

    [LabelText("三阶段速度滚动张速"), Range(0, 100)]
    public int ms_thirdSpeedSlotCount = 5;

    [LabelText("三阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ms_thirdDecSpeed = 20f;

    [LabelText("回弹速度 (张/秒)"), Range(0.1f, 100f)]
    public float ms_reboundSpeed = 10f;

    [LabelText("回弹偏移 (张数)")]
    public float ms_reboundOffset = 0.5f;

    [LabelText("启用回弹")]
    public bool ms_rebound = false;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 50f)]
    public float ms_stopSpeed = 3f;

    #endregion

    #region 低速Slot

    [Title("低速Slot数值")]
    [LabelText("一阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ls_firstSpeed = 20f;

    [LabelText("一阶段速度滚动张数"), Range(0, 100)]
    public int ls_firstSpeedSlotCount = 20;

    [LabelText("一阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ls_firstDecSpeed = 20f;

    [LabelText("二阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ls_secondSpeed = 15f;

    [LabelText("二阶段速度滚动张速"), Range(0, 100)]
    public int ls_secondSpeedSlotCount = 10;

    [LabelText("二阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ls_secondDecSpeed = 20f;

    [LabelText("三阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float ls_thirdSpeed = 10f;

    [LabelText("三阶段速度滚动张速"), Range(0, 100)]
    public int ls_thirdSpeedSlotCount = 5;

    [LabelText("三阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float ls_thirdDecSpeed = 20f;

    [LabelText("回弹速度 (张/秒)"), Range(0.1f, 100f)]
    public float ls_reboundSpeed = 10f;

    [LabelText("回弹偏移 (张数)")]
    public float ls_reboundOffset = 0.5f;

    [LabelText("启用回弹")]
    public bool ls_rebound = false;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 50f)]
    public float ls_stopSpeed = 3f;

    #endregion

    #endregion

    #region 特效

    [Title("特效")]
    [LabelText("普通下落特效")]
    public GameObject fxNormalHit;

    [LabelText("普通下落特效持续时间 (秒)"), Range(0.1f, 10f)]
    public float fxNormalHitDuration = 0.5f;

    [LabelText("普通下落特效缩放")]
    public Vector3 fxNormalHitScale = Vector3.one;

    [LabelText("完美下落特效")]
    public GameObject fxPerfectHit;

    [LabelText("完美下落特效时间 (秒)"), Range(0.1f, 10f)]
    public float fxPerfectHitDuration = 0.5f;

    [LabelText("完美下落特效缩放")]
    public Vector3 fxPerfectHitScale = 3f * Vector3.one;

    [LabelText("滚轮奖励锁定特效")]
    public GameObject fxSlotDone;

    [LabelText("滚轮奖励锁定特效时间 (秒)")]
    public float fxSlotDoneDuration = 1f;

    [LabelText("滚轮奖励锁定特效缩放")]
    public Vector3 fxSlotDoneScale = Vector3.one;

    [LabelText("滚轮奖励锁定特效偏移")]
    public Vector3 fxSlotDoneOffset = 3f * Vector3.back;

    [LabelText("金币特效")]
    public GameObject fxCoinShower;

    [LabelText("金币特效时间 (秒)"), Range(0.1f, 10f)]
    public float fxCoinShowerDuration = 1f;

    [LabelText("金币特效缩放")]
    public Vector3 fxCoinShowerScale = 6f * Vector3.one;

    [LabelText("建筑升级特效")]
    public GameObject fxBuildUpgrade;

    [LabelText("建筑升级特效缩放")]
    public Vector3 fxBuildUpgradeScale = Vector3.one;

    [LabelText("建筑升级特效偏移1")]
    public Vector3 fxBuildUpgradeOffset1 = Vector3.zero;

    [LabelText("建筑升级特效1时间 (秒)"), Range(0.1f, 10f)]
    public float fxBuildUpgradeDuration1 = 0.5f;

    [LabelText("建筑升级特效偏移2")]
    public Vector3 fxBuildUpgradeOffset2 = Vector3.zero;

    [LabelText("建筑升级特效2开始时间 (秒)")]
    public float fxBuildUpgrade2StartTime = 0.3f;

    [LabelText("建筑升级特效2时间 (秒)"), Range(0.1f, 10f)]
    public float fxBuildUpgradeDuration2 = 0.5f;

    [LabelText("建筑升级特效偏移3")]
    public Vector3 fxBuildUpgradeOffset3 = Vector3.zero;

    [LabelText("建筑升级特效3开始时间 (秒)")]
    public float fxBuildUpgrade3StartTime = 0.6f;

    [LabelText("建筑升级特效3时间 (秒)"), Range(0.1f, 10f)]
    public float fxBuildUpgradeDuration3 = 0.5f;

    [LabelText("建筑升级完成特效")]
    public GameObject fxBuildUpgradeComplete;

    [LabelText("建筑升级完成特效缩放")]
    public Vector3 fxBuildUpgradeCompleteScale = Vector3.one;

    [LabelText("建筑升级完成特效偏移")]
    public Vector3 fxBuildUpgradeCompleteOffset = Vector3.zero;

    [LabelText("建筑升级完成特效时间 (秒)"), Range(0.1f, 10f)]
    public float fxBuildUpgradeCompleteDuration = 1.5f;

    [LabelText("飞星星结束特效")]
    public GameObject fxStarFlyTo;

    [LabelText("飞星星结束特效缩放")]
    public Vector3 fxStarFlyToScale = Vector3.one;

    [LabelText("飞星星结束特效时间 (秒)")]
    public float fxStarFlyToDuration = 0.5f;

    #endregion

    #region 音效

    [Title("音效")]
    [LabelText("音效配置")]
    public SoundManager sounds;

    #endregion

    #region 相机
    [Title("相机")]
    [LabelText("游戏相机"),]
    public Camera mainCamera;

    private CinemachineBrain _vCamController;

    [LabelText("机位1"),]
    public CinemachineVirtualCamera vCam1;

    [LabelText("相机1目标"),]
    public Transform vcamTarget1;

    [LabelText("机位2"),]
    public CinemachineVirtualCamera vCam2;

    [LabelText("相机2目标"),]
    public Transform vcamTarget2;

    [LabelText("城建相机")]
    public CinemachineVirtualCamera vCamBuild;
    #endregion

    #region 引用
    [Title("引用")]
    [LabelText("地面")]
    public GameObject ground;

    [LabelText("挂点")]
    public Transform hinge;

    [LabelText("挂点单摆驱动器")]
    public PendulumMotor pendulumMotor;

    [LabelText("绳子")]
    public Rigidbody rope;

    [LabelText("绳子挂点连接点")]
    public Transform ropeHingeConnector;

    [LabelText("绳子末端连接点")]
    public Transform ropeEndConnector;

    [LabelText("连接")]
    public Rigidbody link;

    [LabelText("楼层预制体")]
    public GameObject blockPrefab;

    [LabelText("人物预制体")]
    public GameObject charPrefab;

    #endregion

    #region 容器
    [Title("容器")]
    [LabelText("活动楼层容器")]
    public Transform activeBlockContainer;

    [LabelText("已下落楼层容器")]
    public Transform deadBlocksContainer;

    [LabelText("人物容器")]
    public Transform charContainer;

    #endregion

    public GameObject activeBlock { get; private set; } = null;

    public GameObject lastBlock { get; private set; } = null;

    public List<GameObject> deadBlocks { get; private set; } = new();

    private float _coinSliderStartTime = 0f;
    private float _coinSliderDuration = 0f;
    private bool _coinSlider = false;
    private long _coinSliderCurValue = 0L;
    private long _coinSliderTargetValue = 0L;
    private float _coinSliderDeltaValue = 0f;

    private void Awake()
    {
        _instance = this;
        StartCoroutine(SendLoginRequest());
        _vCamController = mainCamera.GetComponent<CinemachineBrain>();
    }

    public void UIManagerInit()
    {
        _uiManagerInit = true;
        if (_soundManagerInit) InitGame();
    }

    public void SoundManagerInit()
    {
        _soundManagerInit = true;
        if (_uiManagerInit) InitGame();
    }

    void Update()
    {
        if (isQuitting) return;

        if (_lastGravity != gravity)
        {
            _lastGravity = gravity;
            Physics.gravity = Vector3.down * gravity;
        }

        if (_coinSlider && _coinSliderDuration > 0f)
        {
            if (Time.time - _coinSliderStartTime < _coinSliderDuration)
            {
                _coinSliderCurValue += (long)(Time.deltaTime * _coinSliderDeltaValue);
                UIManager.instance.coinText.text = _coinSliderCurValue.ToString("#,0");
            }
            else
            {
                _coinSlider = false;
                UIManager.instance.coinText.text = _coinSliderTargetValue.ToString("#,0");
            }
        }

        UpdateBuff();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public bool IsBuildingUpgrading(BuildCard build)
    {
        return _buildingUpgrading.TryGetValue(build, out var result) && result;
    }

    private void SetupRope()
    {
        if (rope)
        {
            rope.transform.localScale = new Vector3(0.05f, ropeLength / 2f, 0.05f);
            rope.transform.localPosition = Vector3.up * (ropeLength / 2f + ropeEndY);
        }
        if (hinge && ropeHingeConnector)
        {
            hinge.position = ropeHingeConnector.position;
        }
    }

    private void RandomizePendulumMaxAngle()
    {
        var min = Mathf.Min(pendulumMaxAngle, pendulumMinAngle);
        var max = Mathf.Max(pendulumMaxAngle, pendulumMinAngle);
        pendulumMotor.maxAngle = UnityEngine.Random.Range(min, max);
    }

    private void SetupPendulum()
    {
        if (!pendulumMotor) return;
        RandomizePendulumMaxAngle();
        pendulumMotor.speed = pendulumSpeed;
        pendulumMotor.force = pendulumForce;
    }

    public void InitGame()
    {
        Application.targetFrameRate = 60;
        UIManager.instance.buildUI.SetActive(false);
        vCamBuild.enabled = false;
        SoundManager.instance.bgm.Play();
        SaveDataManager.Load();
        InitSlots();
        UpdateBuildingList();
        TriggerBonus(false);
        SetCameraBlendTime(VCAM_BLEND_TIME_SLOW);
        CreateBlock();
    }

    private void InitSlots()
    {
        if (slotItems != null)
        {
            foreach (var slot in slotItems)
            {
                _slots[slot.id] = slot;
                _slotWeights.Add(slot.id, slot.weight);
            }
        }
    }

    public SlotItem GetRandomSlot()
    {
        return _slots[_slotWeights.GetRandomElement()];
    }

    public void UpdateBuildingList()
    {
        if (buildingList != null)
        {
            foreach (var building in buildingList)
            {
                if (building != null)
                {
                    building.UpdateInfo();
                    building.onClick = OnBuildCardClick;
                }
            }
        }
    }

    private void CreateBlock()
    {
        RandomizePendulumMaxAngle();

        if (blockPrefab == null) return;
        if (activeBlock) return;

        var block = Instantiate(blockPrefab);
        block.name = "block";
        block.transform.SetParent(activeBlockContainer, true);
        var fixedJoint = block.GetComponent<FixedJoint>();
        if (fixedJoint)
        {
            fixedJoint.connectedBody = link;
        }
        block.transform.SetPositionAndRotation(link.position, link.rotation);
        var controller = block.GetComponent<BlockController>();
        if (controller)
        {
            controller.bounceForce = hitBounceForce;
            controller.maxBounceTimes = hitMaxBounceTimes;
        }
        block.SetActive(true);

        activeBlock = block;
    }

    public void DropActiveBlock()
    {
        void onCollisionEnd(GameObject target, bool simulated)
        {
            // 成功
            if (lastBlock == null || target == lastBlock || simulated)
            {
                // 完美特效
                if (simulated)
                {
                    SoundManager.instance.perfect.Play();
                    UIManager.instance.SetPopText("PERFECT");
                    if (fxPerfectHit)
                    {
                        var fxGo = Instantiate(fxPerfectHit);
                        fxGo.transform.position = activeBlock.transform.position + 5f * Vector3.back;
                        fxGo.transform.localScale = fxPerfectHitScale;
                        fxGo.SetActive(true);
                        this.Invoke(() => DestroyGameObject(fxGo), fxPerfectHitDuration);
                    }
                }
                else
                {
                    SoundManager.instance.good.Play();
                    UIManager.instance.SetPopText("Good");
                }

                lastBlock = activeBlock;
                deadBlocks.Add(activeBlock);
                activeBlock.transform.SetParent(deadBlocksContainer, true);
                var controller = activeBlock.GetComponent<BlockController>();

                // 随机分配Slot速度
                var index = UnityEngine.Random.Range(0, 3);
                switch (index)
                {
                    case 0:
                        Debug.Log($"高速Slot");
                        controller.slotController.firstDecSpeed = hs_firstDecSpeed;
                        controller.slotController.firstSpeed = hs_firstSpeed;
                        controller.slotController.firstSpeedSlotCount = hs_firstSpeedSlotCount;
                        controller.slotController.secondDecSpeed = hs_secondDecSpeed;
                        controller.slotController.secondSpeedSlotCount = hs_secondSpeedSlotCount;
                        controller.slotController.secondSpeed = hs_secondSpeed;
                        controller.slotController.thirdDecSpeed = hs_thirdDecSpeed;
                        controller.slotController.thirdSpeedSlotCount = hs_thirdSpeedSlotCount;
                        controller.slotController.thirdSpeed = hs_thirdSpeed;
                        controller.slotController.rebound = hs_rebound;
                        controller.slotController.reboundOffset = hs_reboundOffset;
                        controller.slotController.reboundSpeed = hs_reboundSpeed;
                        controller.slotController.stopSpeed = hs_stopSpeed;
                        break;

                    case 1:
                        Debug.Log($"中速Slot");
                        controller.slotController.firstDecSpeed = ms_firstDecSpeed;
                        controller.slotController.firstSpeed = ms_firstSpeed;
                        controller.slotController.firstSpeedSlotCount = ms_firstSpeedSlotCount;
                        controller.slotController.secondDecSpeed = ms_secondDecSpeed;
                        controller.slotController.secondSpeedSlotCount = ms_secondSpeedSlotCount;
                        controller.slotController.secondSpeed = ms_secondSpeed;
                        controller.slotController.thirdDecSpeed = ms_thirdDecSpeed;
                        controller.slotController.thirdSpeedSlotCount = ms_thirdSpeedSlotCount;
                        controller.slotController.thirdSpeed = ms_thirdSpeed;
                        controller.slotController.rebound = ms_rebound;
                        controller.slotController.reboundOffset = ms_reboundOffset;
                        controller.slotController.reboundSpeed = ms_reboundSpeed;
                        controller.slotController.stopSpeed = ms_stopSpeed;
                        break;

                    default:
                        Debug.Log($"低速Slot");
                        controller.slotController.firstDecSpeed = ls_firstDecSpeed;
                        controller.slotController.firstSpeed = ls_firstSpeed;
                        controller.slotController.firstSpeedSlotCount = ls_firstSpeedSlotCount;
                        controller.slotController.secondDecSpeed = ls_secondDecSpeed;
                        controller.slotController.secondSpeedSlotCount = ls_secondSpeedSlotCount;
                        controller.slotController.secondSpeed = ls_secondSpeed;
                        controller.slotController.thirdDecSpeed = ls_thirdDecSpeed;
                        controller.slotController.thirdSpeedSlotCount = ls_thirdSpeedSlotCount;
                        controller.slotController.thirdSpeed = ls_thirdSpeed;
                        controller.slotController.rebound = ls_rebound;
                        controller.slotController.reboundOffset = ls_reboundOffset;
                        controller.slotController.reboundSpeed = ls_reboundSpeed;
                        controller.slotController.stopSpeed = ls_stopSpeed;
                        break;
                }

                if (!simulated)
                {
                    // 复位
                    activeBlock.transform.DOKill();
                    activeBlock.transform.DOMove(new Vector3(0f, activeBlock.transform.position.y, activeBlock.transform.position.z), 0.3f);

                    // 复位特效
                    if (fxNormalHit)
                    {
                        var fxGo = Instantiate(fxNormalHit);
                        fxGo.transform.position = activeBlock.transform.position + blockHeight * 0.5f * Vector3.down + 5f * Vector3.back;
                        fxGo.SetActive(true);
                        this.Invoke(() => DestroyGameObject(fxGo), fxNormalHitDuration);
                    }
                }
                
                // Slots
                DoSlots(controller, (slotId) =>
                {
                    // Mask处理
                    if (controller.slotMask) controller.slotMask.enabled = false;
                    if (controller.slotController.slot1) controller.slotController.slot1.maskInteraction = SpriteMaskInteraction.None;
                    if (controller.slotController.slot2) controller.slotController.slot2.maskInteraction = SpriteMaskInteraction.None;
                    if (controller.slotController.slot3) controller.slotController.slot3.maskInteraction = SpriteMaskInteraction.None;

                    // 特殊Slot类型
                    string appendText = null;
                    if (_slots.TryGetValue(slotId, out var slot))
                    {
                        Debug.Log($"Slot类型: {slot.slotType}");
                        switch (slot.slotType)
                        {
                            case SlotItem.SlotType.Robbery:
                                // TODO: 偷盗玩法
                                break;

                            case SlotItem.SlotType.Sabotage:
                                // TODO: 破坏玩法
                                break;

                            case SlotItem.SlotType.Bonus:
                                appendText = TriggerBonus();
                                break;
                        }
                    }

                    // 奖励
                    var multiplier = bet * (simulated ? perfectMultiplier : 1f);

                    // 双倍金币
                    if (HasBuff(BonusItem.BuffType.DoubleCoin)) multiplier *= BUFF_DOUBLE_COIN_MULTIPLIER;

                    var reward = slot.baseBonus * multiplier;
                    SetCoin(coin + (long)reward);
                    SoundManager.instance.coin.Play();
                    if (slotId < 2)
                    {
                        SoundManager.instance.reward.Play();
                    }
                    else
                    {
                        SoundManager.instance.rewardBig.Play();
                    }
                    string rewardText = $"+{(long)reward:#,0}";
                    if (!string.IsNullOrEmpty(appendText))
                    {
                        rewardText += $"\n{appendText}";
                    }
                    UIManager.instance.SetPopText(rewardText);

                    // 特效
                    if (fxCoinShower)
                    {
                        var fxGo = Instantiate(fxCoinShower);
                        fxGo.transform.SetParent(hinge);
                        fxGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        fxGo.transform.localScale = fxCoinShowerScale;
                        fxGo.SetActive(true);
                        this.Invoke(() => DestroyGameObject(fxGo), fxCoinShowerDuration);
                    }
                    if (fxSlotDone)
                    {
                        var fxSlotDoneGo = Instantiate(fxSlotDone);
                        fxSlotDoneGo.transform.SetParent(controller.transform);
                        fxSlotDoneGo.transform.SetLocalPositionAndRotation(fxSlotDoneOffset, Quaternion.identity);
                        fxSlotDoneGo.transform.localScale = fxSlotDoneScale;
                        fxSlotDoneGo.SetActive(true);
                        this.Invoke(() => DestroyGameObject(fxSlotDoneGo), fxSlotDoneDuration);
                    }

                    // 人物飞入
                    var leftChar = Instantiate(charPrefab);
                    leftChar.transform.SetParent(charContainer);
                    leftChar.transform.SetPositionAndRotation(
                        new Vector3(-10, activeBlock.transform.position.y, activeBlock.transform.position.z),
                        Quaternion.Euler(0f, 90f, 0f));
                    leftChar.transform.DOMoveX(0f, 1f).OnComplete(() =>
                    {
                        Destroy(leftChar);
                    });

                    var rightChar = Instantiate(charPrefab);
                    rightChar.transform.SetParent(charContainer);
                    rightChar.transform.SetPositionAndRotation(
                        new Vector3(10, activeBlock.transform.position.y, activeBlock.transform.position.z),
                        Quaternion.Euler(0f, -90f, 0f));
                    rightChar.transform.DOMoveX(0f, 1f).OnComplete(() =>
                    {
                        Destroy(rightChar);
                    });

                    // 下一层
                    this.Invoke(() =>
                    {
                        if (simulated)
                        {
                            var rb = lastBlock.GetComponent<Rigidbody>();
                            rb.detectCollisions = true;
                        }
                        RaiseRope();
                        activeBlock = null;
                        Invoke(nameof(CreateBlock), 0.1f);
                        UIManager.instance.EnableButtons();
                    }, slotDoneDelay);
                });
            }

            // 失败
            else
            {
                UIManager.instance.SetPopText("Failed");
                Destroy(activeBlock);
                activeBlock = null;
                Invoke(nameof(CreateBlock), 0.1f);
                UIManager.instance.EnableButtons();
            }
        }

        if (activeBlock)
        {
            var controller = activeBlock.GetComponent<BlockController>();
            var rigidBody = activeBlock.GetComponent<Rigidbody>();
            controller.onCollisionEnd = onCollisionEnd;
            var rb = activeBlock.GetComponent<Rigidbody>();
            var fixedJoint = activeBlock.GetComponent<FixedJoint>();
            if (fixedJoint)
            {
                Destroy(fixedJoint);
            }
            UIManager.instance.DisableButtons();

            // 判定是否模拟完美
            Vector3 targetPos;
            float dh = 0f;
            if (lastBlock == null)
            {
                targetPos = Vector3.up * GROUND_HEIGHT;
                dh = activeBlock.transform.position.y - GROUND_HEIGHT;
            }
            else
            {
                targetPos = lastBlock.transform.position + Vector3.up * blockHeight;
                dh = activeBlock.transform.position.y - lastBlock.transform.position.y + blockHeight;
            }
            var t = Mathf.Sqrt(2 * dh / gravity);
            bool simulate = false;
            float rt;
            if (activeBlock.transform.position.x < 0 && rb.velocity.x > 0 ||
                activeBlock.transform.position.x >= 0f && rb.velocity.x < 0f)
            {
                rt = Mathf.Abs(activeBlock.transform.position.x) / Mathf.Abs(rb.velocity.x);
                var dt = Mathf.Abs(rt - t);
                simulate = Mathf.Abs(dt - 0.35f) <= hitPerfectThreshold;
            }
            if (simulate) controller.SimulatePerfectDrop(targetPos, t, lastBlock, onCollisionEnd);
        }
    }

    private void RaiseRope()
    {
        hinge.transform.position += Vector3.up * blockHeight;
        SmoothMoveCamera(GetCurVCamTarget().position + Vector3.up * blockHeight);
    }

    public void SetCameraBlendTime(float duration = 2f)
    {
        if (!_vCamController) return;
        _vCamController.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Linear, duration);
    }

    private Transform GetCurVCamTarget()
    {
        return vCam1.Priority > vCam2.Priority ? vcamTarget1 : vcamTarget2;
    }

    private void SmoothMoveCamera(Vector3 targetPos)
    {
        var curCam = vCam1.Priority > vCam2.Priority ? vCam1 : vCam2;
        var targetCam = vCam1.Priority < vCam2.Priority ? vCam1 : vCam2;
        var targetTrans = vCam1.Priority < vCam2.Priority ? vcamTarget1 : vcamTarget2;
        targetTrans.position = targetPos;
        curCam.Priority = VCAM_PRIORITY_MIDDLE;
        targetCam.Priority = VCAM_PRIORITY_HIGH;
        curCam.Priority = VCAM_PRIORITY_LOW;
    }

    private void DoSlots(BlockController block, Action<int> onComplete = null)
    {
        block.slotController.gameObject.SetActive(true);
        block.slotController.Reset();
        block.slotController.StartRolling(onComplete);
    }

    public void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }

    public void SetCoin(long value, float transTime = 1f, bool save = true)
    {
        if (transTime > 0f)
        {
            _coinSlider = true;
            _coinSliderCurValue = coin;
            _coinSliderTargetValue = value;
            _coinSliderStartTime = Time.time;
            _coinSliderDuration = transTime;
            _coinSliderDeltaValue = (value - coin) / transTime;
        }
        else
        {
            UIManager.instance.coinText.text = value.ToString("#,0");
        }
        coin = value;
        if (save) SaveDataManager.SaveCoin();
        UpdateBuildingList();
    }

    public void SetStamina(int value, bool save = true)
    {
        stamina = value;
        UIManager.instance.staminaText.text = value.ToString();
        if (save) SaveDataManager.SaveStamina();
    }

    public void SetBet(int value, bool save = true)
    {
        if (value > stamina) value = stamina;
        if (value <= 0)
        {
            UIManager.instance.betButton.interactable = false;
            value = 1;
        }
        bet = value;
        UIManager.instance.betText.text = $"BET x{value}";
        if (bet >= maxBetBuff1)
        {
            UIManager.instance.betButtonImage.sprite = maxBetBuffButtonStyle;
        }
        else if (bet >= maxBetNormal)
        {
            UIManager.instance.betButtonImage.sprite = maxBetNormalButtonStyle;
        }
        else
        {
            UIManager.instance.betButtonImage.sprite = betButtonStyle;
        }
        if (save) SaveDataManager.SaveBet();
    }

    /// <summary>
    /// 切换到城建
    /// </summary>
    public void SwitchToBuild()
    {
        SetCameraBlendTime(VCAM_BLEND_TIME_FAST);
        if (vCamBuild) vCamBuild.enabled = true;
    }

    /// <summary>
    /// 切换到盖楼
    /// </summary>
    public void SwitchToBlock()
    {
        SetCameraBlendTime(VCAM_BLEND_TIME_FAST);
        if (vCamBuild) vCamBuild.enabled = false;
        this.Invoke(() =>
        {
            SetCameraBlendTime(VCAM_BLEND_TIME_SLOW);
        }, VCAM_BLEND_TIME_FAST);
    }

    /// <summary>
    /// 更新等级
    /// </summary>
    public int UpdateLevel(bool update = true)
    {
        int level = 0;
        if (buildingList != null)
        {
            foreach (var item in buildingList)
            {
                level += item.level;
            }
        }
        if (update) UIManager.instance.lvText.text = level.ToString();
        return level;
    }

    public void OnBuildCardClick(BuildCard build)
    {
        if (coin >= build.cost)
        {
            // 状态 (CD)
            _buildingUpgrading[build] = true;
            build.UpdateButton();
            build.MaskEffect();
            this.Invoke(() =>
            {
                _buildingUpgrading[build] = false;
                build.UpdateButton();
                build.UpgradeCompleteEffect();
            }, buildingDuration);


            SetCoin(coin - build.cost);
            build.level++;
            var scale = build.GetScale();
            build.UpdateLevel(false, UIManager.instance.starFlyDuration);
            UIManager.instance.StarFly(build.buildingObj.transform);
            build.buildingObj.transform.DOKill();
            build.buildingObj.transform.DOScale(scale, buildingDuration).SetEase(Ease.InCubic);

            // 音效
            if (sounds.building)
            {
                sounds.building.Play();
                if (sounds.buildComplete)
                {
                    this.Invoke(() =>
                    {
                        sounds.buildComplete.Play();
                    }, buildingDuration);
                }
            }

            // 建造特效
            if (fxBuildUpgrade)
            {
                var fx1 = Instantiate(fxBuildUpgrade);
                fx1.transform.position = build.buildingObj.transform.position + fxBuildUpgradeOffset1;
                fx1.transform.localScale = fxBuildUpgradeScale;
                fx1.SetActive(true);
                this.Invoke(() =>
                {
                    Destroy(fx1);

                }, fxBuildUpgradeDuration1);

                this.Invoke(() =>
                {
                    var fx2 = Instantiate(fxBuildUpgrade);
                    fx2.transform.position = build.buildingObj.transform.position + fxBuildUpgradeOffset2;
                    fx2.transform.localScale = fxBuildUpgradeScale;
                    fx2.SetActive(true);
                    this.Invoke(() =>
                    {
                        Destroy(fx2);
                    }, fxBuildUpgradeDuration2);
                }, fxBuildUpgrade2StartTime);

                this.Invoke(() =>
                {
                    var fx3 = Instantiate(fxBuildUpgrade);
                    fx3.transform.position = build.buildingObj.transform.position + fxBuildUpgradeOffset3;
                    fx3.transform.localScale = fxBuildUpgradeScale;
                    fx3.SetActive(true);
                    this.Invoke(() =>
                    {
                        Destroy(fx3);

                        // 建造完成特效
                        if (fxBuildUpgradeComplete)
                        {
                            var fx = Instantiate(fxBuildUpgradeComplete);
                            fx.transform.position = build.buildingObj.transform.position + fxBuildUpgradeCompleteOffset;
                            fx.transform.localScale = fxBuildUpgradeCompleteScale;
                            fx.SetActive(true);
                            this.Invoke(() =>
                            {
                                Destroy(fx);
                            }, fxBuildUpgradeCompleteDuration);
                        }

                    }, fxBuildUpgradeDuration3);

                }, fxBuildUpgrade3StartTime);
            }
            SaveDataManager.SaveBuildingLevel(build);
        }
        else
        {
            UIManager.instance.buyCoinAmount.text = $"x {buyCoinAmount}";
            UIManager.instance.buyCoinPanel.SetActive(true);
        }
    }

    public string TriggerBonus(bool inc = true)
    {
        if (bonusList == null || bonusList.Length == 0) return null;

        // 获取当前Bonus
        var curBonus = bonusList[bonusIndex];
        if (curBonus == null) return null;
        int showPrgTotal = curBonus.pointNeed;
        string result = null;

        // 获取奖励并切换至下一Bonus
        if (inc) bonusPrg++;
        int showPrgCur = bonusPrg;
        if (bonusPrg >= curBonus.pointNeed)
        {
            if (bonusIndex >= bonusList.Length) bonusIndex = 0;
            else bonusIndex++;
            showPrgCur = 0;
            bonusPrg = 0;
            var newBonus = bonusList[bonusIndex];
            showPrgTotal = newBonus.pointNeed;
            
            // 奖励
            switch (curBonus.type)
            {
                case BonusItem.BonusType.Stamina:
                    SetStamina(stamina + (int)curBonus.value);
                    break;

                case BonusItem.BonusType.Coin:
                    SetCoin(coin + curBonus.value);
                    break;
            }

            // 文本
            result = $"{curBonus.type} +{curBonus.value:#,0}";

            // Buff
            if (curBonus.buff != BonusItem.BuffType.None)
            {
                _buffRemainTime[curBonus.buff] = curBonus.buffTime;
                _buffLastTime[curBonus.buff] = -1f;
            }
        }

        // 更新
        UIManager.instance.UpdateBonus(showPrgCur, showPrgTotal);
        UpdateBuff();

        return result;
    }

    public void UpdateBuff()
    {
        // 双倍金币
        _buffRemainTime.TryGetValue(BonusItem.BuffType.DoubleCoin, out var dcRemainTime);
        dcRemainTime -= Time.deltaTime;
        _buffRemainTime[BonusItem.BuffType.DoubleCoin] = Mathf.Max(0f, dcRemainTime);
        if (dcRemainTime > 0)
        {
            if (!UIManager.instance.buffDoubleCoin.activeSelf)
            {
                UIManager.instance.buffDoubleCoin.SetActive(true);
            }
            if (!_buffLastTime.TryGetValue(BonusItem.BuffType.DoubleCoin, out var dcLastTime)) dcLastTime = -1f;
            if (Time.time - dcLastTime >= 1f)
            {
                UIManager.instance.buffDoubleCoinTime.text = Utils.GetTimeMMSS(dcRemainTime);
            }
            _buffRemainTime[BonusItem.BuffType.DoubleCoin] = dcRemainTime;
            SaveDataManager.SaveBuffRemainTime();
        }
        else
        {
            if (UIManager.instance.buffDoubleCoin.activeSelf)
            {
                UIManager.instance.buffDoubleCoin.SetActive(false);
            }
        }

        // 超级倍率
        if (!_buffRemainTime.TryGetValue(BonusItem.BuffType.SuperBet, out var sbRemainTime)) sbRemainTime = -1f;
        sbRemainTime -= Time.deltaTime;
        _buffRemainTime[BonusItem.BuffType.SuperBet] = Mathf.Max(0f, sbRemainTime);
        if (sbRemainTime > 0)
        {
            if (!UIManager.instance.buffSuperBet.activeSelf)
            {
                UIManager.instance.buffSuperBet.SetActive(true);
            }
            _buffLastTime.TryGetValue(BonusItem.BuffType.SuperBet, out var sbLastTime);
            if (Time.time - sbLastTime >= 1f)
            {
                UIManager.instance.buffSuperBetTime.text = Utils.GetTimeMMSS(sbRemainTime);
            }
            _buffRemainTime[BonusItem.BuffType.SuperBet] = sbRemainTime;
            SaveDataManager.SaveBuffRemainTime();
        }
        else
        {
            if (UIManager.instance.buffSuperBet.activeSelf)
            {
                UIManager.instance.buffSuperBet.SetActive(false);
            }
            SetBet(Mathf.Min(bet, maxBetNormal));
        }
    }

    public bool HasBuff(BonusItem.BuffType type)
    {
        return _buffRemainTime.TryGetValue(type, out var time) && time > 0f;
    }

    public IEnumerator SendLoginRequest()
    {
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log($"Device id: {deviceId}");
        UnityWebRequest req = UnityWebRequest.Get($"http://115.29.231.198/fuck/bairiyishanjin?user_id={deviceId}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(req.error);
        }
        else
        {
            Debug.Log(req.downloadHandler.text);
        }
    }
}
