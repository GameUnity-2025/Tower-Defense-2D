using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTower : Tower
{
    [Header("=== FIRE TOWER SETTINGS ===")]
    [SerializeField] private int _flameBurstCount = 8;
    [SerializeField] private float _flameSpreadAngle = 30f;
    [SerializeField] private float _flameBurstDelay = 0.1f;
    [SerializeField] private float _flameParticleScale = 1f;

    // ** LƯU Ý: Các mảng này đã được chuyển sang FireBullet.cs **

    private bool _isBursting = false;

    protected override void Start()
    {
        base.Start();
    }

    // ** KHÔNG CẦN CÁC HÀM GET NÀY NỮA, chúng đã được chuyển sang FireBullet **
    // public float GetCurrentBurnDPS() { ... }
    // public float GetCurrentBurnDuration() { ... }
    // Tuy nhiên, chúng ta cần tái tạo lại các hàm GET này để TowerInfoPanel có thể hiển thị. 
    // Chúng ta sẽ gọi FireBullet prefab để lấy thông tin.

    public override void ShootTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _bulletPrefab == null) return;

        _runningShootDelay -= Time.deltaTime;
        if (_runningShootDelay > 0f) return;

        // QUAY ĐẦU VỀ ENEMY (Logic quay đầu giữ nguyên)
        if (_towerHead != null)
        {
            Vector3 dir = _targetEnemy.transform.position - _towerHead.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _targetRotation = Quaternion.Euler(0, 0, angle - 90f);
            _towerHead.transform.rotation = Quaternion.RotateTowards(
              _towerHead.transform.rotation, _targetRotation, 360f * Time.deltaTime);
        }

        if (!_isBursting)
            StartCoroutine(FlameBurst());

        _runningShootDelay = _currentDelay;
    }

    private IEnumerator FlameBurst()
    {
        _isBursting = true;

        int currentLevel = CurrentLevel;
        int damage = 0; // Fire Bullet chỉ gây DOT

        float centerAngle = _towerHead != null ? _towerHead.transform.eulerAngles.z : transform.eulerAngles.z;

        for (int i = 0; i < _flameBurstCount; i++)
        {
            float spread = Random.Range(-_flameSpreadAngle / 2, _flameSpreadAngle / 2);
            float angle = centerAngle + spread;

            Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);

            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

                FireBullet fireBullet = bullet.GetComponent<FireBullet>();

                float speed = _bulletSpeed * 0.8f;

                // ** TRUYỀN CẤP ĐỘ THÁP VÀO BULLET **
                if (fireBullet != null)
                {
                    fireBullet.SetTowerLevel(currentLevel);
                }

                bullet.SetProperties(damage, speed, _bulletSplashRadius * 0.5f);

                bullet.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(_flameBurstDelay / _flameBurstCount);
        }

        _isBursting = false;
    }

    // CÁC HÀM GET MỚI CHO TowerInfoPanel
    public float GetCurrentBurnDPS()
    {
        FireBullet prefab = _bulletPrefab as FireBullet;
        if (prefab != null)
        {
            // Lấy DPS từ prefab (giả định FireBullet có logic này)
            return prefab.GetDPSForLevel(CurrentLevel);
        }
        return 0f;
    }

    public float GetCurrentBurnDuration()
    {
        FireBullet prefab = _bulletPrefab as FireBullet;
        if (prefab != null)
        {
            // Lấy Duration từ prefab
            return prefab.GetDurationForLevel(CurrentLevel);
        }
        return 0f;
    }
    private void OnDrawGizmosSelected()
    {
        if (!_isPlaced) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, _currentDistance);

        if (_towerHead != null)
        {
            float centerAngle = _towerHead.transform.eulerAngles.z;
            Vector3 forward = Quaternion.Euler(0, 0, centerAngle) * Vector3.up;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, forward * 1f);

            float leftAngle = centerAngle - _flameSpreadAngle / 2;
            float rightAngle = centerAngle + _flameSpreadAngle / 2;
            Vector3 left = Quaternion.Euler(0, 0, leftAngle) * Vector3.up;
            Vector3 right = Quaternion.Euler(0, 0, rightAngle) * Vector3.up;

            Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
            Gizmos.DrawRay(transform.position, left * 1.2f);
            Gizmos.DrawRay(transform.position, right * 1.2f);
        }
    }
}