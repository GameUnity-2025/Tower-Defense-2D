using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMPro.TextMeshProUGUI musicText;

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            UpdateText();
        }
    }

    public void OpenSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void SetMusicVolume(float volume)
    {
        AudioPlayer.Instance.SetMusicVolume(volume); // Gửi giá trị đến AudioPlayer
        UpdateText();
    }

    private void UpdateText()
    {
        if (musicText != null && musicSlider != null)
        {
            musicText.text = "Music Volume: " + (musicSlider.value * 100).ToString("F0") + "%";
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
}