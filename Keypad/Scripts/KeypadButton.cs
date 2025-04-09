using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavKeypad
{
    public class KeypadButton : MonoBehaviour
    {
        // -------------------- BUTON AYARLARI --------------------
        [Header("Değer")]
        [SerializeField] private string value; // Butonun üzerindeki sayı/değer (Örnek: "1", "2", "A", "B")
        
        [Header("Animasyon Ayarları")]
        [SerializeField] private float bttnspeed = 0.1f;       // Butonun inip çıkma hızı
        [SerializeField] private float moveDist = 0.0025f;     // Butonun ne kadar içeri gireceği
        [SerializeField] private float buttonPressedTime = 0.1f; // Butonun basılı kalma süresi
        
        [Header("Referanslar")]
        [SerializeField] private Keypad keypad; // Ana keypad sistemine bağlantı

        private bool moving; // Buton şu anda hareket ediyor mu? (Çift tıklamayı önlemek için)

        // Butona basıldığında çalışır (Unity Event ile tetiklenir)
        public void PressButton()
        {
            if (!moving) // Eğer buton zaten hareket etmiyorsa
            {
                keypad.AddInput(value); // Ana keypad'e bu butonun değerini gönder
                StartCoroutine(MoveSmooth()); // Buton animasyonunu başlat
            }
        }

        // Butonun basılıp geri dönme animasyonunu yöneten coroutine
        private IEnumerator MoveSmooth()
        {
            moving = true; // Buton hareket etmeye başladı
            
            // ---------- BUTONUN İÇERİ GİRME KISMI ----------
            Vector3 startPos = transform.localPosition; // Butonun başlangıç pozisyonu
            Vector3 endPos = transform.localPosition + new Vector3(0, 0, moveDist); // İçeri gideceği pozisyon
            
            float elapsedTime = 0;
            
            // Yumuşak hareket için Lerp kullanımı (Saniyede 60 kere güncelleme)
            while (elapsedTime < bttnspeed)
            {
                elapsedTime += Time.deltaTime; // Gerçek zamanlı süre takibi
                float t = Mathf.Clamp01(elapsedTime / bttnspeed); // 0-1 arası ilerleme değeri
                
                transform.localPosition = Vector3.Lerp(startPos, endPos, t); // Pozisyonu güncelle
                
                yield return null; // Bir sonraki frame'e kadar bekle
            }
            
            transform.localPosition = endPos; // Son pozisyona tam olarak yerleş
            
            // ---------- BUTONUN BASILI KALMA SÜRESİ ----------
            yield return new WaitForSeconds(buttonPressedTime); // Buton basılı kalsın
            
            // ---------- BUTONUN GERİ DÖNME KISMI ----------
            startPos = transform.localPosition; 
            endPos = transform.localPosition - new Vector3(0, 0, moveDist); // Başlangıç pozisyonuna dön
            
            elapsedTime = 0;
            while (elapsedTime < bttnspeed)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / bttnspeed);
                
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                
                yield return null;
            }
            
            transform.localPosition = endPos; // Başlangıç pozisyonuna tam dönüş
            
            moving = false; // Hareket tamamlandı
        }
    }
}
