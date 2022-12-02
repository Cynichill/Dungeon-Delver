 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject disableObject;
    [SerializeField] private GameObject enableObject;

    public void OnMenuSwap()
    {
        enableObject.SetActive(true);
        disableObject.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
