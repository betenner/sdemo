using Sirenix.OdinInspector;
using UnityEngine;

public class SlotItem : MonoBehaviour
{
    public enum SlotType
    {
        Normal,
        Robbery,
        Sabotage,
        Event,
    }

    [LabelText("ID"), Min(1)]
    public int id = 1;

    [LabelText("类型"), EnumPaging]
    public SlotType slotType = SlotType.Normal;

    [LabelText("权重"), Range(1, 10000)]
    public uint weight = 100;

    [LabelText("图片"), PreviewField]
    public Sprite image;

    [LabelText("基础奖励"), Min(1)]
    public long baseBonus = 1000;
}
