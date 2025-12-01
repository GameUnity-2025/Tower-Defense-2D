using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingController : MonoBehaviour
{
    // ... [Các trường SerializeField giữ nguyên]
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

    // --- KHAI BÁO KEY ĐỂ LƯU PLAYERPREFS ---
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const float DefaultVolume = 0.75f; // Mặc định 75%

    void Start()
    {
        // Khởi tạo tốc độ game
        Time.timeScale = speedLevels[currentSpeedIndex];
        UpdateSpeedText();

        // --- 1. TẢI VOLUME TỪ PLAYERPREFS VÀ THIẾT LẬP SLIDER VÀ MIXER ---

        // Tải Music Volume
        if (musicSlider != null && musicMixer != null)
        {
            // Tải giá trị đã lưu (từ 0 đến 1), nếu chưa có thì dùng giá trị mặc định 0.75
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);

            musicSlider.value = savedMusicVolume;

            // ÁP DỤNG LUÔN CHO MIXER (vì Mixer cần giá trị dB)
            musicMixer.SetFloat("Music", Mathf.Log10(savedMusicVolume) * 20);
        }

        // Tải SFX Volume
        if (sfxSlider != null && musicMixer != null)
        {
            // Tải giá trị đã lưu (từ 0 đến 1), nếu chưa có thì dùng giá trị mặc định 0.75
            float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);

            sfxSlider.value = savedSfxVolume;

            // ÁP DỤNG LUÔN CHO MIXER
            musicMixer.SetFloat("SFX", Mathf.Log10(savedSfxVolume) * 20);
        }

        // --- 2. GÁN LISTENER (Chỉ gán nếu component có mặt) ---
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (speedToggleButton != null)
            speedToggleButton.onClick.AddListener(ToggleSpeed);

        // --- 3. CẬP NHẬT TEXT LẦN ĐẦU ---
        UpdateText();
    }

    // ... [Hàm OpenSetting, CloseSetting giữ nguyên] ...

    public void OpenSetting()
    {
        if (settingPanel == null) return;
        settingPanel.transform.SetAsLastSibling();

        settingPanel.SetActive(true);
        // Tạm dừng game (Chỉ thực hiện nếu đây là Scene Gameplay)
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0f;
        }
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            // Tiếp tục game (Chỉ thực hiện nếu đây là Scene Gameplay)
            if (Time.timeScale == 0)
            {
                Time.timeScale = speedLevels[currentSpeedIndex];
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        // CẬP NHẬT MIXER
        if (musicMixer != null)
            musicMixer.SetFloat("Music", Mathf.Log10(volume) * 20);

        // --- LƯU TRỮ (SAVE) GIÁ TRỊ VĨNH VIỄN ---
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();

        // GỌI CẬP NHẬT TEXT
        UpdateText();
    }

    public void SetSFXVolume(float volume)
    {
        // CẬP NHẬT MIXER
        if (musicMixer != null)
            musicMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);

        // --- LƯU TRỮ (SAVE) GIÁ TRỊ VĨNH VIỄN ---
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        PlayerPrefs.Save();

        // GỌI CẬP NHẬT TEXT
        UpdateText();
    }

    private void UpdateText()
    {
        // Chuyển đổi giá trị Slider (0..1) sang phần trăm (0..100) để hiển thị

        // KIỂM TRA musicSlider và musicText TRƯỚC KHI CẬP NHẬT
        if (musicText != null && musicSlider != null)
            musicText.text = "Music: " + (musicSlider.value * 100).ToString("F0") + "%";

        // KIỂM TRA sfxSlider và sfxText TRƯỚC KHI CẬP NHẬT
        if (sfxText != null && sfxSlider != null)
            sfxText.text = "SFX: " + (sfxSlider.value * 100).ToString("F0") + "%";
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        // Giả sử Scene Menu tên là "Main"
        SceneManager.LoadScene("Main");
    }

    public void ToggleSpeed()
    {
        if (speedText == null) return; // Nếu nút tốc độ không được gắn (ví dụ: ở Menu), thì dừng

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