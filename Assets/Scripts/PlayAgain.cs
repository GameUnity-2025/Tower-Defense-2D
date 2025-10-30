using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    // CHƠI LẠI LEVEL HIỆN TẠI
    public void PlayAgainFuntion()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // CHUYỂN LEVEL TIẾP THEO (TỰ ĐỘNG)
    public void NextLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        int nextLevel = currentLevel + 1;
        string sceneName = "Level" + nextLevel;

        if (SceneExistsInBuild(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene("Main"); // Về menu nếu hết level
        }
    }

    // QUAY VỀ MENU CHÍNH
    public void BackToMain()
    {
        SceneManager.LoadScene("Main");
    }

    // KIỂM TRA CẢNH CÓ TRONG BUILD SETTINGS
    private bool SceneExistsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}