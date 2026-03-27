using UnityEngine;

public class SimpleUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject choice1;
    public GameObject choice2;
    public GameObject choice3;

    public PhoneSystem phoneSystem;

    // ตัวแปรเก็บสถานะว่ากด Choice ไปแล้วหรือยัง
    private bool choice1Clicked = false;
    private bool choice2Clicked = false;
    private bool choice3Clicked = false;

    void Start()
    {
        mainPanel.SetActive(true);
        choice1.SetActive(false);
        choice2.SetActive(false);
        choice3.SetActive(false);
    }

    public void ShowChoice1()
    {
        if (choice1Clicked) return; // ถ้ากดไปแล้ว ไม่ทำอะไร
        choice1Clicked = true;

        mainPanel.SetActive(false);
        choice1.SetActive(true);
        phoneSystem.PauseRingtone();
    }

    public void ShowChoice2()
    {
        if (choice2Clicked) return;
        choice2Clicked = true;

        mainPanel.SetActive(false);
        choice2.SetActive(true);
        phoneSystem.PauseRingtone();
    }

    public void ShowChoice3()
    {
        if (choice3Clicked) return;
        choice3Clicked = true;

        mainPanel.SetActive(false);
        choice3.SetActive(true);
        phoneSystem.PauseRingtone();
    }

    public void Back()
    {
        mainPanel.SetActive(true);
        choice1.SetActive(false);
        choice2.SetActive(false);
        choice3.SetActive(false);
        phoneSystem.ResumeRingtone();
    }

    // ฟังก์ชัน Reset ถ้าต้องการให้กด Choice ใหม่
    public void ResetChoices()
    {
        choice1Clicked = false;
        choice2Clicked = false;
        choice3Clicked = false;
    }
}