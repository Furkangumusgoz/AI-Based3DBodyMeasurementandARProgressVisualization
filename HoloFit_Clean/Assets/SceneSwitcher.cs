using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþleri için gerekli kütüphane

public class SceneSwitcher : MonoBehaviour
{
    public void GoToMeasurementScreen()
    {
        // Ölçüm sahnesinin adýný tam olarak buraya yazýyoruz
        SceneManager.LoadScene("MeasurementScene");
    }
}