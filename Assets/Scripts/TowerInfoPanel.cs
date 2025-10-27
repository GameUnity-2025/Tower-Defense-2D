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
            Debug.Log("[TowerInfoPanel] Singleton CREATED!");

            // Ẩn panel SAU KHI tạo xong Instance
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
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

        // Đặt vị trí panel gần tower (trên màn hình)
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
     

        gameObject.SetActive(true);
    }

    private void UpdateInfo(Tower tower)
    {
        // LOẠI BỎ (Clone) VÀ KHOẢNG TRẮNG
        string displayName = tower.name.Replace("(Clone)", "").Trim();

        _towerName.text = displayName;
        _damageText.text = $"Damage: {tower.GetShootPower()}";
        _rangeText.text = $"Range: {tower.GetShootDistance():F1}";
        _fireRateText.text = $"Rate: {1f / tower.GetShootDelay():F2}/s";

        _upgradeButton.onClick.RemoveAllListeners();
        _upgradeButton.onClick.AddListener(() => {
            Debug.Log("Upgrade clicked!");
            HidePanel();
        });

        _sellButton.onClick.RemoveAllListeners();
        _sellButton.onClick.AddListener(() => {
            int refund = tower.EnergyCost / 2;
            LevelManager.Instance.AddEnergy(refund);
            LevelManager.Instance.RegisterSpawnedTowerRemoval(tower);
            Destroy(tower.gameObject);
            HidePanel();
        });
    }

    private void UpgradeTower(Tower tower)
    {
        // TODO: Logic nâng cấp
        Debug.Log($"Upgrading {tower.name}");
        HidePanel();
    }

    private void SellTower(Tower tower)
    {
        int refund = tower.EnergyCost / 2;
        LevelManager.Instance.AddEnergy(refund);
        LevelManager.Instance.RegisterSpawnedTowerRemoval(tower); // Cần thêm hàm này
        Destroy(tower.gameObject);
        HidePanel();
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        _currentTower = null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && gameObject.activeSelf)
        {
            // Click ngoài panel → đóng
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)transform, Input.mousePosition, Camera.main))
            {
                HidePanel();
            }
        }
    }
}