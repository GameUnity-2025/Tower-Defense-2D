using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // === COMPONENT ===
    [SerializeField] private SpriteRenderer _towerPlace;
    [SerializeField] private SpriteRenderer _towerHead;

    // === TOWER STATS ===
    [SerializeField] private int _shootPower = 1;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 5f;
    [SerializeField] private float _bulletSpeed = 1f;
    [SerializeField] private float _bulletSplashRadius = 0f;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _energyCost = 50;

    // === RUNTIME VARIABLES ===
    private float _runningShootDelay;
    private Enemy _targetEnemy;
    private Quaternion _targetRotation; // ĐÃ KHAI BÁO ĐÚNG

    // === PLACEMENT STATE ===
    private bool _isPlaced = false;
    public Vector2? PlacePosition { get; private set; }

    // === GETTERS ===
    public int EnergyCost => _energyCost;
    public int GetShootPower() => _shootPower;
    public float GetShootDistance() => _shootDistance;
    public float GetShootDelay() => _shootDelay;
    public float GetBulletSpeed() => _bulletSpeed;
    public float GetSplashRadius() => _bulletSplashRadius;

    // =============================================================
    private void Start()
    {
        _runningShootDelay = _shootDelay;
    }

    // === UI ICON ===
    public Sprite GetTowerHeadIcon()
    {
        return _towerHead != null ? _towerHead.sprite : null;
    }

    // === DRAG & DROP PLACEMENT ===
    public void SetPlacePosition(Vector2? newPosition)
    {
        PlacePosition = newPosition;
    }

    public void LockPlacement()
    {
        if (PlacePosition.HasValue)
        {
            transform.position = PlacePosition.Value;
            _isPlaced = true;
            PlacePosition = null; // Reset
        }
    }

    public void ToggleOrderInLayer(bool toFront)
    {
        int order = toFront ? 2 : 0;
        if (_towerPlace != null) _towerPlace.sortingOrder = order;
        if (_towerHead != null) _towerHead.sortingOrder = order + 1;
    }

    // === CLICK TO SHOW INFO PANEL ===
    private void Update()
    {
        if (!_isPlaced) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (GetComponent<Collider2D>().OverlapPoint(mousePos))
            {
                Debug.Log($"[TOWER] Clicked via Raycast on {name}");
                TowerInfoPanel.Instance?.ShowPanel(this, transform.position);
            }
        }
    }

    // === TOWER AI: FIND NEAREST ENEMY ===
    public void CheckNearestEnemy(List<Enemy> enemies)
    {
        if (!_isPlaced || enemies == null) return;

        // Reset nếu target không hợp lệ
        if (_targetEnemy != null)
        {
            if (!_targetEnemy.gameObject.activeSelf ||
                Vector3.Distance(transform.position, _targetEnemy.transform.position) > _shootDistance)
            {
                _targetEnemy = null;
            }
            else
            {
                return; // Vẫn trong tầm
            }
        }

        float nearestDist = Mathf.Infinity;
        Enemy nearest = null;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.gameObject.activeSelf) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > _shootDistance) continue;

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }

        _targetEnemy = nearest;
    }

    // === TOWER AI: ROTATE TO TARGET ===
    public void SeekTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _towerHead == null) return;

        Vector3 direction = _targetEnemy.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler(0, 0, angle - 90f);

        _towerHead.transform.rotation = Quaternion.RotateTowards(
            _towerHead.transform.rotation,
            _targetRotation,
            Time.deltaTime * 180f
        );
    }

    // === TOWER AI: SHOOT ===
    public void ShootTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _bulletPrefab == null) return;

        _runningShootDelay -= Time.unscaledDeltaTime;
        if (_runningShootDelay > 0f) return;

        // Chờ đầu tower xoay gần đúng hướng
        if (Quaternion.Angle(_towerHead.transform.rotation, _targetRotation) > 10f)
            return;

        Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);
        if (bullet != null)
        {
            bullet.transform.position = transform.position;
            bullet.SetProperties(_shootPower, _bulletSpeed, _bulletSplashRadius);
            bullet.SetTargetEnemy(_targetEnemy);
            bullet.gameObject.SetActive(true);
        }

        _runningShootDelay = _shootDelay;
    }

    // === VISUAL DEBUG: RANGE CIRCLE ===
    private void OnDrawGizmosSelected()
    {
        if (_isPlaced)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _shootDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _shootDistance);
        }
    }
}