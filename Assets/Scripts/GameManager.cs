using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int VCAM_PRIORITY_HIGH = 10;
    private const int VCAM_PRIORITY_MIDDLE = 7;
    private const int VCAM_PRIORITY_LOW = 5;
    private const float GROUND_HEIGHT = 1.75f;

    private static GameManager _instance;
    public static GameManager instance => _instance;

    [Title("数值")]
    [LabelText("重力"), Range(0.01f, 500f)]
    public float gravity = 50f;
    private float _lastGravity = 0f;

    [LabelText("绳子长度"), Range(0.1f, 50f), OnValueChanged("SetupRope")]
    public float ropeLength = 10f;

    [LabelText("绳子末端高度"), Range(0f, 20f), OnValueChanged("SetupRope")]
    public float ropeEndY = 5f;

    [LabelText("楼层高度"), Range(0.1f, 10f)]
    public float blockHeight = 3.4f;

    //[LabelText("初始水平速度"), Range(-100f, 100f)]
    //public float initSpeed = -10f;

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

    [Title("引用")]
    [LabelText("地面")]
    public GameObject ground;

    [LabelText("挂点")]
    public Transform hinge;

    [LabelText("绳子")]
    public Rigidbody rope;

    [LabelText("绳子配重")]
    public Rigidbody ropePayload;

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

    [Title("容器")]
    [LabelText("活动楼层容器")]
    public Transform activeBlockContainer;

    [LabelText("已下落楼层容器")]
    public Transform deadBlocksContainer;

    [LabelText("人物容器")]
    public Transform charContainer;

    public GameObject activeBlock { get; private set; } = null;

    public GameObject lastBlock { get; private set; } = null;

    public List<GameObject> deadBlocks { get; private set; } = new();

    private void Awake()
    {
        _instance = this;
        InitGame();
    }

    void Update()
    {
        if (_lastGravity != gravity)
        {
            _lastGravity = gravity;
            Physics.gravity = Vector3.down * gravity;
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

    private void InitGame()
    {
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
                lastBlock = activeBlock;
                deadBlocks.Add(activeBlock);
                activeBlock.transform.SetParent(deadBlocksContainer, true);
                var controller = activeBlock.GetComponent<BlockController>();

                if (!simulated)
                {
                    // 复位
                    activeBlock.transform.DOKill();
                    activeBlock.transform.DOMove(new Vector3(0f, activeBlock.transform.position.y, activeBlock.transform.position.z), 0.3f);
                }
                
                // Slots
                DoSlots(controller, () =>
                {
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
                simulate = dt >= 0.32f && dt <= 0.38f;
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

    private void DoSlots(BlockController block, Action onComplete = null)
    {
        block.slotController.gameObject.SetActive(true);
        block.slotController.Reset();
        block.slotController.StartRolling(onComplete);
    }
}
