// TowerDragItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TowerDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Tower TowerPrefab;

    [Header("Unlock Status UI")]
    // Loại bỏ Image _lockOverlay
    [SerializeField] private TMP_Text _requiredLevelText;

    // THÊM BIẾN NÀY ĐỂ KÉO SPRITE KHÓA VÀO TRONG INSPECTOR
    [Tooltip("Sprite sẽ thay thế icon Tower khi item này bị khóa.")]
    [SerializeField] private Sprite _lockedItemSprite; // <--- BIẾN MỚI CHO TRẠNG THÁI KHÓA

    private Image _image;
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private bool _isLocked = false;
    private Sprite _unlockedSprite; // Lưu trữ sprite gốc khi mở khóa

    void Awake()
    {
        _image = GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(Tower towerPrefab, bool isUnlocked, int requiredLevel)
    {
        TowerPrefab = towerPrefab;
        _isLocked = !isUnlocked;

        // 1. CẤU HÌNH SPRITE GỐC
        if (towerPrefab != null)
        {
            // Lấy và lưu trữ Sprite gốc (Sprite của Tower Head)
            _unlockedSprite = towerPrefab.GetTowerHeadIcon();
            _image.enabled = true;
        }
        else
        {
            _image.enabled = false;
        }

        // 2. THAY THẾ SPRITE DỰA TRÊN TRẠNG THÁI KHÓA
        if (_isLocked)
        {
            // Thay thế hình ảnh Tower bằng hình ảnh Khóa
            if (_lockedItemSprite != null)
            {
                _image.sprite = _lockedItemSprite;
            }
            // Đảm bảo item bị khóa không bị làm mờ thêm
            _image.color = Color.white;
        }
        else
        {
            // Sử dụng hình ảnh Tower gốc (đã lưu ở bước 1)
            _image.sprite = _unlockedSprite;
            _image.color = Color.white;
        }


        // 3. HIỂN THỊ CẤP ĐỘ YÊU CẦU
        if (_requiredLevelText != null)
        {
            // Chỉ hiển thị cấp độ yêu cầu khi bị khóa
            _requiredLevelText.text = $"Lv {requiredLevel}";
            _requiredLevelText.gameObject.SetActive(_isLocked);
        }

        // Cài đặt lại khả năng tương tác (chỉ để đề phòng)
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isLocked)
        {
            return;
        }

        _originalParent = transform.parent;
        transform.SetParent(transform.root);

        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isLocked)
        {
            return;
        }
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isLocked)
        {
            return;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        transform.SetParent(_originalParent);
        transform.localPosition = Vector3.zero;
    }
}