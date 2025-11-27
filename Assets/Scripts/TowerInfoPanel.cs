using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Collections;

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

        // TODO: Thêm logic định vị Panel UI dựa trên worldPosition

        gameObject.SetActive(true);
    }

    private void UpdateInfo(Tower tower)
    {
        string displayName = tower.name.Replace("(Clone)", "").Trim();
        _towerName.text = $"{displayName} (Lv.{tower.CurrentLevel})";

        // === LOGIC CẬP NHẬT THÔNG TIN VÀ FIX LỖI FIRE TOWER/ICE TOWER ===

        FireTower fireTower = tower as FireTower;
        // KIỂM TRA ICE TOWER
        IceWaveTower iceTower = tower as IceWaveTower;

        if (fireTower != null)
        {
            // FIX: Đảm bảo các hàm chỉ được gọi khi đối tượng FireTower hợp lệ
            try
            {
                float dps = fireTower.GetCurrentBurnDPS();
                float duration = fireTower.GetCurrentBurnDuration();

                _damageText.text = $"Burn DPS: {dps:F1}";
                _rangeText.text = $"Burn Duration: {duration:F1}s";
                _fireRateText.text = $"Rate: {1f / fireTower.GetShootDelay():F2}/s";
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Lỗi khi truy cập FireTower data: {ex.Message}", tower);
                _damageText.text = $"Damage: N/A";
                _rangeText.text = $"Range: N/A";
                _fireRateText.text = $"Rate: {1f / tower.GetShootDelay():F2}/s (Fallback)";
            }
        }
        // LOGIC HIỂN THỊ THÔNG TIN CHO ICE TOWER
        else if (iceTower != null)
        {
            float damage = iceTower.GetShootPower(); // Lấy Damage đã được ghi đè
            float slowAmount = iceTower.GetSlowAmount(); // Lấy Slow Amount mới

            _damageText.text = $"Damage: {damage:F0}"; // Sát thương là số nguyên
            _rangeText.text = $"Slow: {(slowAmount * 100):F0}%"; // Hiển thị dưới dạng %
            _fireRateText.text = $"Rate: {1f / iceTower.GetShootDelay():F2}/s";
        }
        else // Tháp thường
        {
            _damageText.text = $"Damage: {tower.GetShootPower()}";
            _rangeText.text = $"Range: {tower.GetShootDistance():F1}";
            _fireRateText.text = $"Rate: {1f / tower.GetShootDelay():F2}/s";
        }

        // === UPGRADE BUTTON ===
        if (tower.CurrentLevel < 3)
        {
            int cost = tower.GetUpgradeCost();
            _upgradeButton.interactable = tower.CanUpgrade();
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade ({cost})";
            _upgradeButton.onClick.RemoveAllListeners();
            _upgradeButton.onClick.AddListener(() =>
            {
                tower.Upgrade();
                UpdateInfo(tower);
            });
        }
        else
        {
            _upgradeButton.interactable = false;
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = "MAX";
        }

        // === SELL BUTTON ===
        _sellButton.onClick.RemoveAllListeners();
        _sellButton.onClick.AddListener(() =>
        {
            if (LevelManager.Instance != null)
            {
                int refund = tower.EnergyCost / 2;
                LevelManager.Instance.AddEnergy(refund);
                LevelManager.Instance.RegisterSpawnedTowerRemoval(tower);
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

        _upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade ({cost})";
    }

    private void Update()
    {
        if (_currentTower != null && gameObject.activeSelf)
        {
            UpdateUpgradeButton();
        }
    }
}