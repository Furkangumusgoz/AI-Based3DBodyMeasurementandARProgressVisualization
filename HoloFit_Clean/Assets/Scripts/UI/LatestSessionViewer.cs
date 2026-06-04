using TMPro;
using UnityEngine;

public class LatestSessionViewer : MonoBehaviour
{
    [Header("References")]
    public MeasurementLoadSystem loadSystem;
    public TMP_Text targetText;

    [Header("Auto Refresh")]
    public bool refreshOnStart = true;

    private void Start()
    {
        if (refreshOnStart)
            RefreshLatestSession();
    }

    public void RefreshLatestSession()
    {
        if (targetText == null)
        {
            Debug.LogWarning("LatestSessionViewer -> targetText atanmadý.");
            return;
        }

        if (loadSystem == null)
        {
            targetText.text = "Load system atanmadý.";
            Debug.LogWarning("LatestSessionViewer -> loadSystem atanmadý.");
            return;
        }

        MeasurementSession session = loadSystem.LoadLatestSession();

        if (session == null)
        {
            targetText.text = "Henüz kayýt bulunamadý.";
            return;
        }

        targetText.text =
            "Son Kayit\n" +
            "Tarih: " + session.dateTime + "\n" +
            "Boy: " + session.userHeightCm.ToString("0.0") + " cm\n" +
            "Shoulder: " + session.shoulderCm.ToString("0.0") + " cm\n" +
            "Chest: " + session.chestCm.ToString("0.0") + " cm\n" +
            "Waist: " + session.waistCm.ToString("0.0") + " cm\n" +
            "Hip: " + session.hipCm.ToString("0.0") + " cm\n" +
            "Arm: " + session.armCm.ToString("0.0") + " cm\n" +
            "Leg: " + session.legCm.ToString("0.0") + " cm";
    }
}