using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int VCAM_PRIORITY_HIGH = 10;
    private const int VCAM_PRIORITY_MIDDLE = 7;
    private const int VCAM_PRIORITY_LOW = 5;
    private const float GROUND_HEIGHT = 1.75f;

    private static GameManager _instance;
    public static GameManager instance => _instance;

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
    #endregion

    #region 资源数值
    [Title("资源数值")]
    [LabelText("初始体力"), Range(1, 100)]
    public int initStamina = 100;
    public int stamina { get; private set; }

    [LabelText("初始金币"), Min(1000)]
    public long initCoin = 1000000L;
    public long coin { get; private set; }

    [LabelText("基础奖励"), Min(1L)]
    public long baseReward = 1000L;

    [LabelText("完美下落额外倍率")]
    public float perfectMultiplier = 10f;

    [LabelText("最大倍率"), Range(1, 10)]
    public int maxBet = 5;
    public int bet { get; private set; }

    [LabelText("Slot倍率 (需要与Slot数量一致)"), Range(1f, 1000f)]
    public float[] slotMultiplier = { 1f, 5f, 20f, 100f};

    #endregion

    #region 单摆数值
    [Title("单摆数值")]
    [LabelText("最大摆角 (度数)"), Range(1f, 179f), OnValueChanged("SetupPendulum")]
    public float pendulumMaxAngle = 30f;

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

    [LabelText("金币特效")]
    public GameObject fxCoinShower;

    [LabelText("金币特效时间 (秒)"), Range(0.1f, 10f)]
    public float fxCoinShowerDuration = 1f;

    [LabelText("金币特效缩放")]
    public Vector3 fxCoinShowerScale = 6f * Vector3.one;

    #endregion

    #region 相机
    [Title("相机")]
    [LabelText("游戏相机"),]
    public Camera gameCamera;

    [LabelText("机位1"),]
    public CinemachineVirtualCamera vCam1;

    [LabelText("相机1目标"),]
    public Transform vcamTarget1;

    [LabelText("机位2"),]
    public CinemachineVirtualCamera vCam2;

    [LabelText("相机2目标"),]
    public Transform vcamTarget2;
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
    }

    void Update()
    {
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
    }

    private void OnDestroy()
    {
        _instance = null;
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

    private void SetupPendulum()
    {
        if (!pendulumMotor) return;
        pendulumMotor.maxAngle = pendulumMaxAngle;
        pendulumMotor.speed = pendulumSpeed;
        pendulumMotor.force = pendulumForce;
    }

    public void InitGame()
    {
        Application.targetFrameRate = 60;

        SetCoin(initCoin);
        SetStamina(initStamina);
        SetBet(1);

        CreateBlock();
    }

    private void CreateBlock()
    {
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
                DoSlots(controller, (slotIndex) =>
                {
                    // 奖励
                    var multiplier = bet * slotMultiplier[slotIndex] * (simulated ? perfectMultiplier : 1f);
                    var reward = baseReward * multiplier;
                    SetCoin(coin + (long)reward);
                    UIManager.instance.SetPopText($"+{(long)reward:#,0}");

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
                    if (simulated)
                    {
                        var rb = lastBlock.GetComponent<Rigidbody>();
                        rb.detectCollisions = true;
                    }
                    RaiseRope();
                    activeBlock = null;
                    Invoke(nameof(CreateBlock), 0.1f);
                    UIManager.instance.dropButton.interactable = true;
                });
            }

            // 失败
            else
            {
                UIManager.instance.SetPopText("Failed");
                Destroy(activeBlock);
                activeBlock = null;
                Invoke(nameof(CreateBlock), 0.1f);
                UIManager.instance.dropButton.interactable = true;
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
            UIManager.instance.dropButton.interactable = false;

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

    public void SetCoin(long value, float transTime = 1f)
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
    }

    public void SetStamina(int value)
    {
        stamina = value;
        UIManager.instance.staminaText.text = value.ToString();
        UIManager.instance.dropButton.interactable = stamina > 0;
    }

    public void SetBet(int value)
    {
        if (value > stamina) value = stamina;
        if (value <= 0)
        {
            UIManager.instance.betButton.interactable = false;
            value = 1;
        }
        bet = value;
        UIManager.instance.betText.text = $"BET x{value}";
    }
}
