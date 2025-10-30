using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            Debug.Log("[TowerInfoPanel] Initialized!");
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
       ; // GIỮ NGUYÊN VỊ TRÍ CŨ

        gameObject.SetActive(true);
    }

    private void UpdateInfo(Tower tower)
    {
        string displayName = tower.name.Replace("(Clone)", "").Trim();
        _towerName.text = $"{displayName} (Lv.{tower.CurrentLevel})"; // HIỆN LEVEL
        _damageText.text = $"Damage: {tower.GetShootPower()}";
        _rangeText.text = $"Range: {tower.GetShootDistance():F1}";
        _fireRateText.text = $"Rate: {1f / tower.GetShootDelay():F2}/s";

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
                UpdateInfo(tower); // CẬP NHẬT LẠI UI
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
            int refund = tower.EnergyCost / 2;
            LevelManager.Instance.AddEnergy(refund);
            LevelManager.Instance.RegisterSpawnedTowerRemoval(tower);
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
        if (_currentTower == null) return;

        if (_currentTower.CurrentLevel < 3)
        {
            int cost = _currentTower.GetUpgradeCost();
            bool canUpgrade = _currentTower.CanUpgrade();
            _upgradeButton.interactable = canUpgrade;
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = canUpgrade ? $"Upgrade ({cost})" : $"Upgrade ({cost})";
            // Optional: đổi màu nếu muốn
        }
        else
        {
            _upgradeButton.interactable = false;
            _upgradeButton.GetComponentInChildren<TMP_Text>().text = "MAX";
        }
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
            UpdateUpgradeButton(); // GỌI MỖI FRAME
        }
    }
}