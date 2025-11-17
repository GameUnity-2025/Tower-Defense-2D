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
        // Khởi tạo tốc độ game ban đầu
        Time.timeScale = speedLevels[currentSpeedIndex];
        UpdateSpeedText();

        // Load volume từ Audio Mixer
        // Giá trị Slider là tuyến tính (0-1), cần chuyển đổi Logarithmic (dB) cho Mixer
        if (musicMixer.GetFloat("Music", out float musicVolume))
            musicSlider.value = Mathf.Pow(10, musicVolume / 20);
        else
            musicSlider.value = 1f;

        if (musicMixer.GetFloat("SFX", out float sfxVolume))
            sfxSlider.value = Mathf.Pow(10, sfxVolume / 20);
        else
            sfxSlider.value = 1f;

        // Gán Listener cho các Slider và Button
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        speedToggleButton.onClick.AddListener(ToggleSpeed);

        UpdateText();
    }

    public void OpenSetting()
    {
        if (settingPanel == null) return;

        // **[ĐÃ SỬA] Đảm bảo Panel hiển thị trên cùng (SetAsLastSibling)**
        // Việc này đưa Panel về cuối danh sách con của đối tượng cha, vẽ sau cùng, nên hiển thị trên cùng.
        settingPanel.transform.SetAsLastSibling();

        settingPanel.SetActive(true);
        Time.timeScale = 0f; // Tạm dừng game khi mở cài đặt
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            // Khôi phục tốc độ game về mức đã chọn trước đó
            Time.timeScale = speedLevels[currentSpeedIndex];
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicMixer != null)
            // Chuyển đổi giá trị tuyến tính (0-1) sang dB
            musicMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        UpdateText();
    }

    public void SetSFXVolume(float volume)
    {
        if (musicMixer != null)
            // Chuyển đổi giá trị tuyến tính (0-1) sang dB
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
        // Chuyển sang tốc độ tiếp theo (dùng modulo để quay vòng)
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