using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string measurementSceneName = "MeasurementScene";
    [SerializeField] private string comparisonSceneName = "ComparisonScene_3DAnalysis";

    public void GoToMeasurementScene()
    {
        LoadScene(measurementSceneName);
    }

    public void GoToComparisonScene()
    {
        LoadScene(comparisonSceneName);
    }

    public void GoToComparison3DAnalysis()
    {
        LoadScene(comparisonSceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}