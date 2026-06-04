using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeasurementLoadSystem : MonoBehaviour
{
    [Header("Storage")]
    public string sessionsFolderName = "Sessions";

    [Header("Debug")]
    public bool logLoadedSessions = true;

    public string GetSessionsFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, sessionsFolderName);
    }

    public List<MeasurementSession> LoadAllSessions()
    {
        List<MeasurementSession> sessions = new List<MeasurementSession>();
        string folderPath = GetSessionsFolderPath();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Sessions klasörü bulunamadý: " + folderPath);
            return sessions;
        }

        string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                MeasurementSession session = JsonUtility.FromJson<MeasurementSession>(json);

                if (session != null && !string.IsNullOrWhiteSpace(session.sessionId))
                {
                    sessions.Add(session);

                    if (logLoadedSessions)
                        Debug.Log("Session yüklendi: " + session.sessionId + " | " + session.dateTime);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Session okunamadý: " + file + "\n" + ex.Message);
            }
        }

        sessions.Sort((a, b) => a.GetParsedDateTime().CompareTo(b.GetParsedDateTime()));
        return sessions;
    }

    public MeasurementSession LoadLatestSession()
    {
        List<MeasurementSession> sessions = LoadAllSessions();
        if (sessions.Count == 0)
            return null;

        return sessions[sessions.Count - 1];
    }

    public bool LoadLatestTwoSessions(out MeasurementSession oldSession, out MeasurementSession newSession)
    {
        oldSession = null;
        newSession = null;

        List<MeasurementSession> sessions = LoadAllSessions();

        if (sessions.Count < 2)
            return false;

        oldSession = sessions[sessions.Count - 2];
        newSession = sessions[sessions.Count - 1];
        return true;
    }

    public MeasurementSession FindSessionById(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        List<MeasurementSession> sessions = LoadAllSessions();

        foreach (MeasurementSession session in sessions)
        {
            if (session.sessionId == sessionId)
                return session;
        }

        return null;
    }
}