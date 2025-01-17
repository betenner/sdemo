using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public SlotController slotController;

    private const float STABLE_THRESHOLD = 0.01f;
    
    public Action<GameObject, bool> onCollisionEnd;

    private Rigidbody _rigidbody;
    private bool _waitForCollisionEnd = false;
    private GameObject _lastCollideObject = null;
    private bool _simulatingPerfectDrop = false;
    private float _simulateY;
    private float _simulateTargetX;
    private float _simulateTargetY;
    private float _simulateYSpeed = 0f;
    private GameObject _simulateLastBlock;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.sleepThreshold = STABLE_THRESHOLD;
    }

    void Update()
    {
        if (GameManager.instance.activeBlock != gameObject) return;
        if (!_simulatingPerfectDrop && _waitForCollisionEnd)
        {
            // 停止移动
            if (_rigidbody.IsSleeping())
            {
                _waitForCollisionEnd = false;
                _rigidbody.isKinematic = true;
                onCollisionEnd?.Invoke(_lastCollideObject, false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.activeBlock != gameObject) return;
        if (_simulatingPerfectDrop)
        {
            bool end = false;
            _simulateYSpeed += Time.deltaTime * Physics.gravity.y;
            _simulateY += Time.deltaTime * _simulateYSpeed;
            if (_simulateY <= _simulateTargetY)
            {
                _simulateY = _simulateTargetY;
                end = true;
            }
            transform.position = new Vector3(transform.position.x, _simulateY, 0f);
            if (end)
            {
                _simulatingPerfectDrop = false;
                onCollisionEnd?.Invoke(_simulateLastBlock, true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.instance.activeBlock != gameObject) return;

        // 碰地处理
        if (collision.gameObject == GameManager.instance.ground && GameManager.instance.lastBlock != null)
        {
            _waitForCollisionEnd = false;
            onCollisionEnd?.Invoke(collision.gameObject, false);
            return;
        }
        _lastCollideObject = collision.gameObject;
        _waitForCollisionEnd = true;
    }

    public void SimulatePerfectDrop(Vector3 targetPos, float t, GameObject lastBlock, Action<GameObject, bool> onFinish)
    {
        onCollisionEnd = onFinish;
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.Sleep();
        transform.DOKill();
        transform.DORotateQuaternion(Quaternion.identity, t * 0.6f);
        transform.DOMoveX(0f, t * 0.6f);
        _simulateTargetX = targetPos.x;
        _simulateTargetY = targetPos.y;
        _simulateY = transform.position.y;
        _simulateYSpeed = 0f;
        _simulateLastBlock = lastBlock;
        _simulatingPerfectDrop = true;
    }
}
