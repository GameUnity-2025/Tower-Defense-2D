// PresetSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PresetSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Đặt chỉ mục cho ô này: 0, 1, hoặc 2")]
    [SerializeField] public int slotIndex;

    private Image _iconImage;

    void Awake()
    {
        // Lấy Image Component (dùng để hiển thị icon tháp)
        _iconImage = GetComponent<Image>();
        if (_iconImage == null)
            Debug.LogError("PresetSlot requires an Image component to display the tower icon.");
    }

    private void OnEnable()
    {
        UpdateIcon();
    }

    // Cập nhật icon dựa trên dữ liệu Preset hiện tại
    public void UpdateIcon()
    {
        if (TowerPresetManager.Instance == null) return;

        Tower[] currentPreset = TowerPresetManager.Instance._currentPresetTowers;

        if (slotIndex < currentPreset.Length && currentPreset[slotIndex] != null)
        {
            _iconImage.sprite = currentPreset[slotIndex].GetTowerHeadIcon();
            _iconImage.color = Color.white;
        }
        else
        {
            // Thiết lập trạng thái khi ô trống
            _iconImage.sprite = null;
            _iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Lấy component kéo thả từ đối tượng đang được kéo
            TowerDragItem draggedItem = eventData.pointerDrag.GetComponent<TowerDragItem>();

            if (draggedItem != null && draggedItem.TowerPrefab != null)
            {
                if (TowerPresetManager.Instance.SetTowerInPreset(slotIndex, draggedItem.TowerPrefab))
                {
                    // Cập nhật UI của tất cả các slot sau khi Preset thay đổi
                    TowerSelectionPanel.Instance?.RefreshAllSlots();
                }
            }
        }
    }
}