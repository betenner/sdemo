using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ToggleNode : MonoBehaviour
{
    [LabelText("开"), OnValueChanged("UpdateState")]
    public bool on;

    [LabelText("开时激活列表")]
    public GameObject[] onList;

    [LabelText("关时激活列表")]
    public GameObject[] offList;

    private void OnEnable()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        foreach(var item in onList)
        {
            item.SetActive(on);
        }

        foreach(var item in offList)
        {
            item.SetActive(!on);
        }
    }
}
