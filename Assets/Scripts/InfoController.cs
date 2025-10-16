using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoController : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject towerListPanel;
    [SerializeField] private GameObject enemyListPanel;
    [SerializeField] private GameObject towerDetailPanel;
    [SerializeField] private Image towerDetailImage;
    [SerializeField] private TMPro.TextMeshProUGUI towerDetailText;
    [SerializeField] private Button tower1Button;
    [SerializeField] private Button tower2Button;
    [SerializeField] private Button tower3Button;
    [SerializeField] private Button tower4Button;
    [SerializeField] private Sprite tower1Sprite;
    [SerializeField] private Sprite tower2Sprite;
    [SerializeField] private Sprite tower3Sprite;
    [SerializeField] private Sprite tower4Sprite;
    [SerializeField] private Button enemy1Button;
    [SerializeField] private Button enemy2Button;
    [SerializeField] private Button enemy3Button;
    [SerializeField] private Sprite enemy1Sprite;
    [SerializeField] private Sprite enemy2Sprite;
    [SerializeField] private Sprite enemy3Sprite;
    [SerializeField] private Button enemyTabButton;
    [SerializeField] private Button towerTabButton; // Nút Tower

    private bool showingTowers = true;

    void Start()
    {
        // Gán sự kiện cho các nút tháp
        tower1Button.onClick.AddListener(() => ShowTowerDetails(1));
        tower2Button.onClick.AddListener(() => ShowTowerDetails(2));
        tower3Button.onClick.AddListener(() => ShowTowerDetails(3));
        tower4Button.onClick.AddListener(() => ShowTowerDetails(4));

        // Gán sự kiện cho các nút kẻ địch
        enemy1Button.onClick.AddListener(() => ShowEnemyDetails(1));
        enemy2Button.onClick.AddListener(() => ShowEnemyDetails(2));

        // Kiểm tra và log trạng thái của enemy3Button
        Debug.Log("enemy3Button status: " + (enemy3Button == null ? "NULL" : "NOT NULL"));

        if (enemy3Button != null)
        {
            enemy3Button.onClick.AddListener(() => ShowEnemyDetails(3));
            Debug.Log("Successfully added click listener to enemy3Button");
        }
        else
        {
            Debug.LogWarning("enemy3Button is not assigned in Inspector!");
        }

        // Gán sự kiện cho nút Enemy và Tower
        enemyTabButton.onClick.AddListener(SwitchToEnemies);
        towerTabButton.onClick.AddListener(SwitchToTowers);

        // Ẩn nút Tower ban đầu
        towerTabButton.gameObject.SetActive(false);
        enemyListPanel.SetActive(false);
        towerDetailPanel.SetActive(false);
    }

    public void ShowInfo()
    {
        if (infoPanel == null || towerListPanel == null || enemyListPanel == null || towerDetailPanel == null ||
            towerDetailImage == null || towerDetailText == null)
        {
            Debug.LogError("One or more references are missing in InfoController!");
            return;
        }
        infoPanel.SetActive(true);
        towerListPanel.SetActive(true); // Hiển thị tháp ban đầu
        enemyListPanel.SetActive(false);
        towerDetailPanel.SetActive(false);
        showingTowers = true;
        towerTabButton.gameObject.SetActive(false); // Ẩn nút Tower khi mở Info
    }

    public void HideInfo()
    {
        infoPanel.SetActive(false);
    }

    private void SwitchToEnemies()
    {
        towerListPanel.SetActive(false);
        enemyListPanel.SetActive(true);
        towerDetailPanel.SetActive(false);
        showingTowers = false;
        towerTabButton.gameObject.SetActive(true); // Hiển thị nút Tower khi chuyển sang Enemy

        // Đếm và hiển thị số lượng enemy
        int enemyCount = CountEnemies();
        Debug.Log("Chuyển sang tab Enemy. Số lượng enemy: " + enemyCount);
    }

    private void SwitchToTowers()
    {
        enemyListPanel.SetActive(false);
        towerListPanel.SetActive(true);
        towerDetailPanel.SetActive(false);
        showingTowers = true;
        towerTabButton.gameObject.SetActive(false); // Ẩn nút Tower khi quay lại Tower
    }

    private void ShowTowerDetails(int towerId)
    {
        if (!showingTowers) return; // Chỉ hiển thị nếu đang ở tab tháp
        towerDetailPanel.SetActive(true);
        switch (towerId)
        {
            case 1:
                towerDetailImage.sprite = tower1Sprite;
                towerDetailText.text = "Tower 1: Basic Tower\n" +
                                      "Health: 100\n" +
                                      "Speed: 1.0s\n" +
                                      "Damage: 10\n" +
                                      "Special Ability: None";
                break;

            case 2:
                towerDetailImage.sprite = tower2Sprite;
                towerDetailText.text = "Tower 2: Advanced Tower\n" +
                                      "Health: 150\n" +
                                      "Speed: 0.8s\n" +
                                      "Damage: 20\n" +
                                      "Special Ability: Splash Damage";
                break;

            case 3:
                towerDetailImage.sprite = tower3Sprite;
                towerDetailText.text = "Tower 3: Pro Tower\n" +
                                      "Health: 200\n" +
                                      "Speed: 0.6s\n" +
                                      "Damage: 30\n" +
                                      "Special Ability: Slow Enemy";
                break;

            case 4:
                if (PlayerPrefs.GetInt("UnlockedTower4", 0) == 1)
                {
                    towerDetailImage.sprite = tower4Sprite;
                    towerDetailText.text = "Tower 4: Ultimate Tower\n" +
                                          "Health: 300\n" +
                                          "Speed: 0.4s\n" +
                                          "Damage: 50\n" +
                                          "Special Ability: Area Freeze";
                }
                else
                {
                    towerDetailText.text = "Tower 4: Ultimate Tower (Locked)\nUnlock at Level 10";
                    towerDetailImage.sprite = null;
                }
                break;
        }
    }

    private void ShowEnemyDetails(int enemyId)
    {
        if (showingTowers) return; // Chỉ hiển thị nếu đang ở tab kẻ địch
        towerDetailPanel.SetActive(true);
        switch (enemyId)
        {
            case 1:
                towerDetailImage.sprite = enemy1Sprite;
                towerDetailText.text = "Enemy 1: Basic Enemy\n" +
                                      "Health: 50\n" +
                                      "Speed: 1.0s\n" +
                                      "Damage: 5\n" +
                                      "Special Ability: None";
                break;

            case 2:
                towerDetailImage.sprite = enemy2Sprite;
                towerDetailText.text = "Enemy 2: Midder Enemy\n" +
                                      "Health: 100\n" +
                                      "Speed: 0.8s\n" +
                                      "Damage: 10\n" +
                                      "Special Ability: Armor";
                break;

            case 3:
                towerDetailImage.sprite = enemy3Sprite;
                towerDetailText.text = "Enemy 3: Advanced Enemy\n" +
                                      "Health: 200\n" +
                                      "Speed: 0.1s\n" +
                                      "Damage: 16\n" +
                                      "Special Ability: Armor";
                break;
        }
    }

    // Thêm phương thức này vào class InfoController
    public int CountEnemies()
    {
        if (enemyListPanel == null) return 0;

        int count = 0;
        Button[] buttons = enemyListPanel.GetComponentsInChildren<Button>(true); // true để lấy cả các object bị ẩn

        foreach (Button button in buttons)
        {
            if (button == enemy1Button || button == enemy2Button || button == enemy3Button)
                count++;
        }

        Debug.Log("Số lượng enemy trong danh sách: " + count);
        return count;
    }
}