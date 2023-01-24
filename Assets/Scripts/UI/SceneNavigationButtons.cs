using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationButtons : MonoBehaviour
{
    public void BackToMenu()
    {
        Menus menus = GameObject.FindWithTag("Menus").GetComponent<Menus>();
        menus.PlayButtonClick(); //sfx
        menus.UnpauseGame();
        menus.LoadScene("Menu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Menus menus = GameObject.FindWithTag("Menus").GetComponent<Menus>();
        menus.PlayButtonClick(); //sfx
        menus.UnpauseGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        Menus menus = GameObject.FindWithTag("Menus").GetComponent<Menus>();
        menus.PlayButtonClick(); //sfx
        menus.UnpauseGame();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == 5)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}
