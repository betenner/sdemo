using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 保存数据管理器
/// </summary>
public static class SaveDataManager
{
    private const string KEY_STAMINA = "_stamina";
    private const string KEY_COIN = "_coin";
    private const string KEY_BET = "_bet";
    private const string KEY_BUILD_LEVEL = "_build_{0}_level";

    /// <summary>
    /// 加载数据
    /// </summary>
    public static void Load()
    {
        GameManager.instance.SetStamina(PlayerPrefs.GetInt(KEY_STAMINA, GameManager.instance.initStamina));
        GameManager.instance.SetCoin(PlayerPrefs.GetInt(KEY_COIN, GameManager.instance.initCoin));
        GameManager.instance.SetBet(PlayerPrefs.GetInt(KEY_BET, 1));

        if (GameManager.instance.buildingList != null)
        {
            foreach (var building in GameManager.instance.buildingList)
            {
                if (building != null)
                {
                    building.level = PlayerPrefs.GetInt(string.Format(KEY_BUILD_LEVEL, building.id), 0);
                    building.UpdateInfo();
                }
            }
        }
    }

    public static void SaveStamina(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_STAMINA, GameManager.instance.stamina);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveCoin(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_COIN, GameManager.instance.coin);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveBet(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_BET, GameManager.instance.bet);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveBuildingLevel(BuildCard build, bool flush = true)
    {
        if (build == null) return;
        PlayerPrefs.SetInt(string.Format(KEY_BUILD_LEVEL, build.id), build.level);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveAll()
    {
        SaveStamina(false);
        SaveCoin(false);
        SaveBet(false);
        if (GameManager.instance.buildingList != null)
        {
            foreach (var building in GameManager.instance.buildingList)
            {
                if (building != null)
                {
                    SaveBuildingLevel(building, false);
                }
            }
        }
        PlayerPrefs.Save();
    }
}
