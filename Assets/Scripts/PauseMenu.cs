using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    // reference to the pause menu canvas
    public GameObject pauseMenuCanvas;
    public GameObject settingsMenuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject confirmQuitCanvas;
    public GameObject confirmMainMenuCanvas;

    // paused or nah
    private bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        if (pauseMenuCanvas != null && settingsMenuCanvas != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        } else
        {
            Debug.Log("pauseMenu and settingsMenu canvas not set");
        }
      
        
    }

    public void Pause()
    {
        // set canvas to active
        pauseMenuCanvas.SetActive(true);
        // disable time to stop game
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        // set pause menu to active
        pauseMenuCanvas.SetActive(false);
        confirmMainMenuCanvas.SetActive(false);
        confirmQuitCanvas.SetActive(false);
        // start time to continue game
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    // SETTINGS - IN GAME
    public void OpenSettingsInGame()
    {
        pauseMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(true);
    }

    public void CloseSettingsInGame()
    {
        settingsMenuCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);
    }

    // SETTINGS - MAIN MENU
    public void OpenSettingsMainMenu()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
            settingsMenuCanvas.SetActive(true);
        }
    }

    public void CloseSettingsMainMenu()
    {
        if (mainMenuCanvas != null)
        {
            settingsMenuCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
        }
    }

    // QUIT CONFIRMATION
    public void OpenMainMenuConfirmation()
    {
        confirmMainMenuCanvas.SetActive(true);
    }

    public void OpenCloseGameConfirmation()
    {
        confirmQuitCanvas.SetActive(true);
    }


    // LOAD CHARACTER SELECT SCREEN
    public void LoadCharacterSelectScreen()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    // LOAD THE MAIN MENU
    public void LoadMainMenu()
    {
        CharacterSelector characterSelector = FindObjectOfType<CharacterSelector>();
        if (characterSelector != null) 
        {
            Destroy(characterSelector.gameObject);
        }

        // reset time scale in case paused
        Time.timeScale = 1f;

        // load main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    // CHECK IF GAME IS PAUSED
    public bool IsPaused()
    {
        return isPaused;
    }

    // CLOSE GAME
    public void Exit()
    {
        {
            Debug.Log("Exiting Game....");
            Application.Quit();
        }
    }
}
