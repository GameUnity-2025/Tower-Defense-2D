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
        if (TowerPresetManager.Instance == null) return;

        foreach (Transform child in _allTowerListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Tower tower in TowerPresetManager.Instance.GetAllTowerPrefabs())
        {
            GameObject itemGO = Instantiate(_towerDragItemPrefab, _allTowerListContainer.transform);
            TowerDragItem dragItem = itemGO.GetComponent<TowerDragItem>();
            if (dragItem != null)
            {
                dragItem.SetTower(tower);
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