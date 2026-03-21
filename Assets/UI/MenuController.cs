using UnityEngine;

public class MenuController : MonoBehaviour
{
    public RectTransform selector;
    public RectTransform[] options;

    public GameObject mainMenuPanel;
    public GameObject settingMenu;   // ⭐ เพิ่มตัวนี้

    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    int index = 0;

    void Start()
    {
        UpdateMenu();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll < 0f)
        {
            index++;
            if (index >= options.Length)
                index = options.Length - 1;

            UpdateMenu();
        }

        if (scroll > 0f)
        {
            index--;
            if (index < 0)
                index = 0;

            UpdateMenu();
        }
    }

    public void ClickOption(int i)
    {
        index = i;
        UpdateMenu();

        if (i == 0)
            OpenPanel(graphicsPanel);

        else if (i == 1)
            OpenPanel(audioPanel);

        else if (i == 2)
            OpenPanel(controlsPanel);

        else if (i == 3)
            OpenPanel(creditsPanel);

        else if (i == 4)
            BackToMenu();
    }

    void UpdateMenu()
    {
        selector.position = options[index].position;

        for (int i = 0; i < options.Length; i++)
        {
            if (i == index)
                options[i].localScale = Vector3.one * 1.2f;
            else
                options[i].localScale = Vector3.one;
        }
    }

    void OpenPanel(GameObject panel)
    {
        mainMenuPanel.SetActive(false);
        settingMenu.SetActive(true);   // ⭐ เปิด SettingMenu
        panel.SetActive(true);
    }

    public void BackToMenu()
    {
        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        settingMenu.SetActive(false);  // ⭐ ปิด SettingMenu
        mainMenuPanel.SetActive(true);
    }
}