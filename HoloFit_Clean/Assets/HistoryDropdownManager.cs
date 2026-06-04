using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using System.Linq;

public class HistoryDropdownManager : MonoBehaviour
{
    [Header("Dependencies")]
    public MeasurementLoadSystem loadSystem;
    public MeasurementComparisonSystem comparisonSystem;

    [Header("UI Dropdowns")]
    public TMP_Dropdown oldHistoryDropdown;
    public TMP_Dropdown newHistoryDropdown;

    private List<MeasurementSession> allSessions = new List<MeasurementSession>();
    private CultureInfo trCulture = new CultureInfo("tr-TR");

    void Start()
    {
        InitializeDropdowns();
    }

    public void InitializeDropdowns()
    {
        if (loadSystem == null || oldHistoryDropdown == null || newHistoryDropdown == null || comparisonSystem == null)
            return;

        allSessions = loadSystem.LoadAllSessions();

        if (allSessions.Count < 2)
        {
            oldHistoryDropdown.gameObject.SetActive(false);
            newHistoryDropdown.gameObject.SetActive(false);
            return;
        }

        UpdateDropdownOptions(oldHistoryDropdown, null);
        UpdateDropdownOptions(newHistoryDropdown, allSessions[allSessions.Count - 1].sessionId);

        oldHistoryDropdown.value = oldHistoryDropdown.options.Count - 1;
        newHistoryDropdown.value = 0;

        oldHistoryDropdown.onValueChanged.AddListener(delegate { OnOldDropdownChanged(); });
        newHistoryDropdown.onValueChanged.AddListener(delegate { OnNewDropdownChanged(); });

        TriggerComparison();
    }

    private void OnOldDropdownChanged()
    {
        string selectedOldId = GetSelectedId(oldHistoryDropdown);
        string currentNewId = GetSelectedId(newHistoryDropdown);

        UpdateDropdownOptions(newHistoryDropdown, selectedOldId, currentNewId);
        TriggerComparison();
    }

    private void OnNewDropdownChanged()
    {
        string selectedNewId = GetSelectedId(newHistoryDropdown);
        string currentOldId = GetSelectedId(oldHistoryDropdown);

        UpdateDropdownOptions(oldHistoryDropdown, selectedNewId, currentOldId);
        TriggerComparison();
    }

    private void UpdateDropdownOptions(TMP_Dropdown dropdown, string excludeId, string targetId = null)
    {
        dropdown.onValueChanged.RemoveAllListeners();

        dropdown.ClearOptions();
        List<string> options = new List<string>();
        List<MeasurementSession> filteredSessions = allSessions.Where(s => s.sessionId != excludeId).ToList();

        for (int i = filteredSessions.Count - 1; i >= 0; i--)
        {
            System.DateTime date = filteredSessions[i].GetParsedDateTime();
            options.Add(date.ToString("d MMMM yyyy", trCulture));
        }

        dropdown.AddOptions(options);

        if (targetId != null)
        {
            for (int i = 0; i < filteredSessions.Count; i++)
            {
                if (filteredSessions[(filteredSessions.Count - 1) - i].sessionId == targetId)
                {
                    dropdown.value = i;
                    break;
                }
            }
        }

        if (dropdown == oldHistoryDropdown)
            dropdown.onValueChanged.AddListener(delegate { OnOldDropdownChanged(); });
        else
            dropdown.onValueChanged.AddListener(delegate { OnNewDropdownChanged(); });
    }

    private string GetSelectedId(TMP_Dropdown dropdown)
    {
        string selectedText = dropdown.options[dropdown.value].text;
        foreach (var session in allSessions)
        {
            if (session.GetParsedDateTime().ToString("d MMMM yyyy", trCulture) == selectedText)
                return session.sessionId;
        }
        return null;
    }

    private void TriggerComparison()
    {
        string id1 = GetSelectedId(oldHistoryDropdown);
        string id2 = GetSelectedId(newHistoryDropdown);

        if (id1 != null && id2 != null)
        {
            MeasurementSession session1 = allSessions.Find(s => s.sessionId == id1);
            MeasurementSession session2 = allSessions.Find(s => s.sessionId == id2);

            // AKILLI ZAMAN KONTROLÜ: Hangi tarih kronolojik olarak daha eskiyse, onu her zaman "Eski" yuvaya koyar!
            if (session1.GetParsedDateTime() < session2.GetParsedDateTime())
            {
                comparisonSystem.LoadByIdsAndApply(session1.sessionId, session2.sessionId);
            }
            else
            {
                comparisonSystem.LoadByIdsAndApply(session2.sessionId, session1.sessionId);
            }

            comparisonSystem.RefreshDependentViews();
        }
    }
}