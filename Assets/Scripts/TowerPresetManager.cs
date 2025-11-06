// TowerPresetManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Không cần thiết nhưng thường dùng cho Serialization

public class TowerPresetManager : MonoBehaviour
{
    public static TowerPresetManager Instance { get; private set; }

    // TÊN KEY DÙNG ĐỂ LƯU PRESET TRONG PlayerPrefs
    private const string PRESET_SAVE_KEY = "CustomTowerPresetNames";

    [Header("Danh sách Tất cả Prefab Tháp")]
    [Tooltip("Kéo tất cả Prefab Tháp vào đây.")]
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

            // THAY ĐỔI: Tải Preset nếu có, nếu không thì dùng mặc định
            LoadPreset();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- LOGIC LƯU VÀ TẢI PRESET ---

    private void LoadPreset()
    {
        string savedData = PlayerPrefs.GetString(PRESET_SAVE_KEY, "");

        if (!string.IsNullOrEmpty(savedData))
        {
            // Tải Preset từ chuỗi tên đã lưu
            string[] towerNames = savedData.Split(',');

            for (int i = 0; i < _maxPresetSlots; i++)
            {
                if (i < towerNames.Length && !string.IsNullOrEmpty(towerNames[i]))
                {
                    // Tìm Prefab tháp dựa trên tên đã lưu trong mảng _allTowerPrefabs
                    Tower loadedTower = _allTowerPrefabs.FirstOrDefault(t => t != null && t.name.Equals(towerNames[i]));

                    if (loadedTower != null)
                    {
                        _currentPresetTowers[i] = loadedTower;
                    }
                    else
                    {
                        // Nếu tháp không tìm thấy (đã xóa hoặc đổi tên), để trống slot
                        _currentPresetTowers[i] = null;
                    }
                }
                else
                {
                    // Slot trống (do chuỗi tên ngắn hơn _maxPresetSlots)
                    _currentPresetTowers[i] = null;
                }
            }
        }

        // Nếu không có dữ liệu lưu trữ (savedData rỗng) HOẶC sau khi tải vẫn còn slot trống, 
        // thì gọi hàm mặc định để điền vào
        InitializeDefaultPreset();
    }

    private void SavePreset()
    {
        // Chuyển danh sách Prefab thành danh sách tên (name)
        string[] towerNames = _currentPresetTowers
            .Select(t => t != null ? t.name : "")
            .ToArray();

        // Nối các tên lại thành một chuỗi, cách nhau bằng dấu phẩy
        string dataToSave = string.Join(",", towerNames);
        PlayerPrefs.SetString(PRESET_SAVE_KEY, dataToSave);
        PlayerPrefs.Save();
        Debug.Log("Tower preset saved.");
    }

    // --- LOGIC KHỞI TẠO MẶC ĐỊNH (Sửa đổi để chỉ điền vào slot trống) ---

    private void InitializeDefaultPreset()
    {
        // Lấy danh sách tháp chưa được sử dụng trong _currentPresetTowers
        HashSet<Tower> currentTowers = new HashSet<Tower>(_currentPresetTowers.Where(t => t != null));

        // Chỉ lấy những tháp trong _allTowerPrefabs mà chưa có trong Preset
        Tower[] availableTowers = _allTowerPrefabs
            .Where(t => t != null && !currentTowers.Contains(t))
            .ToArray();

        int availableIndex = 0;

        for (int i = 0; i < _maxPresetSlots; i++)
        {
            // Chỉ điền vào slot trống (NULL)
            if (_currentPresetTowers[i] == null)
            {
                if (availableIndex < availableTowers.Length)
                {
                    _currentPresetTowers[i] = availableTowers[availableIndex];
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

    public bool SetTowerInPreset(int slotIndex, Tower newTower)
    {
        if (slotIndex >= 0 && slotIndex < _currentPresetTowers.Length)
        {
            _currentPresetTowers[slotIndex] = newTower;

            // LƯU PRESET MỖI KHI NGƯỜI CHƠI THAY ĐỔI LỰA CHỌN
            SavePreset();

            // Yêu cầu Panel làm mới (Nếu TowerSelectionPanel.cs có hàm này)
            // TowerSelectionPanel.Instance?.RefreshAllSlots(); 
            return true;
        }
        return false;
    }

    public Tower[] GetAllTowerPrefabs() => _allTowerPrefabs;

    // Đảm bảo lưu dữ liệu khi ứng dụng đóng
    private void OnApplicationQuit()
    {
        SavePreset();
    }
}