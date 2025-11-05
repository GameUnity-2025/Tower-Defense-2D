using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Cần thiết nếu chưa có

public class TowerInfoPanel : MonoBehaviour
{
    public static TowerInfoPanel Instance { get; private set; }

    [SerializeField] private TMP_Text _towerName;
    [SerializeField] private TMP_Text _damageText;
    [SerializeField] private TMP_Text _rangeText;
    [SerializeField] private TMP_Text _fireRateText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _sellButton;

    private Tower _currentTower;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowPanel(Tower tower, Vector3 worldPosition)
    {
        _currentTower = tower;
        UpdateInfo(tower);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        // GIỮ NGUYÊN VỊ TRÍ CŨ

        gameObject.SetActive(true);
    }

    private void UpdateInfo(Tower tower)
    {
        string displayName = tower.name.Replace("(Clone)", "").Trim();
        _towerName.text = $"{displayName} (Lv.{tower.CurrentLevel})";

        // Sử dụng GetType().Name.Contains thay vì 'is FireTower' để tránh lỗi nếu tên class khác
        if (tower.GetType().Name.Contains("FireTower"))
        {
            // ** CẬP NHẬT THÔNG TIN CHO FIRE TOWER (DOT) **
            FireTower fireTower = tower as FireTower;
            if (fireTower != null)
            {
                // GỌI CÁC HÀM GET MỚI ĐỂ LẤY CHỈ SỐ TỪ PREFAB FIRE BULLET
                float dps = fireTower.GetCurrentBurnDPS();
                float duration = fireTower.GetCurrentBurnDuration();

                _damageText.text = $"Burn DPS: {dps:F1}"; // Hiển thị DPS
                _rangeText.text = $"Burn Duration: {duration:F1}s"; // Hiển thị Duration
                _fireRateText.text = $"Rate: {1f / fireTower.GetShootDelay():F2}/s";
            }
        }
        else
        {
            // Tháp thông thường
            _damageText.text = $"Damage: {tower.GetShootPower()}";
            _rangeText.text = $"Range: {tower.GetShootDistance():F1}";
            _fireRateText.text = $"Rate: {1f / tower.GetShootDelay():F2}/s";
        }
        // **------------------------------------------**

        // === NÚT NÂNG CẤP ===
        if (tower.CurrentLevel < 3)
        {
            int cost = tower.GetUpgradeCost();
            _upgradeButton.interactable = tower.CanUpgrade();
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade ({cost})";
            _upgradeButton.onClick.RemoveAllListeners();
            _upgradeButton.onClick.AddListener(() =>
            {
                tower.Upgrade();
                UpdateInfo(tower); // CẬP NHẬT LẠI UI sau khi nâng cấp
            });
        }
        else
        {
            _upgradeButton.interactable = false;
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = "MAX";
        }

        // === NÚT BÁN ===
        _sellButton.onClick.RemoveAllListeners();
        _sellButton.onClick.AddListener(() =>
        {
            // Giả định LevelManager tồn tại và có RegisterSpawnedTowerRemoval
            if (LevelManager.Instance != null)
            {
                int refund = tower.EnergyCost / 2;
                LevelManager.Instance.AddEnergy(refund);
                LevelManager.Instance.RegisterSpawnedTowerRemoval(tower); // Đảm bảo gọi hàm này
            }
            Destroy(tower.gameObject);
            HidePanel();
        });
    }
    public void HidePanel()
    {
        gameObject.SetActive(false);
        _currentTower = null;
    }

    private void UpdateUpgradeButton()
    {
        if (_currentTower == null || _currentTower.CurrentLevel >= 3) return;

        int cost = _currentTower.GetUpgradeCost();
        bool canUpgrade = _currentTower.CanUpgrade();
        _upgradeButton.interactable = canUpgrade;

        // Cập nhật text để hiển thị chi phí và trạng thái
        _upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade ({cost})";
    }

    private void Update()
    {
        // 1. Click ngoài → ẩn panel
        if (Input.GetMouseButtonDown(0) && gameObject.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)transform, Input.mousePosition, Camera.main))
            {
                HidePanel();
                return;
            }
        }

        // 2. TỰ CẬP NHẬT NÚT UPGRADE MỖI FRAME
        if (_currentTower != null && gameObject.activeSelf)
        {
            UpdateUpgradeButton(); // GỌI MỖI FRAME để kiểm tra đủ tiền hay không
        }
    }
}