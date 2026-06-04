using UnityEngine;
using System.IO;
using System;

public class MeasurementDataManager : MonoBehaviour
{
    public void SaveSessionData(string date, float chest, float waist, float hip, float arm, float leg)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Sessions");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        MeasurementSession newSession = new MeasurementSession();
        newSession.sessionId = System.Guid.NewGuid().ToString();

        // Dropdown menüsü için bugünün tarihini "31 Mayýs 2026" vb. alýyoruz
        newSession.dateTime = date;

        // HATA VEREN KISIMLAR DÜZELTÝLDÝ: Senin sýnýfýndaki "Cm" takýlarý eklendi
        newSession.chestCm = chest;
        newSession.waistCm = waist;
        newSession.hipCm = hip;
        newSession.armCm = arm;
        newSession.legCm = leg;

        string json = JsonUtility.ToJson(newSession, true);

        string fileName = "Session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        string filePath = Path.Combine(folderPath, fileName);

        File.WriteAllText(filePath, json);

        Debug.Log("<color=cyan>[JSON SÝSTEMÝ] BAÞARILI: Veri klasöre kaydedildi -> " + filePath + "</color>");
    }
}