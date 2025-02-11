using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    // Units per pixel
    private const float UPP = 0.01f;

    /// <summary>
    /// 阶段
    /// </summary>
    private enum Phase
    {
        First,
        Second,
        Third,
    }

    [Title("引用")]
    public SpriteRenderer slot1;
    public SpriteRenderer slot2;
    public SpriteRenderer slot3;
    public Sprite[] slotPools;

    [Title("数值")]
    [LabelText("每张大小 (像素)")]
    public Vector2Int slotSize;

    [LabelText("一阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float firstSpeed = 20f;

    [LabelText("一阶段速度滚动张数"), Range(0, 100)]
    public int firstSpeedSlotCount = 20;

    [LabelText("一阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float firstDecSpeed = 20f;

    [LabelText("二阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float secondSpeed = 15f;

    [LabelText("二阶段速度滚动张速"), Range(0, 100)]
    public int secondSpeedSlotCount = 10;

    [LabelText("二阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float secondDecSpeed = 20f;

    [LabelText("三阶段速度 (张/秒)"), Range(0.1f, 100f)]
    public float thirdSpeed = 10f;

    [LabelText("三阶段速度滚动张速"), Range(0, 100)]
    public int thirdSpeedSlotCount = 5;

    [LabelText("三阶段速度下的减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float thirdDecSpeed = 20f;

    [LabelText("回弹速度 (张/秒)"), Range(0.1f, 100f)]
    public float reboundSpeed = 10f;

    [LabelText("回弹偏移 (张数)")]
    public float reboundOffset = 0.5f;

    [LabelText("启用回弹")]
    public bool rebound = false;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 50f)]
    public float stopSpeed = 3f;

    private Phase _phase = Phase.First;

    private Sprite GetRandomSlot(out int index)
    {
        index = UnityEngine.Random.Range(0, slotPools.Length);
        return (slotPools == null || slotPools.Length == 0) ? null : slotPools[index];
    }

    private bool _rolling = false;
    private bool _stopping = false;
    private float _offset = 0f;
    private SpriteRenderer _curSlot;
    private SpriteRenderer _up2Slot;
    private SpriteRenderer _up1Slot;
    private int _curSlotIndex = 0;
    private int _up2SlotIndex = 0;
    private int _up1SlotIndex = 0;
    private Action<int> _onStop;
    private int _initSlotCount = 0;
    private int _midSlotCount = 0;
    private int _lowSlotCount = 0;
    private float _speed = 0f;
    private float _midSpeed = 0f;
    private float _lowSpeed = 0f;
    private float _stoppingSpeed = 0f;
    private float _initDec = 0f;
    private float _midDec = 0f;
    private float _lowDec = 0f;
    private float _reboundingSpeed = 0f;
    private bool _rebounding = false;

    public void Reset()
    {
        _rolling = false;
        //SoundManager.instance.slot.Stop();
        _offset = 0f;
        _curSlot = slot1;
        _curSlot.sprite = GetRandomSlot(out _curSlotIndex);
        _curSlot.sortingOrder = 0;
        _curSlot.transform.localPosition = Vector3.zero;
        _up1Slot = slot2;
        _up1Slot.sprite = GetRandomSlot(out _up1SlotIndex);
        _up1Slot.sortingOrder = 0;
        _up1Slot.transform.localPosition = UPP * slotSize.y * Vector3.up;
        _up2Slot = slot3;
        _up2Slot.sprite = GetRandomSlot(out _up2SlotIndex);
        _up2Slot.sortingOrder = 0;
        _up2Slot.transform.localPosition = UPP * slotSize.y * 2f * Vector3.up;
        _initSlotCount = 0;
        _midSlotCount = 0;
        _lowSlotCount = 0;
        _speed = -UPP * firstSpeed * slotSize.y;
        _midSpeed = -UPP * secondSpeed * slotSize.y;
        _lowSpeed = -UPP * thirdSpeed * slotSize.y;
        _stoppingSpeed = -UPP * stopSpeed * slotSize.y;
        _reboundingSpeed = UPP * reboundSpeed * slotSize.y;
        _rebounding = false;
        _initDec = UPP * firstDecSpeed * slotSize.y;
        _midDec = UPP * secondDecSpeed * slotSize.y;
        _lowDec = UPP * thirdDecSpeed * slotSize.y;
        slot1.gameObject.SetActive(true);
        slot2.gameObject.SetActive(true);
        slot3.gameObject.SetActive(true);
        _phase = Phase.First;
    }

    public void StartRolling(Action<int> onStop)
    {
        _onStop = onStop;
        _rolling = true;
        //SoundManager.instance.slot.Play();
    }

    private void DetermineSpeed()
    {
        if (_rebounding)
        {
            _speed = _reboundingSpeed;
            return;
        }
        switch (_phase)
        {
            case Phase.First:
                if (_initSlotCount >= firstSpeedSlotCount && !_stopping)
                {
                    _speed += Time.deltaTime * _initDec;
                }
                break;

            case Phase.Second:
                if (_midSlotCount >= secondSpeedSlotCount && !_stopping)
                {
                    _speed += Time.deltaTime * _midDec;
                }
                break;

            case Phase.Third:
                if (_lowSlotCount >= thirdSpeedSlotCount && !_stopping)
                {
                    _speed += Time.deltaTime * _lowDec;
                }
                break;
        }
    }

    private void Update()
    {
        if (!_rolling) return;

        // 滚动
        DetermineSpeed();
        var dy = Time.deltaTime * _speed;
        _offset += dy;
        _curSlot.transform.localPosition = _offset * Vector3.up;
        _up1Slot.transform.localPosition = (_offset + UPP * slotSize.y) * Vector3.up;
        _up2Slot.transform.localPosition = (_offset + UPP * slotSize.y * 2f) * Vector3.up;

        // 没在回弹中
        if (!_rebounding)
        {
            // 切换速度
            switch (_phase)
            {
                case Phase.First:
                    if (_speed >= _midSpeed)
                    {
                        _phase = Phase.Second;
                        //Debug.Log($"Switch to mid speed");
                    }
                    break;

                case Phase.Second:
                    if (_speed >= _lowSpeed)
                    {
                        _phase = Phase.Third;
                        //Debug.Log($"Switch to low speed");
                    }
                    break;
            }

            // 判定停止
            if (_phase == Phase.Third && _speed >= _stoppingSpeed)
            {
                _stopping = true;
                //Debug.Log($"Stopping");
            }

            // 判定是否回弹
            if (_stopping && rebound)
            {
                if (!_rebounding && _offset <= -UPP * reboundOffset * slotSize.y)
                {
                    _rebounding = true;
                }
            }

            // 切换Slot
            else if (_offset <= -UPP * slotSize.y)
            {
                SoundManager.instance.slotClick.Play();
                _curSlot = GetNextSlot(_curSlot, out _curSlotIndex);
                _up1Slot = GetNextSlot(_up1Slot, out _up1SlotIndex);
                _up2Slot = GetNextSlot(_up2Slot, out _up2SlotIndex);
                if (!_rebounding) _offset += UPP * slotSize.y;
                _up2Slot.transform.localPosition = (_offset + UPP * slotSize.y * 2f) * Vector3.up;
                _up2Slot.sprite = GetRandomSlot(out _up2SlotIndex);
                switch (_phase)
                {
                    case Phase.First:
                        _initSlotCount++;
                        //Debug.Log($"Init slot count: {_initSlotCount}");
                        break;

                    case Phase.Second:
                        _midSlotCount++;
                        //Debug.Log($"Mid slot count: {_midSlotCount}");
                        break;

                    case Phase.Third:
                        _lowSlotCount++;
                        //Debug.Log($"Low slot count: {_lowSlotCount}");
                        break;
                }

                // 停止
                if (_stopping)
                {
                    _rolling = false;
                    //SoundManager.instance.slot.Stop();
                    _up1Slot.gameObject.SetActive(false);
                    _up2Slot.gameObject.SetActive(false);
                    _curSlot.sortingOrder = 100;
                    _curSlot.transform.localPosition = Vector3.zero;
                    _onStop?.Invoke(_curSlotIndex);
                }
            }
        }

        // 在回弹中
        else
        {
            if (_offset >= 0)
            {
                _rolling = false;
                //SoundManager.instance.slot.Stop();
                _up1Slot.gameObject.SetActive(false);
                _up2Slot.gameObject.SetActive(false);
                _curSlot.sortingOrder = 100;
                _curSlot.transform.localPosition = Vector3.zero;
                _onStop?.Invoke(_curSlotIndex);
            }
        }
    }

    private int GetSlotIndex(SpriteRenderer slot)
    {
        if (slot == _curSlot) return _curSlotIndex;
        if (slot == _up1Slot) return _up1SlotIndex;
        return _up2SlotIndex;
    }

    private SpriteRenderer GetNextSlot(SpriteRenderer slot, out int index)
    {
        if (slot == slot1)
        {
            index = GetSlotIndex(slot2);
            return slot2;
        }
        if (slot == slot2)
        {
            index = GetSlotIndex(slot3);
            return slot3;
        }
        index = GetSlotIndex(slot1);
        return slot1;
    }
}
