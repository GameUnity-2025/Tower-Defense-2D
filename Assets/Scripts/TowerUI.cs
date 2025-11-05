using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _towerIcon;

    private Tower _towerPrefab;
    private Tower _currentSpawnedTower;

    void Start()
    {

    }

    void Update()
    {

    }

    public void SetTowerPrefab(Tower tower)
    {
        _towerPrefab = tower;
        _towerIcon.sprite = tower.GetTowerHeadIcon();
    }

    // Called once when dragging starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!LevelManager.Instance.CanPlaceTower(_towerPrefab))
        {
            _currentSpawnedTower = null;
            return;
        }

        GameObject newTowerObj = Instantiate(_towerPrefab.gameObject);
        _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
        _currentSpawnedTower.ToggleOrderInLayer(true);
    }

    // Called while dragging
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        Camera mainCamera = Camera.main;

        // Use eventData.position for accurate touch/mouse tracking
        Vector3 pointerPosition = eventData.position;

        pointerPosition.z = -mainCamera.transform.position.z;
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(pointerPosition);
        _currentSpawnedTower.transform.position = targetPosition;
    }

    // Called once when dragging ends
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        if (_currentSpawnedTower.PlacePosition == null)
        {
            Destroy(_currentSpawnedTower.gameObject);
        }
        else
        {
            if (LevelManager.Instance.CanPlaceTower(_currentSpawnedTower))
            {
                _currentSpawnedTower.LockPlacement();
                _currentSpawnedTower.ToggleOrderInLayer(false);
                LevelManager.Instance.RegisterSpawnedTower(_currentSpawnedTower);

                LevelManager.Instance.AddEnergy(-_currentSpawnedTower.EnergyCost);
            }
            else
            {
                Destroy(_currentSpawnedTower.gameObject);
            }

            _currentSpawnedTower = null;
        }
    }
}