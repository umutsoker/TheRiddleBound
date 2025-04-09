using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NavKeypad
{
    public class Keypad : MonoBehaviour
    {
        // -------------------- AYARLAR VE OLAYLAR --------------------
        [Header("Olaylar")]
        [SerializeField] private UnityEvent onAccessGranted; // Şifre doğruysa tetiklenecek olay
        [SerializeField] private UnityEvent onAccessDenied;   // Şifre yanlışsa tetiklenecek olay
        
        [Header("Kombinasyon Ayarları (Max 9 Haneli)")]
        [SerializeField] private int keypadCombo = 12345;     // Kapıyı açacak şifre

        // -------------------- DENEME HAKKİ AYARLARİ --------------------
        [Header("Deneme Ayarları")]
        [SerializeField] private int maxAttempts = 3;        // Maksimum yanlış girme hakkı
        [SerializeField] private TMP_Text attemptsText;      // Kalan hakları gösteren text
        private int remainingAttempts;                       // Geriye kalan deneme hakkı

        // -------------------- DİGER AYARLAR --------------------
        [Header("Görsel Ayarlar")]
        [SerializeField] private float displayResultTime = 1f; // Mesajların ekranda kalma süresi
        [Range(0, 5)]
        [SerializeField] private float screenIntensity = 2.5f;  // Ekran parlaklık ayarı

        [Header("Renkler")]
        [SerializeField] private Color screenNormalColor = new Color(0.98f, 0.50f, 0.032f, 1f);  // Normal renk (turuncu)
        [SerializeField] private Color screenDeniedColor = new Color(1f, 0f, 0f, 1f);           // Reddedilince kırmızı
        [SerializeField] private Color screenGrantedColor = new Color(0f, 0.62f, 0.07f);        // Onaylanınca yeşil

        [Header("Ses Efektleri")]
        [SerializeField] private AudioClip buttonClickedSfx;  // Tuş sesi
        [SerializeField] private AudioClip accessDeniedSfx;  // Hatalı giriş sesi
        [SerializeField] private AudioClip accessGrantedSfx; // Doğru giriş sesi

        [Header("Game Over Ayarları")]
        [SerializeField] private TMP_Text gameOverText;      // "Başaramadın" yazısı
        [SerializeField] private float gameOverDelay = 2f;   // Sahne değişmeden önce bekleme süresi
        [SerializeField] private string gameOverSceneName = "GameOverScene"; // Yüklenecek sahne adı

        // -------------------- REFERANSLAR --------------------
        [Header("Component Referansları")]
        [SerializeField] private Renderer panelMesh;         // Keypad ekranının materyali
        [SerializeField] private TMP_Text keypadDisplayText;// Şifre gösterim texti
        [SerializeField] private AudioSource audioSource;   // Ses kaynağı

        // -------------------- DEGİSKENLER --------------------
        private string currentInput;           // Kullanıcının anlık girdisi
        private bool displayingResult = false;// Sonuç gösteriliyor mu?
        private bool accessWasGranted = false; // Şifre doğru girildi mi?

        // Oyun başlangıcında ayarlamaları yap
        private void Awake()
        {
            ClearInput(); // Ekranı temizle
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity); // Ekran rengini ayarla
            
            remainingAttempts = maxAttempts; // Deneme hakkını doldur
            UpdateAttemptsText();            // UI'ı güncelle
        }

        // Tuşlara basıldığında çalışır
        public void AddInput(string input)
        {
            audioSource.PlayOneShot(buttonClickedSfx); // Tuş sesi çal
            
            // Eğer animasyon oynuyorsa veya şifre zaten doğruysa işlem yapma
            if (displayingResult || accessWasGranted) return;

            switch (input)
            {
                case "enter": // Enter tuşuna basıldığında
                    CheckCombo(); // Şifreyi kontrol et
                    break;
                default: // Sayı tuşlarına basıldığında
                    if (currentInput.Length == 9) return; // Max 9 haneli şifre
                    currentInput += input; // Girilen sayıyı ekle
                    keypadDisplayText.text = currentInput; // Ekranda göster
                    break;
            }
        }

        // Şifre kontrolünü yapan metod
        public void CheckCombo()
        {
            if (int.TryParse(currentInput, out var currentKombo)) // Girilen sayıyı integer'a çevir
            {
                bool granted = currentKombo == keypadCombo; // Şifre doğru mu?
                StartCoroutine(DisplayResultRoutine(granted)); // Sonucu göster
            }
            else
            {
                Debug.LogWarning("Hatalı giriş!"); // Sayıya çevrilemezse hata
            }
        }

        // Sonuç gösterimini yöneten coroutine
        private IEnumerator DisplayResultRoutine(bool granted)
        {
            displayingResult = true; // Animasyon başladı

            if (granted) AccessGranted(); // Doğruysa açılma işlemi
            else AccessDenied();          // Yanlışsa hata işlemi

            yield return new WaitForSeconds(displayResultTime); // Bekleme süresi
            displayingResult = false; // Animasyon bitti
            
            if (!granted) // Yanlış şifre girilirse ekran temizlenecek.
            {
                ClearInput();
                panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);
            }
        }

        // Şifre doğru girilirse çalışır
        private void AccessGranted()
        {
            SceneManager.LoadScene("SampleScene"); // Ana sahneye dön
            accessWasGranted = true;                // Giriş onaylandı
            keypadDisplayText.text = accessGrantedText; // Granted yaz
            panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity); // Yeşil yanıp sönsün
            audioSource.PlayOneShot(accessGrantedSfx); // Onay sesi çal
        }

        // Şifre yanlış girilirse çalışır
        private void AccessDenied()
        {
            remainingAttempts--; // Deneme hakkını azalt
            UpdateAttemptsText(); // UI'ı güncelle

            if (remainingAttempts <= 0) // Hak kalmadıysa
            {
                StartCoroutine(LockKeypadRoutine()); // Keypad'i kilitle
                return;
            }

            // Hata efekti ayarları
            keypadDisplayText.text = accessDeniedText; // "Denied" yaz
            panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity); // Kırmızı yanıp sönsün
            audioSource.PlayOneShot(accessDeniedSfx); // Hata sesi çal
        }

        // Keypad'i kilitleyen ve game over sahnesine geçiş yapan coroutine
        private IEnumerator LockKeypadRoutine()
        {
            keypadDisplayText.text = "KİLİTLENDİ"; 
            yield return new WaitForSeconds(2f);
            
            this.enabled = false; // Keypad'i devre dışı bırak

            if (gameOverText != null)
                gameOverText.gameObject.SetActive(true); // "Başaramadın" yazısını göster

            yield return new WaitForSeconds(gameOverDelay);
            SceneManager.LoadScene(gameOverSceneName); // Game Over sahnesine geç
        }

        // Kalan deneme hakkını UI'da göster
        private void UpdateAttemptsText()
        {
            if (attemptsText != null)
                attemptsText.text = $"Kalan Hakkınız: {remainingAttempts}";
        }

        // Ekranı ve girdiyi temizle
        private void ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = currentInput;
        }
    }
}
