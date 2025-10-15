using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel";
    private const string LastLevelKey = "LastLevel";

    [SerializeField] private Button _levelButtonPrefab;
    [SerializeField] private RectTransform _buttonContainer;
    [SerializeField] private int _totalLevels = 20;
    [SerializeField] private int _buttonsPerRow = 5;
    [SerializeField] private Vector2 _firstButtonPosition = new Vector2(-390f, 165f);
    [SerializeField] private Vector2 _buttonSpacing = new Vector2(110f, -60f);
    [SerializeField] private Color _lockedTextColor = Color.gray;
    [SerializeField] private Color _unlockedTextColor = Color.white;

    private readonly List<Button> _levelButtons = new List<Button>();

    private void Start()
    {
        int highestUnlockedLevel = Mathf.Clamp(PlayerPrefs.GetInt(HighestUnlockedKey, 1), 1, _totalLevels);
        int lastLevel = Mathf.Clamp(PlayerPrefs.GetInt(LastLevelKey, 1), 1, _totalLevels);

        GenerateLevelButtons(highestUnlockedLevel);

        // Ensure last played level is highlighted as unlocked (in case PlayerPrefs was cleared)
        if (lastLevel > highestUnlockedLevel)
        {
            UnlockLevel(lastLevel);
        }
    }

    private void GenerateLevelButtons(int highestUnlockedLevel)
    {
        if (_levelButtonPrefab == null || _buttonContainer == null)
        {
            Debug.LogError("LevelSelectController is missing prefab or container references.");
            return;
        }

        _levelButtonPrefab.gameObject.SetActive(false);

        for (int level = 1; level <= _totalLevels; level++)
        {
            Button button = Instantiate(_levelButtonPrefab, _buttonContainer);
            button.gameObject.name = $"Level{level}Button";
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = $"Level {level}";
            }

            bool isUnlocked = level <= highestUnlockedLevel;
            button.interactable = isUnlocked;
            ApplyLockedStyling(button, isUnlocked);

            int capturedLevel = level;
            button.onClick.AddListener(() => LoadLevel(capturedLevel));

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                int row = (level - 1) / _buttonsPerRow;
                int column = (level - 1) % _buttonsPerRow;
                Vector2 anchoredPosition = _firstButtonPosition + new Vector2(_buttonSpacing.x * column, _buttonSpacing.y * row);
                buttonRect.anchoredPosition = anchoredPosition;
            }

            _levelButtons.Add(button);
        }
    }

    private void ApplyLockedStyling(Button button, bool isUnlocked)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.color = isUnlocked ? _unlockedTextColor : _lockedTextColor;
        }
    }

    private void UnlockLevel(int level)
    {
        if (level < 1 || level > _levelButtons.Count)
        {
            return;
        }

        Button button = _levelButtons[level - 1];
        button.interactable = true;
        ApplyLockedStyling(button, true);
    }

    private void LoadLevel(int level)
    {
        string sceneName = $"Level{level}";
        Debug.Log($"Loading Level: {sceneName}");

        PlayerPrefs.SetInt(LastLevelKey, level);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }
}
