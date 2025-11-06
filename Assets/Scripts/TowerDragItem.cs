// TowerDragItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Tháp mà icon này đại diện (được gán khi tạo danh sách)
    [HideInInspector] public Tower TowerPrefab;

    private Image _image;
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _image = GetComponent<Image>();
        // Cần CanvasGroup để làm mờ và tạm tắt Raycast
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetTower(Tower tower)
    {
        TowerPrefab = tower;
        if (tower != null)
        {
            _image.sprite = tower.GetTowerHeadIcon();
            _image.enabled = true;
        }
        else
        {
            _image.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        // Di chuyển icon lên cấp cao nhất của Canvas để luôn nằm trên các UI khác
        transform.SetParent(transform.root);

        _canvasGroup.alpha = 0.6f; // Làm mờ nhẹ
        _canvasGroup.blocksRaycasts = false; // Tắt raycast để OnDrop hoạt động
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Di chuyển theo vị trí con trỏ/ngón tay
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Trở về trạng thái ban đầu
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        // Nếu không thả vào PresetSlot nào, trả về vị trí ban đầu
        transform.SetParent(_originalParent);
        transform.localPosition = Vector3.zero;
    }
}