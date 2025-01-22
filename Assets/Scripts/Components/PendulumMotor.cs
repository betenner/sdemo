using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单摆驱动
/// </summary>
public class PendulumMotor : MonoBehaviour
{
    /// <summary>
    /// 最大摆角（度数）
    /// </summary>
    public float maxAngle = 30f;

    /// <summary>
    /// 摆动速度
    /// </summary>
    public float speed = 2.0f;

    /// <summary>
    /// 驱动力
    /// </summary>
    public float force = 300f;

    /// <summary>
    /// 当前速度
    /// </summary>
    public float velocity { get; private set; }

    private HingeJoint _joint;

    private void Awake()
    {
        _joint = GetComponent<HingeJoint>();
    }

    void FixedUpdate()
    {
        // 计算摆动角度（基于正弦函数）
        velocity = maxAngle * Mathf.Sin(Time.fixedTime * speed);

        // 计算旋转
        var motor = _joint.motor;
        motor.targetVelocity = velocity;
        motor.force = 100f;
        _joint.motor = motor;
    }
}
