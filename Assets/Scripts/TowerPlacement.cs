using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_placedTower != null) return;

        Tower tower = collision.GetComponent<Tower>();
        // The check for CanPlaceTower should ideally happen in TowerUI/LevelManager 
        // to prevent spawning, but this is a good safety check for placement validation.
        if (tower != null && LevelManager.Instance.CanPlaceTower(tower))
        {
            tower.SetPlacePosition(transform.position);
            _placedTower = tower;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_placedTower == null) return;

        if (collision.GetComponent<Tower>() == _placedTower)
        {
            _placedTower.SetPlacePosition(null);
            _placedTower = null;
        }
    }

    public void LockTowerPlacement()
    {
        if (_placedTower != null)
        {
            _placedTower.LockPlacement();
            _placedTower = null;
        }
    }

    public Tower GetPlacedTower() => _placedTower;
}