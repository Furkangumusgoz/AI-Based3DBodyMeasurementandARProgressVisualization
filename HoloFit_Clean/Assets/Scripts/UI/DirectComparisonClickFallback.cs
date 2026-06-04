using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectComparisonClickFallback : MonoBehaviour
{
    [Header("Click Area")]
    [SerializeField] private RectTransform comparisonButtonRect;

    [Header("Target Scene")]
    [SerializeField] private string targetSceneName = "ComparisonScene_3DAnalysis";

    [Header("Debug")]
    [SerializeField] private bool logClicks = true;

    private void Update()
    {
        if (comparisonButtonRect == null)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosition = Input.mousePosition;

            bool clickedInside = RectTransformUtility.RectangleContainsScreenPoint(
                comparisonButtonRect,
                mousePosition,
                null
            );

            if (clickedInside)
            {
                if (logClicks)
                    Debug.Log("Direct comparison click detected. Loading: " + targetSceneName);

                SceneManager.LoadScene(targetSceneName);
            }
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            bool touchedInside = RectTransformUtility.RectangleContainsScreenPoint(
                comparisonButtonRect,
                touchPosition,
                null
            );

            if (touchedInside)
            {
                if (logClicks)
                    Debug.Log("Direct comparison touch detected. Loading: " + targetSceneName);

                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}