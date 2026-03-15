using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    void Start()
    {
        // เริ่มเกมให้เมนูหลักเปิด และ Settings ปิด
        if (mainMenu != null)
            mainMenu.SetActive(true);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        Debug.Log("Start Game Clicked");

        if (mainMenu != null)
            mainMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        Debug.Log("Settings Clicked");

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void BackToMenu()
    {
        Debug.Log("Back To Menu");

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (mainMenu != null)
            mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}