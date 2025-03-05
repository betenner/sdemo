using InspectorGadgets.Attributes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BonusItem : MonoBehaviour
{
    public enum BonusType
    {
        Coin,
        Stamina,
    }

    public enum BuffType
    {
        None,
        DoubleCoin,
        SuperBet,
    }

    [LabelText("类型"), EnumPaging]
    public BonusType type;

    [LabelText("数值"), Min(0)]
    public long value;

    [LabelText("需要点数"), Min(1)]
    public int pointNeed;

    [LabelText("获得Buff"), EnumPaging]
    public BuffType buff = BuffType.None;

    [LabelText("Buff持续时间 (秒)"), Range(5, 3600)]
    public int buffTime = 300;

    [LabelText("奖励图标"), PreviewField]
    public Sprite icon;
}
