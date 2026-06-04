using UnityEngine;

/// <summary>
/// Body region overlay objesinin (Hologram materyal yuvalarýnýn) rengini ve parlaklýðýný kontrol eder.
/// AvatarDifferenceVisualizer bu script'e SetColor() çaðýrýr.
/// </summary>
public class RegionColorController : MonoBehaviour
{
    [Header("Renderer Settings")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("Bu bölgenin Element numarasý (Örn: Kollar Element 1 ise buraya 1 yazýn)")]
    [SerializeField] private int materialIndex = 0; // YENÝ: Hangi yuvayý kontrol edeceðini seçeceðiz

    [Header("Material")]
    [SerializeField] private bool createMaterialInstance = true;

    [Header("Emission")]
    [SerializeField] private bool useEmission = true;

    [Range(0f, 5f)]
    [SerializeField] private float emissionIntensity = 2.2f;

    [Header("Alpha (Hologram Þeffaflýðý)")]
    [SerializeField] private bool overrideAlpha = true;

    [Range(0f, 1f)]
    [SerializeField] private float fixedAlpha = 0.3f; // Hologram için default deðeri düþürdük

    private Material runtimeMaterial;

    private void Awake()
    {
        SetupMaterial();
    }

    private void OnEnable()
    {
        SetupMaterial();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
    }

    private void SetupMaterial()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name}: RegionColorController needs a Renderer.");
            return;
        }

        if (runtimeMaterial != null)
            return;

        if (createMaterialInstance)
        {
            // Orijinal materyal dizisinin bir kopyasýný al
            Material[] mats = targetRenderer.materials;

            // Güvenlik: Verdiðimiz index, listedeki yuva sayýsýndan büyük olmasýn
            if (materialIndex >= 0 && materialIndex < mats.Length)
            {
                runtimeMaterial = mats[materialIndex];
            }
            else
            {
                Debug.LogError($"{name}: Material Index ({materialIndex}) is out of bounds!");
                return;
            }
        }
        else
        {
            Material[] sharedMats = targetRenderer.sharedMaterials;
            if (materialIndex >= 0 && materialIndex < sharedMats.Length)
                runtimeMaterial = sharedMats[materialIndex];
        }

        if (runtimeMaterial != null)
            runtimeMaterial.EnableKeyword("_EMISSION");
    }

    public void SetColor(Color color)
    {
        SetupMaterial();

        if (runtimeMaterial == null)
            return;

        Color finalColor = color;

        if (overrideAlpha)
        {
            finalColor.a = fixedAlpha;
            // Hologram shader'ýmýzdaki özel _Alpha deðerini de tetikle
            if (runtimeMaterial.HasProperty("_Alpha"))
            {
                runtimeMaterial.SetFloat("_Alpha", fixedAlpha);
            }
        }

        runtimeMaterial.SetColor("_Color", finalColor);

        if (useEmission)
        {
            runtimeMaterial.EnableKeyword("_EMISSION");

            Color emissionColor = new Color(
                finalColor.r * emissionIntensity,
                finalColor.g * emissionIntensity,
                finalColor.b * emissionIntensity,
                1f
            );

            runtimeMaterial.SetColor("_EmissionColor", emissionColor);
        }

        // Debug.Log($"{name} color applied: {finalColor} to Material Index: {materialIndex}");
    }

    public void SetEmissionIntensity(float intensity)
    {
        emissionIntensity = Mathf.Max(0f, intensity);
    }
}