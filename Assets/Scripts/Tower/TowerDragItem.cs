// TowerDragItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; 

public class TowerDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Tower TowerPrefab;

    [Header("Unlock Status UI")]
    [SerializeField] private Image _lockOverlay;
    [SerializeField] private TMP_Text _requiredLevelText; 

    private Image _image;
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private bool _isLocked = false; 

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

        if (towerPrefab != null)
        {
            _image.sprite = towerPrefab.GetTowerHeadIcon();
            _image.enabled = true;
        }
        else
        {
            _image.enabled = false;
        }

        if (_lockOverlay != null)
        {
            _lockOverlay.gameObject.SetActive(_isLocked);
        }

        if (_requiredLevelText != null)
        {
            _requiredLevelText.text = $"Lv {requiredLevel}";
            _requiredLevelText.gameObject.SetActive(_isLocked);
        }

        if (_image != null)
        {
            _image.color = _isLocked ? new Color(0.5f, 0.5f, 0.5f, 0.7f) : Color.white;
        }
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