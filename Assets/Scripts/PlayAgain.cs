using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    // REPLAY CURRENT LEVEL
    public void PlayAgainFuntion()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // LOAD NEXT LEVEL (AUTOMATICALLY)
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
            SceneManager.LoadScene("Main"); // Back to menu if all levels are finished
        }
    }

    // BACK TO MAIN MENU
    public void BackToMain()
    {
        SceneManager.LoadScene("Main");
    }

    // CHECK IF SCENE EXISTS IN BUILD SETTINGS
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