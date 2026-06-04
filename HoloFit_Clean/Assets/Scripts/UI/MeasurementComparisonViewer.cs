using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

public class MeasurementComparisonViewer : MonoBehaviour
{
    [Header("References")]
    public MeasurementComparisonSystem comparisonSystem;
    public TMP_Text comparisonText;

    [Header("Auto Refresh")]
    public bool refreshOnStart = true;

    [Header("TMP Rich Text Colors")]
    public string titleColor = "#FFFFFF";
    public string decreasedColor = "#00C853";
    public string increasedColor = "#FF1744";
    public string sameColor = "#FFD600";

    private void Start()
    {
        if (refreshOnStart)
            RefreshView();
    }

    public void RefreshView()
    {
        if (comparisonText == null || comparisonSystem == null) return;

        MeasurementComparisonResult result = comparisonSystem.latestResult;

        if (result == null || string.IsNullOrWhiteSpace(result.oldDateTime) || string.IsNullOrWhiteSpace(result.newDateTime))
        {
            comparisonText.text = "Karþýlaþtýrma için en az 2 kayýt gerekli.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Colorize("Ölçüm Karþýlaþtýrmasý", titleColor));
        sb.AppendLine();

        CultureInfo trCulture = new CultureInfo("tr-TR");

        string formattedOldDate = result.oldDateTime;
        if (DateTime.TryParse(result.oldDateTime, out DateTime oldDate))
            formattedOldDate = oldDate.ToString("d MMMM yyyy", trCulture);

        string formattedNewDate = result.newDateTime;
        if (DateTime.TryParse(result.newDateTime, out DateTime newDate))
            formattedNewDate = newDate.ToString("d MMMM yyyy", trCulture);

        sb.AppendLine("Eski: " + formattedOldDate);
        sb.AppendLine("Yeni: " + formattedNewDate);
        sb.AppendLine();

        // REÝSÝN ÝSTEDÝÐÝ KESÝN SIRALAMA VE OMUZ ÝPTALÝ
        AppendDeltaLine(sb, "Kol", result.armDelta);
        AppendDeltaLine(sb, "Göðüs", result.chestDelta);
        AppendDeltaLine(sb, "Bel", result.waistDelta);
        AppendDeltaLine(sb, "Kalça", result.hipDelta);
        AppendDeltaLine(sb, "Bacak", result.legDelta);

        comparisonText.text = sb.ToString();
    }

    private void AppendDeltaLine(StringBuilder sb, string label, float delta)
    {
        string color = GetColorForDelta(delta);
        string stateText = GetStateText(delta);

        string line = $"{label}: {stateText} {delta:+0.0;-0.0;0.0} cm";
        sb.AppendLine(Colorize(line, color));
    }

    private string GetColorForDelta(float delta)
    {
        if (comparisonSystem == null) return sameColor;
        string state = comparisonSystem.GetRegionState(delta);
        switch (state)
        {
            case "decreased": return decreasedColor;
            case "increased": return increasedColor;
            default: return sameColor;
        }
    }

    private string GetStateText(float delta)
    {
        if (comparisonSystem == null) return "AYNI";
        string state = comparisonSystem.GetRegionState(delta);
        switch (state)
        {
            case "decreased": return "AZALDI";
            case "increased": return "ARTTI";
            default: return "AYNI";
        }
    }

    private string Colorize(string text, string hexColor)
    {
        return $"<color={hexColor}>{text}</color>";
    }
}