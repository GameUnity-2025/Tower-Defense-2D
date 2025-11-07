using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Cần thiết nếu chưa có

public class FireBullet : Bullet
{
    [Header("=== FIRE BULLET SETTINGS ===")]
    private int _towerLevel = 1; // Biến lưu trữ cấp độ

    // DPS: 0.5, 1.5, 2.5
    [Header("=== BURN DOT STATS (INDEX 0: LV1, 1: LV2, 2: LV3) ===")]
    [SerializeField] private float[] _burnDamagePerSecondByLevel = { 0.5f, 1.5f, 2.5f };
    // Duration: 2.0, 3.0, 4.0
    [SerializeField] private float[] _burnDurationByLevel = { 2.0f, 3.0f, 4.0f };

    [SerializeField] private float _maxTravelDistance = 5f;
    [SerializeField] private float _damageInterval = 0.1f;
    [SerializeField] private LayerMask _enemyLayer = -1;
    [SerializeField] private float _damageRadius = 0.5f;

    private float _travelledDistance = 0f;
    private float _lastDamageTime = 0f;
    private Vector2 _direction;

    private float CurrentBurnDPS => GetDPSForLevel(_towerLevel);
    private float CurrentBurnDuration => GetDurationForLevel(_towerLevel);

    // === HÀM GET ĐỂ TOWER/UI TRUY CẬP ===
    public float GetDPSForLevel(int level)
    {
        int index = level - 1;
        if (index >= 0 && index < _burnDamagePerSecondByLevel.Length)
        {
            return _burnDamagePerSecondByLevel[index];
        }
        return 0f;
    }

    public float GetDurationForLevel(int level)
    {
        int index = level - 1;
        if (index >= 0 && index < _burnDurationByLevel.Length)
        {
            return _burnDurationByLevel[index];
        }
        return 0f;
    }
    // === HẾT HÀM GET ===

    protected override void OnEnable()
    {
        base.OnEnable();
        _targetEnemy = null;

        _direction = transform.up;
        _travelledDistance = 0f;
        _lastDamageTime = 0f;
    }


    public void SetTowerLevel(int level)
    {
        _towerLevel = level;
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        float moveDistance = BulletSpeed * Time.deltaTime;
        transform.Translate(_direction * moveDistance, Space.World);
        _travelledDistance += moveDistance;

        if (Time.time - _lastDamageTime >= _damageInterval)
        {
            DamageEnemiesInArea();
            _lastDamageTime = Time.time;
        }

        if (_travelledDistance >= _maxTravelDistance)
        {
            gameObject.SetActive(false);
        }
    }

    private void DamageEnemiesInArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _damageRadius, _enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyBurnEffect(CurrentBurnDuration, CurrentBurnDPS);
            }
        }
    }
    private void OnDrawGizmos()

    {

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        Gizmos.DrawSphere(transform.position, _damageRadius); 

    }

}