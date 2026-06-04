using System;
using UnityEngine;

[Serializable]
public class MeasurementSession
{
    public string sessionId;
    public string dateTime;

    public float userHeightCm;
    public float shoulderCm;
    public float chestCm;
    public float waistCm;
    public float hipCm;
    public float armCm;
    public float legCm;

    public DateTime GetParsedDateTime()
    {
        if (string.IsNullOrWhiteSpace(dateTime))
            return DateTime.MinValue;

        string[] formats =
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyyMMdd_HHmmss",
            "yyyy-MM-ddTHH:mm:ss",
            "dd.MM.yyyy HH:mm:ss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(
                    dateTime,
                    format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsed))
            {
                return parsed;
            }
        }

        if (DateTime.TryParse(dateTime, out DateTime fallback))
            return fallback;

        return DateTime.MinValue;
    }

    public override string ToString()
    {
        return $"{sessionId} | {dateTime} | Shoulder:{shoulderCm} Chest:{chestCm} Waist:{waistCm} Hip:{hipCm} Arm:{armCm} Leg:{legCm}";
    }
}