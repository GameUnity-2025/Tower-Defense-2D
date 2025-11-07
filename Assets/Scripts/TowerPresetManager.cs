// TowerPresetManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TowerPresetManager : MonoBehaviour
{
    public static TowerPresetManager Instance { get; private set; }

    private const string PRESET_SAVE_KEY = "CustomTowerPresetNames";
    // Tên key trong PlayerPrefs dùng để lưu level cao nhất đã hoàn thành
    private const string MAX_LEVEL_KEY = "MaxCompletedLevel";

    // --- CẤU TRÚC DỮ LIỆU ĐỂ LƯU THÔNG TIN KHÓA ---
    // Chúng ta không cần Serializable class nếu lấy dữ liệu từ Prefab
    public struct TowerUnlockData
    {
        public Tower towerPrefab;
        public int requiredLevel;
    }
    // Danh sách sẽ được điền trong Awake() bằng cách quét Prefab
    private List<TowerUnlockData> _allTowerUnlockData = new List<TowerUnlockData>();
    // ---------------------------------------------

    [Header("Danh sách Tất cả Prefab Tháp")]
    [Tooltip("Kéo tất cả Prefab Tháp vào đây. Level yêu cầu được lấy từ script TowerUnlockRequirement.")]
    [SerializeField] private Tower[] _allTowerPrefabs;

    [Header("Số lượng Tháp có thể chọn trong Level")]
    [Tooltip("Ví dụ: 3")]
    [SerializeField] private int _maxPresetSlots = 3;

    internal Tower[] _currentPresetTowers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _currentPresetTowers = new Tower[_maxPresetSlots];

            // Bước 1: Quét Prefab để thiết lập _allTowerUnlockData
            ScanTowerRequirements();

            // Bước 2: Tải Preset đã lưu
            LoadPreset();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // HÀM MỚI: Quét tất cả Prefab tháp để lấy Level yêu cầu
    private void ScanTowerRequirements()
    {
        _allTowerUnlockData.Clear();
        foreach (Tower tower in _allTowerPrefabs)
        {
            if (tower == null) continue;

            int requiredLevel = 0;

            // Tìm component TowerUnlockRequirement trên Prefab
            TowerUnlockRequirement req = tower.GetComponent<TowerUnlockRequirement>();
            if (req != null)
            {
                requiredLevel = req.RequiredLevel;
            }
            // Nếu không có script, RequiredLevel mặc định là 0 (luôn mở)

            _allTowerUnlockData.Add(new TowerUnlockData
            {
                towerPrefab = tower,
                requiredLevel = requiredLevel
            });
        }
    }

    // --- LOGIC LƯU VÀ TẢI PRESET (ĐÃ CẬP NHẬT KIỂM TRA MỞ KHÓA) ---

    private void LoadPreset()
    {
        string savedData = PlayerPrefs.GetString(PRESET_SAVE_KEY, "");
        int maxCompletedLevel = PlayerPrefs.GetInt(MAX_LEVEL_KEY, 0); // Lấy level hoàn thành

        if (!string.IsNullOrEmpty(savedData))
        {
            string[] towerNames = savedData.Split(',');

            for (int i = 0; i < _maxPresetSlots; i++)
            {
                if (i < towerNames.Length && !string.IsNullOrEmpty(towerNames[i]))
                {
                    Tower loadedTower = _allTowerPrefabs.FirstOrDefault(t => t != null && t.name.Equals(towerNames[i]));

                    if (loadedTower != null)
                    {
                        // KIỂM TRA: Nếu tháp đã lưu bị khóa (do Level hoàn thành chưa đủ)
                        (bool isUnlocked, int requiredLevel) status = GetUnlockStatus(loadedTower, maxCompletedLevel);
                        if (status.isUnlocked)
                        {
                            _currentPresetTowers[i] = loadedTower;
                        }
                        else
                        {
                            // Nếu tháp bị khóa, thay thế bằng tháp mặc định đầu tiên đã mở khóa
                            _currentPresetTowers[i] = GetFirstUnlockedTower(maxCompletedLevel);
                        }
                    }
                }
                else
                {
                    _currentPresetTowers[i] = null;
                }
            }
        }

        // Điền các slot trống (nếu có) bằng tháp đã mở khóa.
        InitializeDefaultPreset(maxCompletedLevel);
    }

    private void SavePreset()
    {
        string[] towerNames = _currentPresetTowers
            .Select(t => t != null ? t.name : "")
            .ToArray();

        string dataToSave = string.Join(",", towerNames);
        PlayerPrefs.SetString(PRESET_SAVE_KEY, dataToSave);
        PlayerPrefs.Save();
        Debug.Log("Tower preset saved.");
    }

    // --- LOGIC MỞ KHÓA VÀ KHỞI TẠO ---

    // Lấy Prefab tháp đầu tiên đã mở khóa
    private Tower GetFirstUnlockedTower(int maxCompletedLevel)
    {
        return _allTowerUnlockData
            .Where(data => data.requiredLevel <= maxCompletedLevel)
            .Select(data => data.towerPrefab)
            .FirstOrDefault();
    }

    private void InitializeDefaultPreset(int maxCompletedLevel)
    {
        HashSet<Tower> currentTowers = new HashSet<Tower>(_currentPresetTowers.Where(t => t != null));

        // Chỉ lấy những tháp ĐÃ MỞ KHÓA và chưa có trong Preset
        Tower[] availableUnlockedTowers = _allTowerUnlockData
            .Where(data => data.requiredLevel <= maxCompletedLevel && !currentTowers.Contains(data.towerPrefab))
            .Select(data => data.towerPrefab)
            .ToArray();

        int availableIndex = 0;

        for (int i = 0; i < _maxPresetSlots; i++)
        {
            if (_currentPresetTowers[i] == null)
            {
                if (availableIndex < availableUnlockedTowers.Length)
                {
                    _currentPresetTowers[i] = availableUnlockedTowers[availableIndex];
                    availableIndex++;
                }
            }
        }
    }

    // --- LOGIC GET/SET TOWER ---

    public Tower[] GetCurrentTowerPreset()
    {
        return _currentPresetTowers.Where(t => t != null).ToArray();
    }

    // Hàm public SetTowerInPreset sẽ kiểm tra trạng thái khóa
    public bool SetTowerInPreset(int slotIndex, Tower newTower)
    {
        if (slotIndex >= 0 && slotIndex < _currentPresetTowers.Length)
        {
            (bool isUnlocked, int requiredLevel) status = GetUnlockStatus(newTower);
            if (!status.isUnlocked)
            {
                Debug.LogWarning($"Tower {newTower.name} is locked (Lv {status.requiredLevel} required). Cannot set preset.");
                return false;
            }

            _currentPresetTowers[slotIndex] = newTower;
            SavePreset();

            // Yêu cầu Panel làm mới (Nếu TowerSelectionPanel.cs có hàm này)
            // TowerSelectionPanel.Instance?.RefreshAllSlots(); 
            return true;
        }
        return false;
    }

    // Hàm MỚI: Trả về trạng thái mở khóa của một tháp
    public (bool isUnlocked, int requiredLevel) GetUnlockStatus(Tower towerPrefab)
    {
        // Lấy Level hoàn thành cao nhất hiện tại của người chơi
        int maxCompletedLevel = PlayerPrefs.GetInt(MAX_LEVEL_KEY, 0);
        return GetUnlockStatus(towerPrefab, maxCompletedLevel);
    }

    // Hàm phụ trợ dùng nội bộ
    private (bool isUnlocked, int requiredLevel) GetUnlockStatus(Tower towerPrefab, int maxCompletedLevel)
    {
        TowerUnlockData data = _allTowerUnlockData.FirstOrDefault(d => d.towerPrefab == towerPrefab);

        // Nếu không tìm thấy data, mặc định là mở khóa
        if (data.towerPrefab == null) return (true, 0);

        // Trả về: (Đã mở khóa?, Level yêu cầu)
        return (maxCompletedLevel >= data.requiredLevel, data.requiredLevel);
    }

    // Hàm MỚI: Trả về toàn bộ danh sách tháp và yêu cầu để hiển thị trong UI
    public List<TowerUnlockData> GetAllTowerUnlockData() => _allTowerUnlockData;

    // Chỉ còn hàm này (GetAllTowerPrefabs) để loại bỏ
    // public Tower[] GetAllTowerPrefabs() => _allTowerPrefabs; 

    // Đảm bảo lưu dữ liệu khi ứng dụng đóng
    private void OnApplicationQuit()
    {
        SavePreset();
    }
}