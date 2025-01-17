using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumController : MonoBehaviour
{
    public float length = 2.0f; // 摆长
    public float maxAngle = 30f; // 最大摆角（度数）
    public float speed = 2.0f; // 摆动速度

    public float angle { get; private set; } // 当前角度

    private void Awake()
    {
        length = GameManager.instance.ropeLength;
    }

    void Update()
    {
        // 计算摆动角度（基于正弦函数）
        angle = maxAngle * Mathf.Sin(Time.time * speed);

        // 计算旋转
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
