using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems; // Cần thiết để kiểm tra UI

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

        // Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition); // Bỏ comment
        // // Position logic ( giữ nguyên ) // Bỏ comment/Xóa

        // Debug 4: Xác nhận Panel nhận được lệnh hiển thị
        Debug.Log($"[PANEL SHOW] Attempting to show Panel for: {tower.name}");

        // Ép Panel hiển thị ngay tại điểm Neo (Middle Center)
        gameObject.SetActive(true);

        // Debug 5: Kiểm tra trạng thái cuối cùng
        Debug.Log($"[PANEL SHOW] Panel GameObject Active: {gameObject.activeSelf}");
    }

    private void UpdateInfo(Tower tower)
    {
        string displayName = tower.name.Replace("(Clone)", "").Trim();
        _towerName.text = $"{displayName} (Lv.{tower.CurrentLevel})";

        if (tower.GetType().Name.Contains("FireTower"))
        {
            // Update info for Fire Tower (DOT)
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
            // Normal Tower
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
        // 1. Click/Tap outside -> hide panel
        if (gameObject.activeSelf && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            Vector2 screenPoint = Input.mousePosition;

            // Use touch position if available
            if (Input.touchCount > 0)
            {
                screenPoint = Input.GetTouch(0).position;
            }

            // Check if the click/tap occurred outside the panel's RectTransform
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)transform, screenPoint, Camera.main))
            {
                // ** IMPORTANT MOBILE CHECK: Is the tap over any other UI element? **
                // This prevents hiding the panel when tapping on other UI buttons (e.g., TowerUI buttons)
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // If it's not over any UI, check if it's over the current tower itself
                    if (_currentTower != null && _currentTower.gameObject.GetComponent<Collider2D>() != null)
                    {
                        // Check if the tap is NOT on the tower's collider
                        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
                        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);

                        if (hitCollider != null && hitCollider.gameObject == _currentTower.gameObject)
                        {
                            // We clicked the tower itself, do not hide (it will re-open the panel)
                            return;
                        }
                    }

                    // If it passed all checks (outside the panel AND not on the tower/other UI)
                    HidePanel();
                }
            }
        }

        // 2. AUTO UPDATE UPGRADE BUTTON 
        if (_currentTower != null && gameObject.activeSelf)
        {
            UpdateUpgradeButton();
        }
    }
}