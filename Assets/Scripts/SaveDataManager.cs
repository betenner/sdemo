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
    private const string KEY_SLOT_ROLL_TIME = "_slot_roll_time";
    private const string KEY_BONUS_INDEX = "_bonus_index";
    private const string KEY_BONUS_PRG = "_bonus_prg";
    private const string KEY_BUFF_REMAIN_TIME_DOUBLE_COIN = "_buff_remain_time_double_coin";
    private const string KEY_BUFF_REMAIN_TIME_SUPER_BET = "_buff_remain_time_super_bet";

    /// <summary>
    /// 加载数据
    /// </summary>
    public static void Load()
    {
        GameManager.instance.SetStamina(PlayerPrefs.GetInt(KEY_STAMINA, GameManager.instance.initStamina));
        var coinStr = PlayerPrefs.GetString(KEY_COIN, GameManager.instance.initCoin.ToString());
        if (long.TryParse(coinStr, out var coin))
        {
            GameManager.instance.SetCoin(coin, 1f, false);
        }
        GameManager.instance.SetBet(PlayerPrefs.GetInt(KEY_BET, 1), false);
        GameManager.instance.SetSlotRollTime(PlayerPrefs.GetInt(KEY_SLOT_ROLL_TIME, 0), false);
        GameManager.instance.SetBonusIndex(PlayerPrefs.GetInt(KEY_BONUS_INDEX, 0), false);
        GameManager.instance.SetBonusPrg(PlayerPrefs.GetInt(KEY_BONUS_PRG, 0), false);
        GameManager.instance.SetBuffRemainTime(BonusItem.BuffType.DoubleCoin, PlayerPrefs.GetFloat(KEY_BUFF_REMAIN_TIME_DOUBLE_COIN, 0f), false);
        GameManager.instance.SetBuffRemainTime(BonusItem.BuffType.SuperBet, PlayerPrefs.GetFloat(KEY_BUFF_REMAIN_TIME_SUPER_BET, 0f), false);

        if (GameManager.instance.buildingList != null)
        {
            foreach (var building in GameManager.instance.buildingList)
            {
                if (building != null)
                {
                    building.level = PlayerPrefs.GetInt(string.Format(KEY_BUILD_LEVEL, building.id), 0);
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
        PlayerPrefs.SetString(KEY_COIN, GameManager.instance.coin.ToString());
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

    public static void SaveSlotRollTime(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_SLOT_ROLL_TIME, GameManager.instance.slotRollTimes);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveBonusIndex(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_BONUS_INDEX, GameManager.instance.bonusIndex);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveBonusPrg(bool flush = true)
    {
        PlayerPrefs.SetInt(KEY_BONUS_PRG, GameManager.instance.bonusPrg);
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveBuffRemainTime(bool flush = true)
    {
        PlayerPrefs.SetFloat(KEY_BUFF_REMAIN_TIME_DOUBLE_COIN, GameManager.instance.GetBuffRemainTime(BonusItem.BuffType.DoubleCoin));
        PlayerPrefs.SetFloat(KEY_BUFF_REMAIN_TIME_SUPER_BET, GameManager.instance.GetBuffRemainTime(BonusItem.BuffType.SuperBet));
        if (flush) PlayerPrefs.Save();
    }

    public static void SaveAll()
    {
        SaveStamina(false);
        SaveCoin(false);
        SaveBet(false);
        SaveSlotRollTime(false);
        SaveBonusIndex(false);
        SaveBonusPrg(false);
        SaveBuffRemainTime(false);
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
