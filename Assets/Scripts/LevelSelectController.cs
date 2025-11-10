using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    // --- KHAI BÁO CŨ ---
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;
    public Button level6Button;

    // --- KHAI BÁO MỚI (Level 7 - 10) ---
    public Button level7Button;
    public Button level8Button;
    public Button level9Button;
    public Button level10Button;

    void Start()
    {
        // Load the highest unlocked level saved by PlayerPrefs
        int unlockedLevel = PlayerPrefs.GetInt("LastLevel", 1);
        Debug.Log($"[LevelSelect] Unlocked up to Level {unlockedLevel}");

        // Assign listeners to buttons
        level1Button.onClick.AddListener(() => LoadLevel(1));
        level2Button.onClick.AddListener(() => LoadLevel(2));
        level3Button.onClick.AddListener(() => LoadLevel(3));
        level4Button.onClick.AddListener(() => LoadLevel(4));
        level5Button.onClick.AddListener(() => LoadLevel(5));
        level6Button.onClick.AddListener(() => LoadLevel(6));
        level7Button.onClick.AddListener(() => LoadLevel(7));
        level8Button.onClick.AddListener(() => LoadLevel(8));
        level9Button.onClick.AddListener(() => LoadLevel(9));
        level10Button.onClick.AddListener(() => LoadLevel(10));
        // -------------------------

        // Set button interactivity based on unlocked level
        level1Button.interactable = true;
        level2Button.interactable = unlockedLevel >= 2;
        level3Button.interactable = unlockedLevel >= 3;
        level4Button.interactable = unlockedLevel >= 4;
        level5Button.interactable = unlockedLevel >= 5;
        level6Button.interactable = unlockedLevel >= 6;
        level7Button.interactable = unlockedLevel >= 7;
        level8Button.interactable = unlockedLevel >= 8;
        level9Button.interactable = unlockedLevel >= 9;
        level10Button.interactable = unlockedLevel >= 10;
        // ----------------------------------------
    }

    void LoadLevel(int level)
    {
        string sceneName = "Level" + level;

        // Check if the scene is in Build Settings before loading
        if (SceneExistsInBuild(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[LevelSelect] Scene '{sceneName}' NOT in Build Settings!");
        }
    }

    // Check if the scene exists in Build Settings
    bool SceneExistsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    public void OnBackButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
}