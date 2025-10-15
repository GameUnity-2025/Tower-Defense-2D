using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    private const string LevelPrefix = "Level";
    private const string LevelSelectScene = "LevelSelect";
    private const int MaxLevel = 20;

    public void playAgain ()
    {
        Scene currentScene = SceneManager.GetActiveScene ();
        SceneManager.LoadScene (currentScene.name);
    }

    public void nextScene ()
    {
        if (!TryGetCurrentLevelIndex (out int currentLevel))
        {
            SceneManager.LoadScene (LevelSelectScene);
            return;
        }

        int nextLevel = currentLevel + 1;
        if (nextLevel > MaxLevel)
        {
            SceneManager.LoadScene (LevelSelectScene);
            return;
        }

        LoadLevel (nextLevel);
    }

    public void level3Scene ()
    {
        SceneManager.LoadScene (LevelSelectScene);
    }

    public void prevScene ()
    {
        if (!TryGetCurrentLevelIndex (out int currentLevel))
        {
            SceneManager.LoadScene (LevelSelectScene);
            return;
        }

        int previousLevel = currentLevel - 1;
        if (previousLevel < 1)
        {
            SceneManager.LoadScene (LevelSelectScene);
            return;
        }

        LoadLevel (previousLevel);
    }

    private static void LoadLevel (int levelIndex)
    {
        string sceneName = $"{LevelPrefix}{levelIndex}";
        SceneManager.LoadScene (sceneName);
    }

    private static bool TryGetCurrentLevelIndex (out int levelIndex)
    {
        string sceneName = SceneManager.GetActiveScene ().name;
        return TryParseLevelIndex (sceneName, out levelIndex);
    }

    private static bool TryParseLevelIndex (string sceneName, out int levelIndex)
    {
        levelIndex = 0;
        if (!sceneName.StartsWith (LevelPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string indexPart = sceneName.Substring (LevelPrefix.Length);
        return int.TryParse (indexPart, out levelIndex);
    }
    public void BackToMain()
    {
        SceneManager.LoadScene("Main");
    }    
}
