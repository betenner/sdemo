using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [LabelText("背景音乐")]
    public AudioSource bgm;

    [LabelText("Slot音效")]
    public AudioSource slot;

    [LabelText("普通堆叠音效")]
    public AudioSource good;

    [LabelText("完美堆叠音效")]
    public AudioSource perfect;

    [LabelText("普通奖励音效")]
    public AudioSource reward;

    [LabelText("大奖励音效")]
    public AudioSource rewardBig;

    [LabelText("获得金币音效")]
    public AudioSource coin;

    [LabelText("首次落地音效")]
    public AudioSource firstDrop;

    [LabelText("后续堆叠音效")]
    public AudioSource stack;

    [LabelText("Slot单次音效")]
    public AudioSource slotClick;

    [LabelText("建造中音效")]
    public AudioSource building;

    [LabelText("建造完成音效")]
    public AudioSource buildComplete;

    private void Awake()
    {
        instance = this;
        GameManager.instance.SoundManagerInit();
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
