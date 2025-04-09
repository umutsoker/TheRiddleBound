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
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;
        [Header("Combination Code (9 Numbers Max)")]
        [SerializeField] private int keypadCombo = 12345;

        //
        [Header("Attempt Settings")]
        [SerializeField] private int maxAttempts = 3; // Maksimum deneme hakk�
        [SerializeField] private TMP_Text attemptsText; // Kalan haklar� g�sterecek UI Text
        private int remainingAttempts;
        //

        public UnityEvent OnAccessGranted => onAccessGranted;
        public UnityEvent OnAccessDenied => onAccessDenied;

        [Header("Settings")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        [Header("Visuals")]
        [SerializeField] private float displayResultTime = 1f;
        [Range(0, 5)]
        [SerializeField] private float screenIntensity = 2.5f;
        [Header("Colors")]
        [SerializeField] private Color screenNormalColor = new Color(0.98f, 0.50f, 0.032f, 1f); //orangy
        [SerializeField] private Color screenDeniedColor = new Color(1f, 0f, 0f, 1f); //red
        [SerializeField] private Color screenGrantedColor = new Color(0f, 0.62f, 0.07f); //greenish
        [Header("SoundFx")]
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;
        [Header("Component References")]
        [SerializeField] private Renderer panelMesh;
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private AudioSource audioSource;

        [Header("Game Over Settings")]
        [SerializeField] private TMP_Text gameOverText;
        [SerializeField] private float gameOverDelay = 2f; // 2 saniye sonra sahne de�i�sin
        [SerializeField] private string gameOverSceneName = "GameOverScene";


        private string currentInput;
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        private void Awake()
        {
            ClearInput();
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);
            //
            remainingAttempts = maxAttempts; // Ba�lang��ta t�m haklar aktif
            UpdateAttemptsText(); // UI'� g�ncelle
            //
        }


        //Gets value from pressedbutton
        public void AddInput(string input)
        {
            audioSource.PlayOneShot(buttonClickedSfx);
            if (displayingResult || accessWasGranted) return;
            switch (input)
            {
                case "enter":
                    CheckCombo();
                    break;
                default:
                    if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                    {
                        return;
                    }
                    currentInput += input;
                    keypadDisplayText.text = currentInput;
                    break;
            }

        }
        public void CheckCombo()
        {
            if (int.TryParse(currentInput, out var currentKombo))
            {
                bool granted = currentKombo == keypadCombo;
                if (!displayingResult)
                {
                    StartCoroutine(DisplayResultRoutine(granted));
                }
            }
            else
            {
                Debug.LogWarning("Couldn't process input for some reason..");
            }

        }

        //mainly for animations 
        private IEnumerator DisplayResultRoutine(bool granted)
        {
            displayingResult = true;

            if (granted) AccessGranted();
            else AccessDenied();

            yield return new WaitForSeconds(displayResultTime);
            displayingResult = false;
            if (granted) yield break;
            ClearInput();
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);

        }
        //
        private IEnumerator LockKeypadRoutine()
        {
            keypadDisplayText.text = "K�L�TLEND�";
            yield return new WaitForSeconds(2f);
            // Burada keypad'i tamamen devre d��� b�rakabilirsiniz
            this.enabled = false; // Keypad script'ini kapat

            keypadDisplayText.text = "K�L�TLEND�";
            yield return new WaitForSeconds(1f);

            // "Ba�aramad�n" yaz�s�n� g�ster
            if (gameOverText != null)
                gameOverText.gameObject.SetActive(true);

            // Sahneyi de�i�tirmeden �nce bekle
            yield return new WaitForSeconds(gameOverDelay);
            SceneManager.LoadScene(gameOverSceneName);
        }
        //

        private void AccessDenied()
        {
            keypadDisplayText.text = accessDeniedText;
            onAccessDenied?.Invoke();
            panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity);
            audioSource.PlayOneShot(accessDeniedSfx);
            //
            remainingAttempts--;
            UpdateAttemptsText();

            if (remainingAttempts <= 0)
            {
                StartCoroutine(LockKeypadRoutine());
                return; // ClearInput() �a�r�lmas�n
            }

            ClearInput(); // Kalan hak varsa temizle

            keypadDisplayText.text = accessDeniedText;
            onAccessDenied?.Invoke();
            panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity);
            audioSource.PlayOneShot(accessDeniedSfx);
            //
        }
        //
        private void UpdateAttemptsText()
        {
            if (attemptsText != null)
                attemptsText.text = $"Kalan Hakk�n�z: {remainingAttempts}";
        }
        //

        private void ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = currentInput;
        }

        private void AccessGranted()
        {
            SceneManager.LoadScene("SampleScene"); // Ana sahneye geri d�n
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            onAccessGranted?.Invoke();
            panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity);
            audioSource.PlayOneShot(accessGrantedSfx);
        }

    }
}