using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavKeypad 
{
    public class KeypadInteractionFPV : MonoBehaviour
    {
        private Camera cam; // Birinci şahıs kamerası

        // Başlangıçta ana kamerayı bul
        private void Awake() => cam = Camera.main;

        // Her frame'de çalışan güncelleme metodu
        private void Update()
        {
            // Fare pozisyonundan bir ışın (ray) oluştur
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            // Sol tıklama yapıldığında
            if (Input.GetMouseButtonDown(0))
            {
                // Işın bir nesneye çarparsa
                if (Physics.Raycast(ray, out var hit))
                {
                    // Çarptığımız nesnede KeypadButton komponenti var mı?
                    if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
                    {
                        // Varsa butona basma fonksiyonunu tetikle
                        keypadButton.PressButton();
                    }
                }
            }
        }
    }
}
