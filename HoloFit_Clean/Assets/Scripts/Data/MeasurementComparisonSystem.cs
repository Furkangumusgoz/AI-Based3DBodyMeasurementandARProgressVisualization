using UnityEngine;

public class MeasurementComparisonSystem : MonoBehaviour
{
    [Header("Dependencies")]
    public MeasurementLoadSystem loadSystem;

    [Header("Avatar References")]
    public AvatarMeasurementApplier oldAvatar;
    public AvatarMeasurementApplier newAvatar;

    [Header("Optional View Refresh")]
    public MeasurementComparisonViewer comparisonViewer;
    public AvatarRegionColorizer regionColorizer;

    [Header("Session Selection")]
    public bool autoLoadLatestTwoOnStart = true;
    public string oldSessionId;
    public string newSessionId;

    [Header("Threshold")]
    public float sameThresholdCm = 1.0f;

    [Header("Runtime Info")]
    public MeasurementSession oldSession;
    public MeasurementSession newSession;
    public MeasurementComparisonResult latestResult;

    private void Start()
    {
        bool success = false;

        if (autoLoadLatestTwoOnStart)
        {
            success = LoadLatestTwoAndApply();
        }
        else if (!string.IsNullOrWhiteSpace(oldSessionId) && !string.IsNullOrWhiteSpace(newSessionId))
        {
            success = LoadByIdsAndApply(oldSessionId, newSessionId);
        }

        if (success)
        {
            RefreshDependentViews();
        }
    }

    public bool LoadLatestTwoAndApply()
    {
        if (loadSystem == null)
        {
            Debug.LogError("MeasurementComparisonSystem -> loadSystem atanmadý.");
            latestResult = null;
            return false;
        }

        if (!loadSystem.LoadLatestTwoSessions(out oldSession, out newSession))
        {
            Debug.LogWarning("Karþýlaþtýrma için en az 2 session gerekli.");
            latestResult = null;
            return false;
        }

        ApplySessionsToAvatars();
        latestResult = BuildResult(oldSession, newSession);

        Debug.Log("Comparison old: " + oldSession.sessionId + " | " + oldSession.dateTime);
        Debug.Log("Comparison new: " + newSession.sessionId + " | " + newSession.dateTime);

        return true;
    }

    public bool LoadByIdsAndApply(string oldId, string newId)
    {
        if (loadSystem == null)
        {
            Debug.LogError("MeasurementComparisonSystem -> loadSystem atanmadý.");
            latestResult = null;
            return false;
        }

        oldSession = loadSystem.FindSessionById(oldId);
        newSession = loadSystem.FindSessionById(newId);

        if (oldSession == null || newSession == null)
        {
            Debug.LogWarning("Session bulunamadý. old=" + oldId + " new=" + newId);
            latestResult = null;
            return false;
        }

        ApplySessionsToAvatars();
        latestResult = BuildResult(oldSession, newSession);

        Debug.Log("Comparison old: " + oldSession.sessionId + " | " + oldSession.dateTime);
        Debug.Log("Comparison new: " + newSession.sessionId + " | " + newSession.dateTime);

        return true;
    }

    public void ApplySessionsToAvatars()
    {
        if (oldAvatar != null && oldSession != null)
            oldAvatar.ApplyMeasurements(oldSession);

        if (newAvatar != null && newSession != null)
            newAvatar.ApplyMeasurements(newSession);
    }

    public MeasurementComparisonResult BuildResult(MeasurementSession oldData, MeasurementSession newData)
    {
        if (oldData == null || newData == null)
            return null;

        MeasurementComparisonResult result = new MeasurementComparisonResult();

        result.oldSessionId = oldData.sessionId;
        result.newSessionId = newData.sessionId;

        result.oldDateTime = oldData.dateTime;
        result.newDateTime = newData.dateTime;

        result.shoulderDelta = newData.shoulderCm - oldData.shoulderCm;
        result.chestDelta = newData.chestCm - oldData.chestCm;
        result.waistDelta = newData.waistCm - oldData.waistCm;
        result.hipDelta = newData.hipCm - oldData.hipCm;
        result.armDelta = newData.armCm - oldData.armCm;
        result.legDelta = newData.legCm - oldData.legCm;

        return result;
    }

    public string GetRegionState(float delta)
    {
        if (Mathf.Abs(delta) < sameThresholdCm)
            return "same";

        if (delta < 0f)
            return "decreased";

        return "increased";
    }

    public void RefreshDependentViews()
    {
        if (comparisonViewer != null)
            comparisonViewer.RefreshView();

        if (regionColorizer != null)
            regionColorizer.RefreshColors();
    }
}