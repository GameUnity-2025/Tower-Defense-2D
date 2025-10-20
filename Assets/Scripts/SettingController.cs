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
    [SerializeField] private Button speedToggleButton; // Nút duy nhất để thay đổi tốc độ
    [SerializeField] private TMPro.TextMeshProUGUI speedText; // Text hiển thị tốc độ

    private float[] speedLevels = { 1f, 1.5f, 2f }; // Các mức tốc độ
    private int currentSpeedIndex = 0; // Chỉ số tốc độ hiện tại, mặc định là 0 (X1)

    void Start()
    {
        if (musicMixer == null || musicSlider == null || sfxSlider == null || speedToggleButton == null)
        {
            Debug.LogError("musicMixer, musicSlider, sfxSlider, hoặc speedToggleButton chưa được gán trong Inspector!");
            return;
        }

        Time.timeScale = speedLevels[currentSpeedIndex]; // Đặt tốc độ mặc định là X1 khi khởi động
        UpdateSpeedText(); // Cập nhật text tốc độ ban đầu

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
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        UpdateText();
        speedToggleButton.onClick.AddListener(ToggleSpeed); // Gắn sự kiện cho nút
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
        Time.timeScale = 0f; // Pause game khi mở setting
        Debug.Log("Trạng thái sau: " + settingPanel.activeSelf);
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            Time.timeScale = speedLevels[currentSpeedIndex]; // Quay lại tốc độ hiện tại khi đóng
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

    public void ToggleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length; // Vòng lặp: 0 → 1 → 2 → 0
        Time.timeScale = speedLevels[currentSpeedIndex];
        UpdateSpeedText();
        Debug.Log("Tốc độ game đặt thành: " + speedLevels[currentSpeedIndex] + "x");
    }

    private void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = "X" + speedLevels[currentSpeedIndex].ToString("F1"); // Hiển thị X1, X1.5, X2
        }
    }
}