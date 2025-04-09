using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeypadTrigger : MonoBehaviour
{
    public string passwordSceneName = "Sifre";

    private void OnMouseDown()
    {
        SceneManager.LoadScene(passwordSceneName); // �ifre sahnesine ge�
    }
}
