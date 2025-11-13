using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nếu bạn dùng TextMeshPro cho số level
using UnityEngine.SceneManagement;

public class LevelButtonDisplay : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Chỉ số level (ví dụ: Level 1 có index là 1).")]
    public int levelIndex;

    [Header("UI References")]
    [Tooltip("Nút bấm chính để chuyển scene.")]
    [SerializeField] private Button _levelButton;
    [Tooltip("Mảng 3 Game Object/Image của ngôi sao (Thứ tự: 1, 2, 3).")]
    [SerializeField] private GameObject[] _starIcons = new GameObject[3];
    [Tooltip("Sprite cho sao đầy (Full Star).")]
    [SerializeField] private Sprite _fullStarSprite;
    [Tooltip("Sprite cho sao rỗng (Empty Star) - Tùy chọn.")]
    [SerializeField] private Sprite _emptyStarSprite;

    private const string StarKeyPrefix = "LevelStars_";

    public void Initialize(int unlockedLevel)
    {
        // 1. Gán Listener cho nút (để có thể dùng chung hàm LoadLevel từ Controller)
        _levelButton.onClick.AddListener(() => FindObjectOfType<LevelSelectController>()?.LoadLevel(levelIndex));

        // 2. Thiết lập trạng thái khóa/mở khóa
        bool isUnlocked = (levelIndex <= unlockedLevel);
        _levelButton.interactable = isUnlocked;

        // 3. Hiển thị sao
        if (isUnlocked)
        {
            LoadAndDisplayStars();
        }
        else
        {
            // Level chưa mở khóa, không hiện sao
            HideAllStars();
        }
    }

    private void LoadAndDisplayStars()
    {
        // Lấy số sao đã lưu cho level này (0 nếu chưa hoàn thành hoặc chưa lưu)
        int starsEarned = PlayerPrefs.GetInt(StarKeyPrefix + levelIndex, 0);

        // Điều kiện của bạn: Level chưa thắng (starsEarned = 0) thì không hiện sao
        if (starsEarned == 0)
        {
            HideAllStars();
            return;
        }

        // Nếu đã thắng (1, 2, hoặc 3 sao)
        for (int i = 0; i < _starIcons.Length; i++)
        {
            GameObject starObject = _starIcons[i];

            if (starObject != null)
            {
                // i là chỉ số mảng (0, 1, 2), tương ứng với Sao 1, Sao 2, Sao 3.
                // Nếu i < starsEarned, ngôi sao đó được BẬT/HIỂN THỊ.
                bool isFullStar = (i < starsEarned);

                starObject.SetActive(true); // Luôn bật nếu đã thắng

                // Tùy chọn: Đổi sprite để thể hiện sao đầy hay sao rỗng
                Image starImage = starObject.GetComponent<Image>();
                if (starImage != null)
                {
                    if (isFullStar && _fullStarSprite != null)
                    {
                        starImage.sprite = _fullStarSprite;
                    }
                    else if (!isFullStar && _emptyStarSprite != null)
                    {
                        starImage.sprite = _emptyStarSprite;
                    }
                }
            }
        }
    }

    private void HideAllStars()
    {
        foreach (GameObject star in _starIcons)
        {
            if (star != null) star.SetActive(false);
        }
    }
}