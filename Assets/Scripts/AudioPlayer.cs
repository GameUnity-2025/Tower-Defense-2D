using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer _instance = null;

    [SerializeField] private AudioSource _audioSource; // AudioSource cho SFX
    [SerializeField] private List<AudioClip> _audioClips; // Danh sách AudioClip cho SFX
    [SerializeField] private AudioSource _musicAudioSource; // AudioSource dành cho Music
    [SerializeField] private List<AudioClip> _musicClips; // Danh sách AudioClip cho Music
    [SerializeField] private Slider _musicSlider; // Slider để điều chỉnh âm lượng Music

    void Start()
    {
        if (_musicAudioSource != null && _musicSlider != null)
        {
            // Khởi tạo âm lượng từ slider (giá trị mặc định 1.0 nếu không có)
            _musicAudioSource.volume = _musicSlider.value;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume); // Gắn sự kiện khi kéo slider
            PlayMusic(); // Phát nhạc mặc định khi khởi động
        }
    }

    void Update()
    {
        // Không cần thêm logic ở đây trừ khi bạn muốn kiểm soát khác
    }

    public static AudioPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioPlayer>();
            }
            return _instance;
        }
    }

    public void PlaySFX(string name)
    {
        AudioClip sfx = _audioClips.Find(s => s.name == name);
        if (sfx == null)
        {
            return;
        }
        _audioSource.PlayOneShot(sfx);
    }

    public void PlayMusic()
    {
        if (_musicAudioSource != null && _musicClips != null && _musicClips.Count > 0)
        {
            _musicAudioSource.clip = _musicClips[0]; // Chơi clip nhạc đầu tiên, có thể mở rộng để chọn ngẫu nhiên
            _musicAudioSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (_musicAudioSource != null)
        {
            _musicAudioSource.volume = volume; // Đặt âm lượng trực tiếp từ slider (0-1)
            Debug.Log("Đã đặt âm lượng Music: " + volume);
        }
    }
}