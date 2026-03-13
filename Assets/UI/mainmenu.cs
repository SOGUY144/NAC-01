using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;

    public void StartGame()
    {
        mainMenu.SetActive(false);
    }
}