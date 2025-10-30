using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // === COMPONENT ===
    [SerializeField] private SpriteRenderer _towerPlace;
    [SerializeField] private SpriteRenderer _towerHead;

    // === BASE STATS (CẤP 1) ===
    [SerializeField] private int _shootPower = 1;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 5f;
    [SerializeField] private float _bulletSpeed = 1f;
    [SerializeField] private float _bulletSplashRadius = 0f;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _energyCost = 50;

    // === NÂNG CẤP ===
    [SerializeField] private int[] _upgradeCosts = { 100, 200 }; // Lv.2, Lv.3
    [SerializeField] private float _damageMultiplier = 1.5f;
    [SerializeField] private float _rangeMultiplier = 1.2f;
    [SerializeField] private float _fireRateMultiplier = 0.7f;

    // === RUNTIME ===
    private int _currentLevel = 1;
    private const int MAX_LEVEL = 3;
    private float _currentPower;
    private float _currentDistance;
    private float _currentDelay;
    private float _runningShootDelay;
    private Enemy _targetEnemy;
    private Quaternion _targetRotation;
    private bool _isPlaced = false;

    public Vector2? PlacePosition { get; private set; }
    public int EnergyCost => _energyCost;
    public int CurrentLevel => _currentLevel;

    // === GETTERS ===
    public int GetShootPower() => Mathf.RoundToInt(_currentPower);
    public float GetShootDistance() => _currentDistance;
    public float GetShootDelay() => _currentDelay;
    public int GetUpgradeCost() => _currentLevel < MAX_LEVEL ? _upgradeCosts[_currentLevel - 1] : 0;

    private void Start()
    {
        ResetStats();
        _runningShootDelay = _currentDelay;
    }

    private void ResetStats()
    {
        _currentPower = _shootPower;
        _currentDistance = _shootDistance;
        _currentDelay = _shootDelay;
        for (int i = 1; i < _currentLevel; i++)
        {
            _currentPower *= _damageMultiplier;
            _currentDistance *= _rangeMultiplier;
            _currentDelay *= _fireRateMultiplier;
        }
    }

    public Sprite GetTowerHeadIcon()
    {
        return _towerHead != null ? _towerHead.sprite : null;
    }

    // === NÂNG CẤP ===
    public bool CanUpgrade()
    {
        return _currentLevel < MAX_LEVEL &&
               LevelManager.Instance != null &&
               LevelManager.Instance.GetCurrentEnergy() >= GetUpgradeCost();
    }

    public void Upgrade()
    {
        if (_currentLevel >= MAX_LEVEL) return;

        int cost = _upgradeCosts[_currentLevel - 1];
        if (LevelManager.Instance == null || LevelManager.Instance.GetCurrentEnergy() < cost) return;

        LevelManager.Instance.AddEnergy(-cost);
        _currentLevel++;
        ResetStats();
        _runningShootDelay = _currentDelay;

        Debug.Log($"[TOWER] Upgraded to Level {_currentLevel}!");
    }

    // === DRAG & DROP PLACEMENT ===
    public void SetPlacePosition(Vector2? newPosition) => PlacePosition = newPosition;

    public void LockPlacement()
    {
        if (PlacePosition.HasValue)
        {
            transform.position = PlacePosition.Value;
            _isPlaced = true;
            PlacePosition = null;
            gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();
        }
    }

    public void ToggleOrderInLayer(bool toFront)
    {
        int order = toFront ? 2 : 0;
        if (_towerPlace != null) _towerPlace.sortingOrder = order;
        if (_towerHead != null) _towerHead.sortingOrder = order + 1;
    }

    // === CLICK TO SHOW PANEL ===
    private void Update()
    {
        if (!_isPlaced || !Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.OverlapPoint(mousePos))
        {
            TowerInfoPanel.Instance?.ShowPanel(this, transform.position);
        }
    }

    // === TOWER AI ===
    public void CheckNearestEnemy(List<Enemy> enemies)
    {
        if (!_isPlaced || enemies == null) return;

        if (_targetEnemy != null)
        {
            if (!_targetEnemy.gameObject.activeSelf || Vector3.Distance(transform.position, _targetEnemy.transform.position) > _currentDistance)
                _targetEnemy = null;
            else return;
        }

        float nearestDist = Mathf.Infinity;
        Enemy nearest = null;
        foreach (Enemy e in enemies)
        {
            if (!e.gameObject.activeSelf) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > _currentDistance) continue;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }
        _targetEnemy = nearest;
    }

    public void SeekTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _towerHead == null) return;

        Vector3 dir = _targetEnemy.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler(0, 0, angle - 90f);

        _towerHead.transform.rotation = Quaternion.RotateTowards(
            _towerHead.transform.rotation, _targetRotation, Time.deltaTime * 180f);
    }

    public void ShootTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _bulletPrefab == null) return;

        _runningShootDelay -= Time.unscaledDeltaTime;
        if (_runningShootDelay > 0f) return;
        if (Quaternion.Angle(_towerHead.transform.rotation, _targetRotation) > 10f) return;

        Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);
        if (bullet != null)
        {
            bullet.transform.position = transform.position;
            bullet.SetProperties(Mathf.RoundToInt(_currentPower), _bulletSpeed, _bulletSplashRadius);
            bullet.SetTargetEnemy(_targetEnemy);
            bullet.gameObject.SetActive(true);
        }

        _runningShootDelay = _currentDelay;
    }

    private void OnDrawGizmosSelected()
    {
        if (_isPlaced)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _currentDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _currentDistance);
        }
    }
}