// TowerSelectionPanel.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TowerSelectionPanel : MonoBehaviour
{
    public static TowerSelectionPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _allTowerListContainer; 
    [SerializeField] private GameObject _towerDragItemPrefab; 

    [Header("Preset Slots")]
    [SerializeField] private PresetSlot[] _presetSlots; 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeTowerList();
        gameObject.SetActive(false); 
    }

    private void InitializeTowerList()
    {
        foreach (Transform child in _allTowerListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<TowerPresetManager.TowerUnlockData> allTowerData =
            TowerPresetManager.Instance.GetAllTowerUnlockData();

        foreach (TowerPresetManager.TowerUnlockData towerData in allTowerData)
        {
            Tower towerPrefab = towerData.towerPrefab;

            if (towerPrefab == null) continue;
            (bool isUnlocked, int requiredLevel) status =
                TowerPresetManager.Instance.GetUnlockStatus(towerPrefab);
            GameObject go = Instantiate(_towerDragItemPrefab, _allTowerListContainer.transform);

            TowerDragItem dragItem = go.GetComponent<TowerDragItem>();

            if (dragItem != null)
            {
                dragItem.Setup(towerPrefab, status.isUnlocked, status.requiredLevel);
            }
        }
    }
    public void RefreshAllSlots()
    {
        foreach (PresetSlot slot in _presetSlots)
        {
            slot.UpdateIcon();
        }
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshAllSlots();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}