using System;
using System.IO;
using UnityEngine;

public static class MeasurementSaveSystem
{
    public static void SaveSession(MeasurementSession session)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Sessions");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 1. Yeni yapýlan ölçümün sadece 'Gün-Ay-Yýl' kýsmýný alýyoruz (Saat/Dakika görmezden gelinir)
        DateTime newSessionDate = session.GetParsedDateTime().Date;

        // 2. Klasördeki kayýtlý tüm eski dosyalarý tarýyoruz
        string[] existingFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (string file in existingFiles)
        {
            try
            {
                string jsonContent = File.ReadAllText(file);
                MeasurementSession existingSession = JsonUtility.FromJson<MeasurementSession>(jsonContent);

                if (existingSession != null)
                {
                    DateTime existingDate = existingSession.GetParsedDateTime().Date;

                    // 3. EÐER BULUNAN DOSYA BUGÜNE AÝTSE -> ACIMADAN SÝL!
                    if (existingDate == newSessionDate)
                    {
                        File.Delete(file);
                        Debug.Log("Ayný güne ait eski ölçüm otomatik silindi: " + file);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Dosya kontrol edilirken hata: " + ex.Message);
            }
        }

        // 4. Evi tertemiz yaptýk, þimdi yeni ve en güncel ölçümümüzü tahta oturtuyoruz
        string json = JsonUtility.ToJson(session, true);
        string fileName = $"session_{session.sessionId}.json";
        string fullPath = Path.Combine(folderPath, fileName);

        File.WriteAllText(fullPath, json);

        Debug.Log("Yeni Ölçüm baþarýyla kaydedildi: " + fullPath);
    }
}