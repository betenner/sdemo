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

    [LabelText("初始速度 (张/秒)"), Range(0.1f, 50f)]
    public float initSpeed = 20f;

    [LabelText("初始滚动张数 (匀速)"), Range(1, 50)]
    public int initSlotCount = 20;

    [LabelText("减速度 (张/秒^2)"), Range(0.1f, 10f)]
    public float decSpeed = 20f;

    [LabelText("停止速度 (张/秒)"), Range(0.1f, 10f)]
    public float stopSpeed = 3f;

    private Sprite GetRandomSlot()
    {
        return (slotPools == null || slotPools.Length == 0) ? null : slotPools[UnityEngine.Random.Range(0, slotPools.Length)];
    }

    private bool _rolling = false;
    private bool _stopping = false;
    private float _offset = 0f;
    private SpriteRenderer _curSlot;
    private SpriteRenderer _up2Slot;
    private SpriteRenderer _up1Slot;
    private Action _onStop;
    private int _initSlotCount = 0;
    private float _speed = 0f;
    private float _acc = 0f;
    private float _stoppingSpeed = 0f;

    public void Reset()
    {
        _rolling = false;
        _offset = 0f;
        _curSlot = slot1;
        _curSlot.sprite = GetRandomSlot();
        _curSlot.sortingOrder = 0;
        _curSlot.transform.localPosition = Vector3.zero;
        _up1Slot = slot2;
        _up1Slot.sprite = GetRandomSlot();
        _up1Slot.sortingOrder = 0;
        _up1Slot.transform.localPosition = UPP * slotSize.y * Vector3.up;
        _up2Slot = slot3;
        _up2Slot.sprite = GetRandomSlot();
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

    public void StartRolling(Action onStop)
    {
        _onStop = onStop;
        _rolling = true;
    }

    private void Update()
    {
        if (!_rolling) return;

        // 向下滚动
        if (_initSlotCount >= initSlotCount && !_stopping)
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
            _curSlot = GetNextSlot(_curSlot);
            _up1Slot = GetNextSlot(_up1Slot);
            _up2Slot = GetNextSlot(_up2Slot);
            _offset += UPP * slotSize.y;
            _up2Slot.transform.localPosition = (_offset + UPP * slotSize.y * 2f) * Vector3.up;
            _up2Slot.sprite = GetRandomSlot();
            _initSlotCount++;

            // 停止
            if (_stopping)
            {
                _rolling = false;
                _up1Slot.gameObject.SetActive(false);
                _up2Slot.gameObject.SetActive(false);
                _curSlot.sortingOrder = 100;
                _curSlot.transform.localPosition = Vector3.zero;
                _onStop?.Invoke();
            }
        }
    }

    private SpriteRenderer GetNextSlot(SpriteRenderer slot)
    {
        if (slot == slot1) return slot2;
        if (slot == slot2) return slot3;
        return slot1;
    }
}
