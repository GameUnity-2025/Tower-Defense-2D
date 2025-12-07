using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private int _debugDamageCount = 0;

    // BIẾN THAM CHIẾU COLLIDER ĐỂ TẮT KHI VỀ POOL (QUAN TRỌNG)
    private Collider2D _bulletCollider;

    // Property chỉ đọc để tính DPS/Duration dựa trên cấp độ hiện tại
    private float CurrentBurnDPS => GetDPSForLevel(_towerLevel);
    private float CurrentBurnDuration => GetDurationForLevel(_towerLevel);

    private void Awake()
    {
        // Lấy Collider 2D khi khởi tạo để sử dụng sau này
        _bulletCollider = GetComponent<Collider2D>();
    }

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

        // BẬT COLLIDER KHI ĐƯỢC KÍCH HOẠT TỪ POOL
        if (_bulletCollider != null)
        {
            _bulletCollider.enabled = true;
        }

        // RẤT QUAN TRỌNG: Loại bỏ logic tìm kiếm mục tiêu của Bullet
        _targetEnemy = null;

        _direction = transform.up;
        _travelledDistance = 0f;
        _lastDamageTime = 0f;
        _debugDamageCount = 0;
    }

    /// <summary>
    /// Được FireTower gọi để thiết lập cấp độ tháp đã bắn ra viên đạn này.
    /// </summary>
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
            // GỌI HÀM ĐỂ TẮT COLLIDER VÀ VỀ POOL
            DisableBullet();
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
                // SỬ DỤNG GIÁ TRỊ TỰ TÍNH TOÁN
                enemy.ApplyBurnEffect(CurrentBurnDuration, CurrentBurnDPS);
            }
        }
    }

    // HÀM MỚI: Quản lý việc tắt đạn và Collider
    private void DisableBullet()
    {
        // TẮT COLLIDER TRƯỚC KHI TẮT OBJECT
        if (_bulletCollider != null)
        {
            _bulletCollider.enabled = false;
        }
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, _damageRadius);
    }
}