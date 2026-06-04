using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;

public enum MeasurementStep
{
    Front, Right, Back, Left, Done
}

public class BodyMeasurementCalculator : MonoBehaviour
{
    [Header("User Settings")]
    public float userHeightCm = 0f;

    [Header("UI & Pop-Up")]
    public TMP_Text measurementText;
    public TMP_Text stepInstructionText;
    public TMP_Text countdownText; // YENİ: Ekranda 5,4,3 yazacak metin
    public GameObject resultPopUpPanel;
    public TMP_InputField heightInputField;

    [Header("Calibration")]
    public float chestCalibration = 1.1f;
    public float waistCalibration = 1.1f;
    public float hipCalibration = 1.1f;
    public float armCalibration = 1.05f;
    public float legCalibration = 1.05f;

    [Header("Auto Capture (Otomatik Çekim)")]
    public float visibilityThreshold = 0.6f;
    public float countdownDuration = 5f; // Kaçtan geriye saysın?
    private bool isCountingDown = false;
    private Coroutine countdownCoroutine;

    private PoseLandmarkerRunner runner;
    public MeasurementStep currentStep = MeasurementStep.Front;

    private float frontChestW, frontWaistW, frontHipW, armL, legL;
    private float backChestW, backWaistW, backHipW;
    private float rightChestD, rightWaistD, rightHipD;
    private float leftChestD, leftWaistD, leftHipD;

    void Start()
    {
        runner = GetComponent<PoseLandmarkerRunner>();
        if (runner == null) runner = FindObjectOfType<PoseLandmarkerRunner>();

        if (resultPopUpPanel) resultPopUpPanel.SetActive(false);
        if (countdownText) countdownText.text = ""; // Başta boş olsun
        UpdateInstructionUI();
    }

    void Update()
    {
        // OTOMATİK ÇEKİM KONTROLÜ
        if (currentStep != MeasurementStep.Done && runner != null && runner.LatestResult.HasValue)
        {
            CheckVisibilityAndStartCountdown();
        }
    }

    private void CheckVisibilityAndStartCountdown()
    {
        var result = runner.LatestResult.Value;
        if (result.poseLandmarks == null || result.poseLandmarks.Count == 0) return;

        var landmarks = result.poseLandmarks[0].landmarks;

        // Kritik noktaların görünürlük kontrolü
        bool isBodyVisible = IsBodyFullyVisible(landmarks);

        if (isBodyVisible && !isCountingDown)
        {
            // Vücut tam görünüyor, geri sayımı başlat
            countdownCoroutine = StartCoroutine(AutoCaptureRoutine());
        }
        else if (!isBodyVisible && isCountingDown)
        {
            // Vücut kadrajdan çıktı, geri sayımı iptal et
            StopCountdown();
        }
    }

    private bool IsBodyFullyVisible(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        // Omuzlar, kalça, ayak bilekleri ve el bilekleri görünür olmalı
        int[] criticalPoints = { 11, 12, 23, 24, 27, 28, 15, 16 };
        foreach (int index in criticalPoints)
        {
            float vis = landmarks[index].visibility.HasValue ? landmarks[index].visibility.Value : 0f;
            if (vis < visibilityThreshold) return false;
        }
        return true;
    }

    IEnumerator AutoCaptureRoutine()
    {
        isCountingDown = true;
        float remainingTime = countdownDuration;

        while (remainingTime > 0)
        {
            if (countdownText) countdownText.text = remainingTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        if (countdownText) countdownText.text = "ÇEKİLDİ!";
        yield return new WaitForSeconds(0.5f);

        OnCaptureButtonClicked(); // Mevcut çekim fonksiyonunu tetikle

        if (countdownText) countdownText.text = "";
        isCountingDown = false;
    }

    private void StopCountdown()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        isCountingDown = false;
        if (countdownText) countdownText.text = "";
    }

    public void OnCaptureButtonClicked()
    {
        if (currentStep == MeasurementStep.Done) return;

        // Boy kontrolü
        if (userHeightCm <= 0)
        {
            if (heightInputField != null && float.TryParse(heightInputField.text, out float inputHeight))
                userHeightCm = inputHeight;

            if (userHeightCm <= 50f)
            {
                if (stepInstructionText) stepInstructionText.text = "<color=red>[HATA] Lütfen boyunuzu girin!</color>";
                return;
            }
        }

        if (!runner.LatestResult.HasValue) return;

        var landmarks = runner.LatestResult.Value.poseLandmarks[0].landmarks;

        // Manuel basıldığında da görünürlük kontrolü yapalım
        if (!IsBodyFullyVisible(landmarks))
        {
            if (stepInstructionText) stepInstructionText.text = "<color=red>[HATA] Vücut tam görünmüyor, çekim iptal!</color>";
            StopCountdown();
            return;
        }

        ProcessCurrentPose(landmarks);
    }

    private void ProcessCurrentPose(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        // Ölçüm mantığı aynı...
        var leftShoulder = landmarks[11]; var rightShoulder = landmarks[12];
        var leftHip = landmarks[23]; var rightHip = landmarks[24];
        var leftAnkle = landmarks[27]; var rightAnkle = landmarks[28];
        var leftWrist = landmarks[15]; var rightWrist = landmarks[16];

        float bodyHeightNorm = Mathf.Abs(landmarks[0].y - ((leftAnkle.y + rightAnkle.y) / 2f));
        float scale = (userHeightCm / bodyHeightNorm) * 1.15f;

        Vector2 lChest = Vector2.Lerp(new Vector2(leftShoulder.x, leftShoulder.y), new Vector2(leftHip.x, leftHip.y), 0.25f);
        Vector2 rChest = Vector2.Lerp(new Vector2(rightShoulder.x, rightShoulder.y), new Vector2(rightHip.x, rightHip.y), 0.25f);
        Vector2 lWaist = Vector2.Lerp(new Vector2(leftShoulder.x, leftShoulder.y), new Vector2(leftHip.x, leftHip.y), 0.55f);
        Vector2 rWaist = Vector2.Lerp(new Vector2(rightShoulder.x, rightShoulder.y), new Vector2(rightHip.x, rightHip.y), 0.55f);

        float currentWidth = Vector2.Distance(lChest, rChest) * scale;
        float currentWaist = Vector2.Distance(lWaist, rWaist) * scale;
        float currentHip = Vector2.Distance(new Vector2(leftHip.x, leftHip.y), new Vector2(rightHip.x, rightHip.y)) * scale;

        switch (currentStep)
        {
            case MeasurementStep.Front:
                frontChestW = currentWidth; frontWaistW = currentWaist; frontHipW = currentHip;
                armL = Vector2.Distance(new Vector2(leftShoulder.x, leftShoulder.y), new Vector2(leftWrist.x, leftWrist.y)) * scale;
                legL = Vector2.Distance(new Vector2(leftHip.x, leftHip.y), new Vector2(leftAnkle.x, leftAnkle.y)) * scale;
                currentStep = MeasurementStep.Right;
                break;
            case MeasurementStep.Right:
                rightChestD = currentWidth; rightWaistD = currentWaist; rightHipD = currentHip;
                currentStep = MeasurementStep.Back;
                break;
            case MeasurementStep.Back:
                backChestW = currentWidth; backWaistW = currentWaist; backHipW = currentHip;
                currentStep = MeasurementStep.Left;
                break;
            case MeasurementStep.Left:
                leftChestD = currentWidth; leftWaistD = currentWaist; leftHipD = currentHip;
                currentStep = MeasurementStep.Done;
                CalculateFinalMeasurements();
                break;
        }
        UpdateInstructionUI();
    }

    private void CalculateFinalMeasurements()
    {
        // Hesaplama mantığı aynı... (Kol, Göğüs, Bel, Kalça, Bacak)
        float avgChestW = (frontChestW + backChestW) / 2f;
        float avgWaistW = (frontWaistW + backWaistW) / 2f;
        float avgHipW = (frontHipW + backHipW) / 2f;

        float avgChestD = (rightChestD + leftChestD) / 2f;
        float avgWaistD = (rightWaistD + leftWaistD) / 2f;
        float avgHipD = (rightHipD + leftHipD) / 2f;

        float finalChest = CalculateEllipsePerimeter(avgChestW / 2f, avgChestD / 2f) * chestCalibration;
        float finalWaist = CalculateEllipsePerimeter(avgWaistW / 2f, avgWaistD / 2f) * waistCalibration;
        float finalHip = CalculateEllipsePerimeter(avgHipW / 2f, avgHipD / 2f) * hipCalibration;

        string finalResult = $"KOL: {armL * 1.05f:F1} cm\nGÖĞÜS: {finalChest:F1} cm\nBEL: {finalWaist:F1} cm\nKALÇA: {finalHip:F1} cm\nBACAK: {legL * 1.05f:F1} cm";
        if (measurementText) measurementText.text = finalResult;
        if (resultPopUpPanel) resultPopUpPanel.SetActive(true);
    }

    private float CalculateEllipsePerimeter(float a, float b) { return Mathf.PI * Mathf.Sqrt(2f * (a * a + b * b)); }

    private void UpdateInstructionUI()
    {
        if (stepInstructionText == null) return;
        string successTag = "<color=green>[BAŞARILI]</color> ";
        switch (currentStep)
        {
            case MeasurementStep.Front: stepInstructionText.text = "1/4: ÖNÜNÜZÜ dönün."; break;
            case MeasurementStep.Right: stepInstructionText.text = successTag + "2/4: SAĞ YANINIZI dönün."; break;
            case MeasurementStep.Back: stepInstructionText.text = successTag + "3/4: ARKANIZI dönün."; break;
            case MeasurementStep.Left: stepInstructionText.text = successTag + "4/4: SOL YANINIZI dönün."; break;
            case MeasurementStep.Done: stepInstructionText.text = successTag + "Ölçüm bitti!"; break;
        }
    }

    public void OnResetButtonClicked()
    {
        currentStep = MeasurementStep.Front;
        if (resultPopUpPanel) resultPopUpPanel.SetActive(false);
        StopCountdown();
        UpdateInstructionUI();
    }
}