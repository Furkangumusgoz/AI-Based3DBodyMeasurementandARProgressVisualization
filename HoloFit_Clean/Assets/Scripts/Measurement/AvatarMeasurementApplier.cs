using System.Collections.Generic;
using UnityEngine;

public class AvatarMeasurementApplier : MonoBehaviour
{
    [Header("Bone References")]
    public Transform hips;
    public Transform spine;
    public Transform spine1;
    public Transform spine2;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftUpLeg;
    public Transform rightUpLeg;

    [Header("Reference Measurements")]
    public float baseShoulder = 30f;
    public float baseChest = 95f;
    public float baseWaist = 100f;
    public float baseHip = 110f;
    public float baseArm = 52f;
    public float baseLeg = 95f;

    [Header("Scale Strength")]
    [Range(0f, 1f)] public float shoulderStrength = 0.20f;
    [Range(0f, 1f)] public float chestStrength = 0.18f;
    [Range(0f, 1f)] public float waistStrength = 0.16f;
    [Range(0f, 1f)] public float hipStrength = 0.18f;
    [Range(0f, 1f)] public float armStrength = 0.15f;
    [Range(0f, 1f)] public float legStrength = 0.15f;

    private readonly Dictionary<Transform, Vector3> initialScales = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        CacheInitialScales();
    }

    public void CacheInitialScales()
    {
        initialScales.Clear();

        SaveInitialScale(hips);
        SaveInitialScale(spine);
        SaveInitialScale(spine1);
        SaveInitialScale(spine2);
        SaveInitialScale(leftArm);
        SaveInitialScale(rightArm);
        SaveInitialScale(leftUpLeg);
        SaveInitialScale(rightUpLeg);
    }

    private void SaveInitialScale(Transform target)
    {
        if (target != null && !initialScales.ContainsKey(target))
        {
            initialScales.Add(target, target.localScale);
        }
    }

    public void ResetAvatar()
    {
        foreach (var pair in initialScales)
        {
            if (pair.Key != null)
            {
                pair.Key.localScale = pair.Value;
            }
        }
    }

    public void ApplyMeasurements(MeasurementSession session)
    {
        if (session == null)
        {
            Debug.LogWarning("AvatarMeasurementApplier -> session null");
            return;
        }

        ResetAvatar();

        float shoulderFactor = ComputeFactor(session.shoulderCm, baseShoulder, shoulderStrength);
        float chestFactor = ComputeFactor(session.chestCm, baseChest, chestStrength);
        float waistFactor = ComputeFactor(session.waistCm, baseWaist, waistStrength);
        float hipFactor = ComputeFactor(session.hipCm, baseHip, hipStrength);
        float armFactor = ComputeFactor(session.armCm, baseArm, armStrength);
        float legFactor = ComputeFactor(session.legCm, baseLeg, legStrength);

        ApplyShoulder(shoulderFactor);
        ApplyChest(chestFactor);
        ApplyWaist(waistFactor);
        ApplyHip(hipFactor);
        ApplyArm(armFactor);
        ApplyLeg(legFactor);

        Debug.Log(
            $"Avatar uygulandý -> " +
            $"Shoulder:{session.shoulderCm:0.0}, " +
            $"Chest:{session.chestCm:0.0}, " +
            $"Waist:{session.waistCm:0.0}, " +
            $"Hip:{session.hipCm:0.0}, " +
            $"Arm:{session.armCm:0.0}, " +
            $"Leg:{session.legCm:0.0}"
        );
    }

    private float ComputeFactor(float currentValue, float baseValue, float strength)
    {
        if (baseValue <= 0.001f)
            return 1f;

        float ratio = currentValue / baseValue;
        return Mathf.Lerp(1f, ratio, strength);
    }

    private void ApplyShoulder(float factor)
    {
        // Omuz geniþliði üst gövdede daha mantýklý görünür
        ScaleX(spine2, factor);
        ScaleX(spine1, Mathf.Lerp(1f, factor, 0.6f));
    }

    private void ApplyChest(float factor)
    {
        ScaleXZ(spine2, factor);
        ScaleXZ(spine1, Mathf.Lerp(1f, factor, 0.7f));
    }

    private void ApplyWaist(float factor)
    {
        ScaleXZ(spine, factor);
    }

    private void ApplyHip(float factor)
    {
        ScaleXZ(hips, factor);
    }

    private void ApplyArm(float factor)
    {
        // Çok bozulmamasý için hafif uygula
        ScaleXYZ(leftArm, new Vector3(
            Mathf.Lerp(1f, factor, 0.6f),
            Mathf.Lerp(1f, factor, 0.35f),
            Mathf.Lerp(1f, factor, 0.6f)
        ));

        ScaleXYZ(rightArm, new Vector3(
            Mathf.Lerp(1f, factor, 0.6f),
            Mathf.Lerp(1f, factor, 0.35f),
            Mathf.Lerp(1f, factor, 0.6f)
        ));
    }

    private void ApplyLeg(float factor)
    {
        // Mixamo rig bozulmasýn diye çok sert deðil
        ScaleXYZ(leftUpLeg, new Vector3(
            Mathf.Lerp(1f, factor, 0.4f),
            Mathf.Lerp(1f, factor, 0.5f),
            Mathf.Lerp(1f, factor, 0.4f)
        ));

        ScaleXYZ(rightUpLeg, new Vector3(
            Mathf.Lerp(1f, factor, 0.4f),
            Mathf.Lerp(1f, factor, 0.5f),
            Mathf.Lerp(1f, factor, 0.4f)
        ));
    }

    private void ScaleX(Transform target, float factor)
    {
        if (target == null || !initialScales.ContainsKey(target))
            return;

        Vector3 baseScale = initialScales[target];
        target.localScale = new Vector3(
            baseScale.x * factor,
            baseScale.y,
            baseScale.z
        );
    }

    private void ScaleXZ(Transform target, float factor)
    {
        if (target == null || !initialScales.ContainsKey(target))
            return;

        Vector3 baseScale = initialScales[target];
        target.localScale = new Vector3(
            baseScale.x * factor,
            baseScale.y,
            baseScale.z * factor
        );
    }

    private void ScaleXYZ(Transform target, Vector3 factor)
    {
        if (target == null || !initialScales.ContainsKey(target))
            return;

        Vector3 baseScale = initialScales[target];
        target.localScale = new Vector3(
            baseScale.x * factor.x,
            baseScale.y * factor.y,
            baseScale.z * factor.z
        );
    }
}