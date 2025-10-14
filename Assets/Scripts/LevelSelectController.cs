using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    public Button level1Button;
    public Button level2Button;

    void Start()
    {
        int lastLevel = PlayerPrefs.GetInt("LastLevel", 1);
        level1Button.onClick.AddListener(() => LoadLevel(1));
        level2Button.onClick.AddListener(() => LoadLevel(2));
        level2Button.interactable = lastLevel >= 2; 
    }

    void LoadLevel(int level)
    {
        Debug.Log("Loading Level: Level" + level);
        SceneManager.LoadScene("Level" + level);
        PlayerPrefs.SetInt("LastLevel", level); 
    }
}
