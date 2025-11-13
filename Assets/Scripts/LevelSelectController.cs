using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Loại bỏ các khai báo Button cũ: level1Button, level2Button, ...
// public Button level1Button; ...

public class LevelSelectController : MonoBehaviour
{
    [Header("Level Buttons")]
    [Tooltip("Kéo tất cả các Game Object chứa script LevelButtonDisplay vào đây.")]
    // Sử dụng mảng để quản lý tất cả các nút
    public LevelButtonDisplay[] levelDisplays;

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("LastLevel", 1);
        Debug.Log($"[LevelSelect] Unlocked up to Level {unlockedLevel}");

        // Lặp qua tất cả các đối tượng hiển thị level và khởi tạo chúng
        foreach (LevelButtonDisplay display in levelDisplays)
        {
            if (display != null)
            {
                display.Initialize(unlockedLevel);
            }
        }

        // Không cần gán Listener và set Interactable ở đây nữa, 
        // vì logic đó đã được chuyển vào LevelButtonDisplay.Initialize()
    }

    // Giữ nguyên hàm LoadLevel để các LevelButtonDisplay gọi
    public void LoadLevel(int level)
    {
        string sceneName = "Level" + level;

        if (SceneExistsInBuild(sceneName))
        {
            Time.timeScale = 1f; // Đảm bảo thời gian chạy bình thường
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[LevelSelect] Scene '{sceneName}' NOT in Build Settings!");
        }
    }

    // Giữ nguyên hàm kiểm tra Scene
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