using UnityEngine;

/// <summary>
/// 工具类
/// </summary>
public static class Utils
{
    /// <summary>
    /// 转化为KMBT数值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ConvertToKMBT(long value)
    {
        long heading = value;
        string unit = string.Empty;

        // T
        if (value > 1000000000000000L)
        {
            heading = value / 1000000000000L;
            unit = "T";
        }

        // B
        else if (value > 1000000000000L)
        {
            heading = value / 1000000000L;
            unit = "B";
        }

        // M
        else if (value > 1000000000L)
        {
            heading = value / 1000000L;
            unit = "M";
        }

        // K
        else if (value > 1000000L)
        {
            heading = value / 1000L;
            unit = "K";
        }

        return $"{heading:#,#}{unit}";
    }

    public static Vector3 WorldToUI(Vector3 worldPos, Camera worldCam, Camera uiCam)
    {
        var vpPos = worldCam.WorldToViewportPoint(worldPos);
        var wdPos = uiCam.ViewportToWorldPoint(vpPos);
        wdPos.z = 0f;
        return wdPos;
    }
}
