using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using System.Collections.Generic;

public class MeasurementManager : MonoBehaviour
{
    [Header("Fotoğraf Kutuları")]
    public RawImage frontBox;
    public RawImage rightBox;
    public RawImage backBox;
    public RawImage leftBox;

    [Header("UI Panelleri")]
    public GameObject measurementInfoPanel;
    public TMP_Text infoPanelText;
    public TMP_InputField heightInput;
    public TMP_Text statusText;

    [Header("MediaPipe Ayarları")]
    public PoseLandmarkerRunner runner;

    [Header("Calibration (Kalibrasyon)")]
    public float chestCalibration = 0.55f;
    public float waistCalibration = 0.64f;
    public float hipCalibration = 0.95f;
    public float armCalibration = 0.65f;
    public float legCalibration = 0.93f;

    private float finalChest, finalWaist, finalHip, finalArm, finalLeg;
    private float frontChestW, frontWaistW, frontHipW, armL, legL;
    private float backChestW, backWaistW, backHipW;
    private float rightChestD, rightWaistD, rightHipD;
    private float leftChestD, leftWaistD, leftHipD;
    private float userHeightCm;

    void Start()
    {
        if (measurementInfoPanel) measurementInfoPanel.SetActive(false);
    }

    public void OnCaptureButtonPressed()
    {
        if (frontBox.texture == null || rightBox.texture == null ||
            backBox.texture == null || leftBox.texture == null)
        {
            if (statusText) statusText.text = "<color=red>Hata: Fotoğraflar eksik!</color>";
            return;
        }

        if (heightInput == null || !float.TryParse(heightInput.text, out userHeightCm))
        {
            if (statusText) statusText.text = "<color=red>Hata: Boy girilmedi!</color>";
            return;
        }

        if (statusText) statusText.text = "<color=yellow>Analiz ediliyor...</color>";
        ProcessAllImages();
    }

    private void ProcessAllImages()
    {
        if (!ProcessSingle(frontBox.texture, "Ön", CalculateFront)) return;
        if (!ProcessSingle(rightBox.texture, "Sağ", CalculateRight)) return;
        if (!ProcessSingle(backBox.texture, "Arka", CalculateBack)) return;
        if (!ProcessSingle(leftBox.texture, "Sol", CalculateLeft)) return;

        FinalizeMeasurements();
    }

    private bool ProcessSingle(Texture texture, string poseName, System.Action<IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark>> calculateAction)
    {
        var result = runner.AnalyzeStaticImage((Texture2D)texture);
        if (result != null && result.Value.poseLandmarks != null && result.Value.poseLandmarks.Count > 0)
        {
            calculateAction(result.Value.poseLandmarks[0].landmarks);
            return true;
        }
        statusText.text = $"<color=red>Hata: {poseName} profilinde vücut algılanamadı!</color>";
        return false;
    }

    #region Matematik
    private void CalculateFront(IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> lm)
    {
        float scale = GetScale(lm);
        frontChestW = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.25f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.25f)) * scale;
        frontWaistW = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.55f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.55f)) * scale;
        frontHipW = Vector2.Distance(V(lm[23]), V(lm[24])) * scale;
        armL = Vector2.Distance(V(lm[11]), V(lm[15])) * scale;
        legL = Vector2.Distance(V(lm[23]), V(lm[27])) * scale;
    }
    private void CalculateRight(IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> lm)
    {
        float scale = GetScale(lm);
        rightChestD = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.25f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.25f)) * scale;
        rightWaistD = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.55f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.55f)) * scale;
        rightHipD = Vector2.Distance(V(lm[23]), V(lm[24])) * scale;
    }
    private void CalculateBack(IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> lm)
    {
        float scale = GetScale(lm);
        backChestW = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.25f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.25f)) * scale;
        backWaistW = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.55f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.55f)) * scale;
        backHipW = Vector2.Distance(V(lm[23]), V(lm[24])) * scale;
    }
    private void CalculateLeft(IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> lm)
    {
        float scale = GetScale(lm);
        leftChestD = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.25f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.25f)) * scale;
        leftWaistD = Vector2.Distance(Vector2.Lerp(V(lm[11]), V(lm[23]), 0.55f), Vector2.Lerp(V(lm[12]), V(lm[24]), 0.55f)) * scale;
        leftHipD = Vector2.Distance(V(lm[23]), V(lm[24])) * scale;
    }
    private float GetScale(IReadOnlyList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> lm)
    {
        float h = Mathf.Abs(lm[0].y - ((lm[27].y + lm[28].y) / 2f));
        return (userHeightCm / h) * 1.15f;
    }
    private Vector2 V(Mediapipe.Tasks.Components.Containers.NormalizedLandmark l) { return new Vector2(l.x, l.y); }
    private float CalculateEllipsePerimeter(float a, float b) { return Mathf.PI * Mathf.Sqrt(2f * (a * a + b * b)); }
    #endregion

    private void FinalizeMeasurements()
    {
        finalChest = CalculateEllipsePerimeter((frontChestW + backChestW) / 4f, (rightChestD + leftChestD) / 4f) * chestCalibration;
        finalWaist = CalculateEllipsePerimeter((frontWaistW + backWaistW) / 4f, (rightWaistD + leftWaistD) / 4f) * waistCalibration;
        finalHip = CalculateEllipsePerimeter((frontHipW + backHipW) / 4f, (rightHipD + leftHipD) / 4f) * hipCalibration;
        finalArm = armL * armCalibration;
        finalLeg = legL * legCalibration;

        if (measurementInfoPanel) measurementInfoPanel.SetActive(true);

        string results = $"<color=#00FF00><b>VÜCUT ANALİZ SONUÇLARI</b></color>\n\n" +
                         $"KOL: {finalArm:F1} cm\n\n" +
                         $"GÖĞÜS: {finalChest:F1} cm\n\n" +
                         $"BEL: {finalWaist:F1} cm\n\n" +
                         $"KALÇA: {finalHip:F1} cm\n\n" +
                         $"BACAK: {finalLeg:F1} cm";

        if (infoPanelText) infoPanelText.text = results;
        if (statusText) statusText.text = "<color=green>Analiz Tamamlandı.</color>";
    }

    public void OnResetButtonPressed()
    {
        frontBox.texture = null; rightBox.texture = null; backBox.texture = null; leftBox.texture = null;
        if (heightInput) heightInput.text = "";
        if (statusText) statusText.text = "Sistem sıfırlandı.";
        if (measurementInfoPanel) measurementInfoPanel.SetActive(false);
    }

    public void On3DAnalizButtonPressed()
    {
        if (finalChest > 0)
        {
            MeasurementSession yeniOlcum = new MeasurementSession();

            // İŞTE DÜZELTİLEN KISIM: Başlarına System. ekledik, artık şıp diye tanıyacak!
            yeniOlcum.sessionId = System.Guid.NewGuid().ToString();
            yeniOlcum.dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            yeniOlcum.userHeightCm = userHeightCm;
            yeniOlcum.chestCm = finalChest;
            yeniOlcum.waistCm = finalWaist;
            yeniOlcum.hipCm = finalHip;
            yeniOlcum.armCm = finalArm;
            yeniOlcum.legCm = finalLeg;
            yeniOlcum.shoulderCm = 0;

            // Senin orijinal kayıt sistemin
            MeasurementSaveSystem.SaveSession(yeniOlcum);

            // Anlık geçiş için PlayerPrefs
            PlayerPrefs.SetFloat("ChestSize", finalChest);
            PlayerPrefs.SetFloat("WaistSize", finalWaist);
            PlayerPrefs.SetFloat("HipSize", finalHip);
            PlayerPrefs.SetFloat("ArmSize", finalArm);
            PlayerPrefs.SetFloat("LegSize", finalLeg);
            PlayerPrefs.Save();

            // Sahne Geçişi
            SceneManager.LoadScene("AvatarScene");
        }
    }
}