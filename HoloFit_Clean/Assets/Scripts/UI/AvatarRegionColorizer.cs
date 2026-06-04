using UnityEngine;
using TMPro; // YAZILARI KONTROL ETMEK ÝÇÝN BU KÜTÜPHANE ÞART

public class AvatarRegionColorizer : MonoBehaviour
{
    [Header("Dependencies")]
    public MeasurementComparisonSystem comparisonSystem;

    [Header("Avatar Region Controllers")]
    public RegionColorController shoulderMarker;
    public RegionColorController chestMarker;
    public RegionColorController waistMarker;
    public RegionColorController hipMarker;
    public RegionColorController armMarker;
    public RegionColorController legMarker;

    [Header("UI Text Reset (YENÝ EKLENDÝ)")]
    public TMP_Text latestSessionText; // Eski tarihlerin yazdýðý yer
    public TMP_Text comparisonText;    // Sonuçlarýn (AZALDI/ARTTI) yazdýðý yer

    [Header("Colors")]
    public Color decreasedColor = Color.green;
    public Color increasedColor = Color.red;
    public Color sameColor = Color.yellow;
    public Color defaultNeutralColor = Color.gray; // Reset için nötr renk

    [Header("Tolerance")]
    public float changeTolerance = 0.5f;

    public void RefreshColors()
    {
        if (comparisonSystem == null || comparisonSystem.latestResult == null) return;

        var result = comparisonSystem.latestResult;
        ApplyColor(shoulderMarker, GetColor(result.shoulderDelta));
        ApplyColor(chestMarker, GetColor(result.chestDelta));
        ApplyColor(waistMarker, GetColor(result.waistDelta));
        ApplyColor(hipMarker, GetColor(result.hipDelta));
        ApplyColor(armMarker, GetColor(result.armDelta));
        ApplyColor(legMarker, GetColor(result.legDelta));
    }

    // --- GÜNCELLENMÝÞ RESET FONKSÝYONU ---
    public void ResetAvatar()
    {
        // 1. Avatarýn renklerini griye (nötr) çevir
        ApplyColor(shoulderMarker, defaultNeutralColor);
        ApplyColor(chestMarker, defaultNeutralColor);
        ApplyColor(waistMarker, defaultNeutralColor);
        ApplyColor(hipMarker, defaultNeutralColor);
        ApplyColor(armMarker, defaultNeutralColor);
        ApplyColor(legMarker, defaultNeutralColor);

        // 2. Ekranda kalan eski yazýlarý temizle ve þýk bir mesaj ver
        if (latestSessionText != null)
        {
            latestSessionText.text = "Kayýt Bekleniyor...";
        }

        if (comparisonText != null)
        {
            // \n kodu yazýyý bir alt satýra geçirir
            comparisonText.text = "Sonuçlar sýfýrlandý.\nLütfen yeni bir analiz için ölçüm yapýnýz.";
        }

        Debug.Log("Avatar ve Yazýlar baþarýyla sýfýrlandý!");
    }

    private Color GetColor(float delta)
    {
        if (Mathf.Abs(delta) <= changeTolerance) return sameColor;
        else if (delta < -changeTolerance) return decreasedColor;
        else return increasedColor;
    }

    private void ApplyColor(RegionColorController target, Color color)
    {
        if (target != null) target.SetColor(color);
    }
}