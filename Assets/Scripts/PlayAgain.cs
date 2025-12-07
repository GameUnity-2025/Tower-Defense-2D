using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // Thêm thư viện này cho Path

public class PlayAgain : MonoBehaviour
{
    // REPLAY CURRENT LEVEL
    public void PlayAgainFuntion()
    {
        // Vẫn hoạt động tốt
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // LOAD NEXT LEVEL (Sử dụng Build Index)
    public void NextLevel()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currentLevelIndex + 1;

        // Tổng số scene đã thêm vào Build Settings (bao gồm cả Menu)
        int totalScenesInBuild = SceneManager.sceneCountInBuildSettings;

        if (nextLevelIndex < totalScenesInBuild)
        {
            // Tải Scene tiếp theo bằng Build Index
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            // Quay về Menu nếu không còn level nào
            SceneManager.LoadScene("Main");
        }
    }

    // BACK TO MAIN MENU
    public void BackToMain()
    {
        SceneManager.LoadScene("Main");
    }

 
}