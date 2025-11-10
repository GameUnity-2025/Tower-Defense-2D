using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MennuController : MonoBehaviour
{
    private const int MAX_LEVEL = 10;

    public void OnStartButton()
    {
        int lastLevel = PlayerPrefs.GetInt("LastLevel", 1);

        if (lastLevel > MAX_LEVEL)
        {
            lastLevel = MAX_LEVEL;
            Debug.LogWarning("Player has completed all levels. Loading the final level: Level" + lastLevel);
        }

        Debug.Log("Loading Level: Level" + lastLevel);
        SceneManager.LoadScene("Level" + lastLevel);
    }

    public void OnSelectLevelButton()
    {
        Debug.Log("Loading LevelSelect");
        SceneManager.LoadScene("LevelSelect");
    }
}