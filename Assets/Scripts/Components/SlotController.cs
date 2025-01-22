using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    // Units per pixel
    private const float UPP = 0.01f;

    [Title("引用")]
    public SpriteRenderer slot1;
    public SpriteRenderer slot2;
    public SpriteRenderer slot3;
    public Sprite[] slotPools;

    [Title("数值")]
    [LabelText("每张大小 (像素)")]
    public Vector2Int slotSize;

    [LabelText("初始速度 (张/秒)"), Range(0.1f, 100f)]
    public float initSpeed = 20f;

    [LabelText("高速滚动张数 (匀速)"), Range(1, 50)]
    public int highspeedSlotCount = 20;

    [LabelText("减速度 (张/秒^2)"), Range(0.1f, 100f)]
    public float decSpeed = 20f;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 50f)]
    public float stopSpeed = 3f;

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
    private float _speed = 0f;
    private float _acc = 0f;
    private float _stoppingSpeed = 0f;

    public void Reset()
    {
        _rolling = false;
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
        _speed = -UPP * initSpeed * slotSize.y;
        _stoppingSpeed = -UPP * stopSpeed * slotSize.y;
        _acc = UPP * decSpeed * slotSize.y;
        slot1.gameObject.SetActive(true);
        slot2.gameObject.SetActive(true);
        slot3.gameObject.SetActive(true);
    }

    public void StartRolling(Action<int> onStop)
    {
        _onStop = onStop;
        _rolling = true;
    }

    private void Update()
    {
        if (!_rolling) return;

        // 向下滚动
        if (_initSlotCount >= highspeedSlotCount && !_stopping)
        {
            _speed += Time.deltaTime * _acc;
        }
        var dy = Time.deltaTime * _speed;
        _offset += dy;
        _curSlot.transform.localPosition = _offset * Vector3.up;
        _up1Slot.transform.localPosition = (_offset + UPP * slotSize.y) * Vector3.up;
        _up2Slot.transform.localPosition = (_offset + UPP * slotSize.y * 2f) * Vector3.up;

        // 判定停止
        if (_speed >= _stoppingSpeed)
        {
            _stopping = true;
        }

        // 切换Slot
        if (_offset <= -UPP * slotSize.y)
        {
            _curSlot = GetNextSlot(_curSlot, out _curSlotIndex);
            _up1Slot = GetNextSlot(_up1Slot, out _up1SlotIndex);
            _up2Slot = GetNextSlot(_up2Slot, out _up2SlotIndex);
            _offset += UPP * slotSize.y;
            _up2Slot.transform.localPosition = (_offset + UPP * slotSize.y * 2f) * Vector3.up;
            _up2Slot.sprite = GetRandomSlot(out _up2SlotIndex);
            _initSlotCount++;

            // 停止
            if (_stopping)
            {
                _rolling = false;
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
