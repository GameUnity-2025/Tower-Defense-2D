using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer _instance = null;

    [SerializeField] private AudioSource _audioSource; 
    [SerializeField] private List<AudioClip> _audioClips; 
    [SerializeField] private AudioSource _musicAudioSource; 
    [SerializeField] private List<AudioClip> _musicClips; 
    [SerializeField] private Slider _musicSlider; 

    void Start()
    {
        if (_musicAudioSource != null && _musicSlider != null)
        {       
            _musicAudioSource.volume = _musicSlider.value;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume); 
            PlayMusic();
        }
    }

    void Update()
    {
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
            _musicAudioSource.clip = _musicClips[0]; 
            _musicAudioSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (_musicAudioSource != null)
        {
            _musicAudioSource.volume = volume; 
            Debug.Log("Đã đặt âm lượng Music: " + volume);
        }
    }
}