using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider; // Slider mới cho SFX
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private TMPro.TextMeshProUGUI musicText;
    [SerializeField] private TMPro.TextMeshProUGUI sfxText; // Text mới cho SFX

    void Start()
    {
        if (musicMixer == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("musicMixer, musicSlider, hoặc sfxSlider chưa được gán trong Inspector!");
            return;
        }

        // Khởi tạo âm lượng Music
        if (musicMixer.GetFloat("Music", out float musicVolume))
        {
            musicSlider.value = Mathf.Pow(10, musicVolume / 20);
        }
        else
        {
            Debug.LogWarning("Parameter 'Music' không tồn tại trong musicMixer! Kiểm tra Audio Mixer.");
            musicSlider.value = 1.0f;
            SetMusicVolume(1.0f);
        }

        // Khởi tạo âm lượng SFX
        if (musicMixer.GetFloat("SFX", out float sfxVolume))
        {
            sfxSlider.value = Mathf.Pow(10, sfxVolume / 20);
        }
        else
        {
            Debug.LogWarning("Parameter 'SFX' không tồn tại trong musicMixer! Kiểm tra Audio Mixer.");
            sfxSlider.value = 1.0f;
            SetSFXVolume(1.0f);
        }

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume); // Gắn sự kiện cho SFX slider
        UpdateText();
    }

    public void OpenSetting()
    {
        if (settingPanel == null)
        {
            Debug.LogError("settingPanel chưa được gán trong Inspector!");
            return;
        }
        Debug.Log("Mở SettingPanel, trạng thái trước: " + settingPanel.activeSelf);
        settingPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Trạng thái sau: " + settingPanel.activeSelf);
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
        if (musicMixer != null)
        {
            Debug.Log("Đang đặt âm lượng cho 'Music': " + Mathf.Log10(volume) * 20);
            musicMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
            UpdateText();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (musicMixer != null)
        {
            Debug.Log("Đang đặt âm lượng cho 'SFX': " + Mathf.Log10(volume) * 20);
            musicMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (musicText != null && musicSlider != null)
        {
            musicText.text = "Music Volume: " + (musicSlider.value * 100).ToString("F0") + "%";
        }
        if (sfxText != null && sfxSlider != null)
        {
            sfxText.text = "SFX Volume: " + (sfxSlider.value * 100).ToString("F0") + "%";
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
}