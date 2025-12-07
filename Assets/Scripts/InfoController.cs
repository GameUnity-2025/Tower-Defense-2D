using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InfoController : MonoBehaviour
{
    // CÁC BIẾN UI CHUNG
    [Header("General UI Panels")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject towerListPanel;
    [SerializeField] private GameObject enemyListPanel;
    [SerializeField] private GameObject towerDetailPanel;
    [SerializeField] private Image towerDetailImage;
    [SerializeField] private TMPro.TextMeshProUGUI towerDetailText;

    // --- TOWER REFERENCES ---
    [Header("Tower References (List)")]
    [Tooltip("Kéo tất cả các nút Tower vào đây theo thứ tự (Tower 1, 2, 3...).")]
    [SerializeField] private List<Button> towerButtons = new List<Button>();

    [Tooltip("Kéo Sprite của từng Tower (UNLOCKED) vào đây theo đúng thứ tự.")]
    [SerializeField] private List<Sprite> towerSprites = new List<Sprite>();

    // --- ENEMY REFERENCES ---
    [Header("Enemy References (List)")]
    [Tooltip("Kéo tất cả các nút Enemy vào đây theo thứ tự (Enemy 1, 2, 3...).")]
    [SerializeField] private List<Button> enemyButtons = new List<Button>();

    [Tooltip("Kéo Sprite của từng Enemy vào đây theo đúng thứ tự.")]
    [SerializeField] private List<Sprite> enemySprites = new List<Sprite>();

    // --- TAB BUTTONS ---
    [Header("Tab References")]
    [SerializeField] private Button enemyTabButton;
    [SerializeField] private Button towerTabButton;

    private bool showingTowers = true;

    // ====================================================================
    // CẤU TRÚC DỮ LIỆU
    // ====================================================================

    [System.Serializable]
    public struct TowerData
    {
        public string towerName;
        [TextArea(3, 10)]
        public string details;
        [Tooltip("Bật (True) để luôn khóa Tower này, bất kể cấp độ người chơi.")]
        public bool isLocked;
        [Tooltip("Sprite hiển thị trên nút khi Tower bị khóa.")]
        public Sprite lockedSprite;
        public int requiredLevel;
        public string unlockMessage;
    }

    [System.Serializable]
    public struct EnemyData
    {
        public string enemyName;
        [TextArea(3, 10)]
        public string details;
    }

    [Header("Tower Data")]
    [Tooltip("Nhập thông tin chi tiết cho từng Tower.")]
    [SerializeField] private List<TowerData> towerDetails = new List<TowerData>();

    [Header("Enemy Data")]
    [Tooltip("Nhập thông tin chi tiết cho từng Enemy. Số lượng phải khớp với Enemy Buttons.")]
    [SerializeField] private List<EnemyData> enemyDetails = new List<EnemyData>();

    // ====================================================================
    // START
    // ====================================================================

    void Start()
    {
        InitializeUI();

        // Gán sự kiện cho các nút ENEMY
        for (int i = 0; i < enemyButtons.Count; i++)
        {
            int enemyId = i + 1;
            if (enemyButtons[i] != null)
            {
                enemyButtons[i].onClick.RemoveAllListeners();
                enemyButtons[i].onClick.AddListener(() => ShowEnemyDetails(enemyId));
            }
        }

        // Assign events to tab buttons
        enemyTabButton.onClick.RemoveAllListeners();
        enemyTabButton.onClick.AddListener(SwitchToEnemies);
        towerTabButton.onClick.RemoveAllListeners();
        towerTabButton.onClick.AddListener(SwitchToTowers);

        // Thiết lập trạng thái UI ban đầu
        towerTabButton.gameObject.SetActive(false);
        enemyListPanel.SetActive(false);
        towerDetailPanel.SetActive(false);
    }

    // ====================================================================
    // HÀM KHỞI TẠO VÀ CẬP NHẬT TRẠNG THÁI TOWER
    // ====================================================================

    private void InitializeUI()
    {
        int currentLevel = GetPlayerLevel();

        // Gán sự kiện và thiết lập trạng thái ban đầu cho các nút TOWER
        for (int i = 0; i < towerButtons.Count; i++)
        {
            int towerId = i + 1;

            if (towerButtons[i] != null && i < towerDetails.Count)
            {
                // LUÔN XÓA CÁC LISTENER CŨ trước khi gán mới
                towerButtons[i].onClick.RemoveAllListeners();

                TowerData data = towerDetails[i];

                // 1. Kiểm tra điều kiện mở khóa dựa trên cấp độ:
                bool isUnlockedByLevel = currentLevel >= data.requiredLevel;

                // 2. LOGIC ĐÃ SỬA LỖI:
                // Tower bị khóa nếu: (Nó được đánh dấu LUÔN KHÓA trong Inspector) HOẶC (người chơi chưa đạt cấp độ yêu cầu)
                bool isCurrentlyLocked = data.isLocked || !isUnlockedByLevel;

                if (isCurrentlyLocked)
                {
                    // 1. Vô hiệu hóa nút
                    towerButtons[i].interactable = false;

                    // 2. Thiết lập Sprite khóa
                    if (data.lockedSprite != null)
                    {
                        towerButtons[i].GetComponent<Image>().sprite = data.lockedSprite;
                    }
                    else if (i < towerSprites.Count)
                    {
                        towerButtons[i].GetComponent<Image>().sprite = towerSprites[i];
                    }

                    // 3. Gán sự kiện chỉ hiển thị thông tin khóa
                    towerButtons[i].onClick.AddListener(() => ShowLockedTowerDetails(towerId));
                }
                else
                {
                    // Đã mở khóa
                    towerButtons[i].interactable = true;

                    // Gán Sprite mở khóa
                    if (i < towerSprites.Count && towerSprites[i] != null)
                    {
                        towerButtons[i].GetComponent<Image>().sprite = towerSprites[i];
                    }

                    // Gán sự kiện hiển thị chi tiết Tower thông thường
                    towerButtons[i].onClick.AddListener(() => ShowTowerDetails(towerId));
                }
            }
        }
    }


    // ====================================================================
    // HÀM MỚI: HIỂN THỊ CHI TIẾT KHI TOWER BỊ KHÓA
    // ====================================================================

    /// <summary>
    /// Hiển thị thông tin và sprite của tower bị khóa.
    /// </summary>
    private void ShowLockedTowerDetails(int towerId)
    {
        if (!showingTowers) return;

        int index = towerId - 1;

        if (index >= 0 && index < towerDetails.Count)
        {
            TowerData data = towerDetails[index];
            towerDetailPanel.SetActive(true);

            // Gán Sprite Khóa
            towerDetailImage.sprite = data.lockedSprite;

            // Gán Thông tin Khóa
            towerDetailText.text = data.towerName + " (LOCKED)\nRequired Level: " + data.requiredLevel
                                     + "\n" + data.unlockMessage;
        }
        else
        {
            towerDetailText.text = "Tower " + towerId + ": Data not set up yet!";
            towerDetailImage.sprite = null;
        }
    }


    // ====================================================================
    // HÀM CƠ BẢN 
    // ====================================================================

    public void ShowInfo()
    {
        if (infoPanel == null)
        {
            Debug.LogError("InfoPanel reference is missing in InfoController!");
            return;
        }

        // Khởi tạo lại trạng thái UI mỗi khi panel được mở
        InitializeUI();

        infoPanel.SetActive(true);
        towerListPanel.SetActive(true);
        enemyListPanel.SetActive(false);
        towerDetailPanel.SetActive(false);
        showingTowers = true;
        towerTabButton.gameObject.SetActive(false);
    }

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void SwitchToEnemies()
    {
        towerListPanel.SetActive(false);
        enemyListPanel.SetActive(true);
        towerDetailPanel.SetActive(false);
        showingTowers = false;
        towerTabButton.gameObject.SetActive(true);
    }

    private void SwitchToTowers()
    {
        enemyListPanel.SetActive(false);
        towerListPanel.SetActive(true);
        towerDetailPanel.SetActive(false);
        showingTowers = true;
        towerTabButton.gameObject.SetActive(false);
    }

    private int GetPlayerLevel()
    {
        // Sử dụng key "PlayerLevel" như trong code gốc của bạn
        return PlayerPrefs.GetInt("PlayerLevel", 1);
    }

    // ====================================================================
    // HIỂN THỊ CHI TIẾT TOWER (MỞ KHÓA)
    // ====================================================================

    private void ShowTowerDetails(int towerId)
    {
        if (!showingTowers) return;

        int index = towerId - 1;

        if (index >= 0 && index < towerDetails.Count)
        {
            TowerData data = towerDetails[index];
            towerDetailPanel.SetActive(true);

            // Gán Sprite Mở Khóa
            if (index < towerSprites.Count && towerSprites[index] != null)
            {
                towerDetailImage.sprite = towerSprites[index];
            }
            else
            {
                towerDetailImage.sprite = null;
            }

            // Gán Thông tin chi tiết
            towerDetailText.text = data.towerName + "\n" + data.details;
        }
        else
        {
            towerDetailText.text = "Tower " + towerId + ": Data not set up yet in Tower Details List!";
            towerDetailImage.sprite = null;
        }
    }

    // ====================================================================
    // HIỂN THỊ CHI TIẾT ENEMY
    // ====================================================================

    private void ShowEnemyDetails(int enemyId)
    {
        if (showingTowers) return;

        int index = enemyId - 1;

        if (index >= 0 && index < enemyDetails.Count)
        {
            EnemyData data = enemyDetails[index];
            towerDetailPanel.SetActive(true);

            // Lấy Sprite
            if (index < enemySprites.Count && enemySprites[index] != null)
            {
                towerDetailImage.sprite = enemySprites[index];
            }
            else
            {
                towerDetailImage.sprite = null;
            }

            // Gán Thông tin chi tiết từ EnemyData
            towerDetailText.text = data.enemyName + "\n" + data.details;
        }
        else
        {
            towerDetailText.text = "Enemy " + enemyId + ": Data not set up yet in Enemy Details List!";
            towerDetailImage.sprite = null;
        }
    }
}