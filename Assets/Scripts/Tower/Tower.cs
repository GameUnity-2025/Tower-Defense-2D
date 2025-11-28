using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // << Cần thiết cho mobile/touch

public class Tower : MonoBehaviour, IPointerClickHandler // << IMPLEMENT INTERFACE NÀY
{
    // === COMPONENT ===
    [SerializeField] protected SpriteRenderer _towerPlace;
    [SerializeField] protected SpriteRenderer _towerHead;


    // === BASE STATS (LEVEL 1) ===
    [SerializeField] private int _shootPower = 1;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 5f;
    [SerializeField] protected float _bulletSpeed = 1f;
    [SerializeField] protected float _bulletSplashRadius = 0f;
    [SerializeField] protected Bullet _bulletPrefab;
    [SerializeField] private int _energyCost = 50;

    // === UPGRADE ===
    [SerializeField] private int[] _upgradeCosts = { 100, 200 }; // Lv.2, Lv.3
    [SerializeField] private float _damageMultiplier = 1.5f;
    [SerializeField] private float _rangeMultiplier = 1.2f;
    [SerializeField] private float _fireRateMultiplier = 0.7f;

    // === RUNTIME ===
    protected int _currentLevel = 1;
    protected const int MAX_LEVEL = 3;
    protected float _currentPower;
    protected float _currentDistance;
    protected float _currentDelay;
    protected float _runningShootDelay;
    protected Enemy _targetEnemy;
    protected Quaternion _targetRotation;
    protected bool _isPlaced = false;
    protected float _baseShootDelay;
    public Vector2? PlacePosition { get; private set; }
    public int EnergyCost => _energyCost;
    public int CurrentLevel => _currentLevel;

    // === GETTERS ===
    public virtual float GetShootPower() => Mathf.RoundToInt(_currentPower);
    public float GetShootDistance() => _currentDistance;
    public float GetShootDelay()
    {
        return Mathf.Max(_shootDelay, 0.001f);
    }
    public int GetUpgradeCost() => _currentLevel < MAX_LEVEL ? _upgradeCosts[_currentLevel - 1] : 0;

    // === BURN ===
    [HideInInspector] public bool IsBurning = false;
    protected virtual void Start()
    {
        ResetStats();
        _runningShootDelay = _currentDelay;
        _baseShootDelay = _shootDelay;
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

    // === UPGRADE ===
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
            PlacePosition = null;
        }
        _isPlaced = true;
        gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();
        LevelManager.Instance?.RegisterSpawnedTower(this); 
    }

    public void ToggleOrderInLayer(bool toFront)
    {
        int order = toFront ? 2 : 0;
        if (_towerPlace != null) _towerPlace.sortingOrder = order;
        if (_towerHead != null) _towerHead.sortingOrder = order + 1;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[TOWER TAP] Tower: {gameObject.name} clicked/tapped. _isPlaced: {_isPlaced}");

        if (!_isPlaced)
        {
            Debug.LogWarning($"[TOWER TAP] Tower: {gameObject.name} is NOT placed. Panel blocked.");
            return;
        }

        if (TowerInfoPanel.Instance != null)
        {
            if (TowerInfoPanel.Instance.gameObject.activeSelf)
            {
                TowerInfoPanel.Instance.HidePanel();
            }
            TowerInfoPanel.Instance.ShowPanel(this, this.transform.position);
        }
        else
        {
            Debug.LogError("TowerInfoPanel.Instance IS NULL! Vấn đề nằm ở thiết lập Scene/UI.");
        }
        Debug.Log($"[TOWER TAP] Success! Calling ShowPanel for: {gameObject.name}");
        TowerInfoPanel.Instance?.ShowPanel(this, transform.position);
    }


    // === TOWER AI  ===
    public void CheckNearestEnemy()
    {
        if (!_isPlaced) return;

        List<Enemy> enemies = LevelManager.Instance.GetEnemies();
        if (enemies == null) return;

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

    public virtual void ShootTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _bulletPrefab == null) return;
        _runningShootDelay -= Time.deltaTime;
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

    public float GetBaseShootDelay() => _baseShootDelay;

    public void SetShootDelay(float delay)
    {
        _currentDelay = delay;
        _runningShootDelay = delay;
    }

    public void SetBurning(bool burning)
    {
        IsBurning = burning;
    }

    protected void Update()
    {

    }
}