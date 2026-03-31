using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName; // ใส่ชื่อ Scene ที่จะไป

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}