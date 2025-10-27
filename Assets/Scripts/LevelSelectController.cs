using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;

    void Start()
    {
        // LẤY MÀN CAO NHẤT ĐÃ MỞ KHÓA
        int unlockedLevel = PlayerPrefs.GetInt("LastLevel", 1);
        Debug.Log($"[LevelSelect] Unlocked up to Level {unlockedLevel}");

        // GÁN SỰ KIỆN
        level1Button.onClick.AddListener(() => LoadLevel(1));
        level2Button.onClick.AddListener(() => LoadLevel(2));
        level3Button.onClick.AddListener(() => LoadLevel(3));
        level4Button.onClick.AddListener(() => LoadLevel(4));
        level5Button.onClick.AddListener(() => LoadLevel(5));

        // MỞ KHÓA NÚT THEO UNLOCKEDLEVEL
        level1Button.interactable = true;
        level2Button.interactable = unlockedLevel >= 2;
        level3Button.interactable = unlockedLevel >= 3;
        level4Button.interactable = unlockedLevel >= 4;
        level5Button.interactable = unlockedLevel >= 5;
    }

    void LoadLevel(int level)
    {
        string sceneName = "Level" + level;
        if (System.IO.File.Exists(Application.dataPath + "/Scenes/" + sceneName + ".unity"))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene not found: " + sceneName);
        }
    }
}