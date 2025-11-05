using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private TMPro.TextMeshProUGUI musicText;
    [SerializeField] private TMPro.TextMeshProUGUI sfxText;
    [SerializeField] private Button speedToggleButton;
    [SerializeField] private TMPro.TextMeshProUGUI speedText;

    private float[] speedLevels = { 1f, 1.5f, 2f };
    private int currentSpeedIndex = 0;

    void Start()
    {
        Time.timeScale = speedLevels[currentSpeedIndex];
        UpdateSpeedText();

        // Load volume
        if (musicMixer.GetFloat("Music", out float musicVolume))
            musicSlider.value = Mathf.Pow(10, musicVolume / 20);
        else
            musicSlider.value = 1f;

        if (musicMixer.GetFloat("SFX", out float sfxVolume))
            sfxSlider.value = Mathf.Pow(10, sfxVolume / 20);
        else
            sfxSlider.value = 1f;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        speedToggleButton.onClick.AddListener(ToggleSpeed);

        UpdateText();
    }

    public void OpenSetting()
    {
        if (settingPanel == null) return;
        settingPanel.SetActive(true);
        Time.timeScale = 0f; // Pause when opening settings
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            Time.timeScale = speedLevels[currentSpeedIndex]; // Restore speed
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicMixer != null)
            musicMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        UpdateText();
    }

    public void SetSFXVolume(float volume)
    {
        if (musicMixer != null)
            musicMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        UpdateText();
    }

    private void UpdateText()
    {
        if (musicText != null)
            musicText.text = "Music: " + (musicSlider.value * 100).ToString("F0") + "%";
        if (sfxText != null)
            sfxText.text = "SFX: " + (sfxSlider.value * 100).ToString("F0") + "%";
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void ToggleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        Time.timeScale = speedLevels[currentSpeedIndex];
        UpdateSpeedText();
        Debug.Log($"[SPEED] Game speed: {speedLevels[currentSpeedIndex]}x");
    }

    private void UpdateSpeedText()
    {
        if (speedText != null)
            speedText.text = "X" + speedLevels[currentSpeedIndex].ToString("F1");
    }
}