using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageUploader : MonoBehaviour
{
    [Tooltip("Bu butona basýldýðýnda resmin yükleneceði beyaz kutu (RawImage)")]
    public RawImage targetRawImage;

    // Butona týklandýðýnda çalýþacak fonksiyon
    public void OpenFileBrowser()
    {
#if UNITY_EDITOR
        // Sadece Unity Editor içinde Windows dosya seçiciyi açar
        string path = EditorUtility.OpenFilePanel("Fotoðraf Seç", "", "png,jpg,jpeg");
        
        if (!string.IsNullOrEmpty(path))
        {
            LoadImageFromPath(path);
        }
#else
        Debug.LogWarning("Bu dosya seçici sadece Unity Editör'de çalýþýr. Build sonrasý için eklenti gerekir.");
#endif
    }

    private void LoadImageFromPath(string path)
    {
        // Dosyadaki resmi byte olarak oku
        byte[] fileData = File.ReadAllBytes(path);

        // Yeni bir doku (Texture) oluþtur ve resmi içine yükle
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); // Boyutlarý otomatik olarak resme göre ayarlar

        // Beyaz kutuya (RawImage) bu dokuyu ata
        if (targetRawImage != null)
        {
            targetRawImage.texture = tex;
        }
    }
}