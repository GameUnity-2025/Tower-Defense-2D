// TowerInfoPanel.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Collections; // Vẫn giữ để đề phòng có các logic Coroutine khác

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

        // Position logic (nếu có)

        gameObject.SetActive(true);
    }

    private void UpdateInfo(Tower tower)
    {
        string displayName = tower.name.Replace("(Clone)", "").Trim();
        _towerName.text = $"{displayName} (Lv.{tower.CurrentLevel})";

        // Logic cập nhật thông tin (FireTower vs Normal Tower)
        if (tower.GetType().Name.Contains("FireTower"))
        {
            FireTower fireTower = tower as FireTower;
            if (fireTower != null)
            {
                float dps = fireTower.GetCurrentBurnDPS();
                float duration = fireTower.GetCurrentBurnDuration();

                _damageText.text = $"Burn DPS: {dps:F1}";
                _rangeText.text = $"Burn Duration: {duration:F1}s";
                _fireRateText.text = $"Rate: {1f / fireTower.GetShootDelay():F2}/s";
            }
        }
        else
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
        // LOGIC TỰ ĐỘNG ĐÓNG PANEL KHI CLICK BÊN NGOÀI ĐÃ BỊ LOẠI BỎ.
        // PANEL CHỈ ĐÓNG KHI NHẤN NÚT CLOSE HOẶC SELL.

        // Tự động cập nhật nút Upgrade
        if (_currentTower != null && gameObject.activeSelf)
        {
            UpdateUpgradeButton();
        }
    }
}