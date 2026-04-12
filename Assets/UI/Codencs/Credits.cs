using UnityEngine;

public class CreditsPanelController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject creditsPanel;

    void Start()
    {
        mainPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        mainPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }
}