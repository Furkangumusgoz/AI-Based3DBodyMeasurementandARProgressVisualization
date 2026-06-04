using UnityEngine;

/// <summary>
/// Ölçüm farklarýný hesaplar ve ilgili body region objelerine renk uygular.
/// Ýlk versiyon inspector test deðerleriyle çalýþýr.
/// Daha sonra JSON/session data buraya baðlanacak.
/// </summary>
public class AvatarDifferenceVisualizer : MonoBehaviour
{
    [System.Serializable]
    public class MeasurementPair
    {
        public string regionName;

        [Header("Measurement Values")]
        public float oldValue;
        public float newValue;

        [Header("Visual Target")]
        public RegionColorController[] regionVisuals;
    }

    private enum DifferenceState
    {
        Increased,
        Decreased,
        Same
    }

    [Header("Measurement Difference Data")]
    [SerializeField] private MeasurementPair shoulder = new MeasurementPair { regionName = "Shoulder", oldValue = 42f, newValue = 44f };
    [SerializeField] private MeasurementPair chest = new MeasurementPair { regionName = "Chest", oldValue = 96f, newValue = 98f };
    [SerializeField] private MeasurementPair waist = new MeasurementPair { regionName = "Waist", oldValue = 84f, newValue = 80f };
    [SerializeField] private MeasurementPair hip = new MeasurementPair { regionName = "Hip", oldValue = 98f, newValue = 98.5f };
    [SerializeField] private MeasurementPair arm = new MeasurementPair { regionName = "Arm", oldValue = 31f, newValue = 33f };
    [SerializeField] private MeasurementPair leg = new MeasurementPair { regionName = "Leg", oldValue = 88f, newValue = 87.5f };

    [Header("Difference Settings")]
    [SerializeField] private float sameThresholdCm = 1.5f;

    [Header("Difference Colors")]
    [Tooltip("Measurement increased. Example: chest +2 cm.")]
    [SerializeField] private Color increasedColor = new Color(0.21f, 0.79f, 0.42f, 0.92f);

    [Tooltip("Measurement decreased. Example: waist -4 cm.")]
    [SerializeField] private Color decreasedColor = new Color(0.90f, 0.32f, 0.32f, 0.92f);

    [Tooltip("Measurement is almost same / stable.")]
    [SerializeField] private Color sameColor = new Color(0.90f, 0.73f, 0.24f, 0.88f);

    [Header("Debug")]
    [SerializeField] private bool applyOnStart = true;

    private void Start()
    {
        if (applyOnStart)
            ApplyAllRegionColors();
    }
    [ContextMenu("Force Apply Difference Colors")]
    public void ForceApplyDifferenceColors()
    {
        ApplyAllRegionColors();
    }


    public void ApplyAllRegionColors()
    {
        ApplyRegionColor(shoulder);
        ApplyRegionColor(chest);
        ApplyRegionColor(waist);
        ApplyRegionColor(hip);
        ApplyRegionColor(arm);
        ApplyRegionColor(leg);
    }

    private void ApplyRegionColor(MeasurementPair pair)
    {
        if (pair == null || pair.regionVisuals == null)
            return;

        float delta = pair.newValue - pair.oldValue;
        DifferenceState state = GetDifferenceState(delta);
        Color targetColor = GetColorForState(state);

        for (int i = 0; i < pair.regionVisuals.Length; i++)
        {
            if (pair.regionVisuals[i] != null)
                pair.regionVisuals[i].SetColor(targetColor);
        }

        Debug.Log($"{pair.regionName}: Old={pair.oldValue}, New={pair.newValue}, Delta={delta}, State={state}");
    }

    private DifferenceState GetDifferenceState(float delta)
    {
        if (delta > sameThresholdCm)
            return DifferenceState.Increased;

        if (delta < -sameThresholdCm)
            return DifferenceState.Decreased;

        return DifferenceState.Same;
    }

    private Color GetColorForState(DifferenceState state)
    {
        switch (state)
        {
            case DifferenceState.Increased:
                return increasedColor;

            case DifferenceState.Decreased:
                return decreasedColor;

            default:
                return sameColor;
        }
    }

    public void SetMeasurementData(
        float shoulderOld, float shoulderNew,
        float chestOld, float chestNew,
        float waistOld, float waistNew,
        float hipOld, float hipNew,
        float armOld, float armNew,
        float legOld, float legNew)
    {
        shoulder.oldValue = shoulderOld;
        shoulder.newValue = shoulderNew;

        chest.oldValue = chestOld;
        chest.newValue = chestNew;

        waist.oldValue = waistOld;
        waist.newValue = waistNew;

        hip.oldValue = hipOld;
        hip.newValue = hipNew;

        arm.oldValue = armOld;
        arm.newValue = armNew;

        leg.oldValue = legOld;
        leg.newValue = legNew;

        ApplyAllRegionColors();
    }
}