using UnityEngine;

/// <summary>
/// Old / New / Difference avatar görünürlük modlarýný yönetir.
/// Bu script Managers/AvatarModeController objesine eklenir.
/// </summary>
public class AvatarModeController : MonoBehaviour
{
    public enum AvatarViewMode
    {
        Old,
        New,
        Difference
    }

    [Header("Avatar References")]
    [SerializeField] private GameObject oldAvatar;
    [SerializeField] private GameObject newAvatar;
    [SerializeField] private GameObject differenceAvatar;

    [Header("Startup")]
    [SerializeField] private AvatarViewMode defaultMode = AvatarViewMode.Difference;

    public AvatarViewMode CurrentMode { get; private set; }

    private void Start()
    {
        SetMode(defaultMode);
    }

    public void ShowOld()
    {
        SetMode(AvatarViewMode.Old);
    }

    public void ShowNew()
    {
        SetMode(AvatarViewMode.New);
    }

    public void ShowDifference()
    {
        SetMode(AvatarViewMode.Difference);
    }

    public void SetMode(AvatarViewMode mode)
    {
        CurrentMode = mode;

        if (oldAvatar != null)
            oldAvatar.SetActive(mode == AvatarViewMode.Old);

        if (newAvatar != null)
            newAvatar.SetActive(mode == AvatarViewMode.New);

        if (differenceAvatar != null)
            differenceAvatar.SetActive(mode == AvatarViewMode.Difference);
    }
}