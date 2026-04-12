using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("QUIT GAME");

        // 👉 ถ้าอยู่ใน Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 👉 ถ้าเป็น Build เกมจริง
        Application.Quit();
#endif
    }
}