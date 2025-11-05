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

    [Header("=== BURN DOT STATS (INDEX 0: LV1, 1: LV2, 2: LV3) ===")]
    // DPS: 0.5, 1.5, 2.5
    [SerializeField] private float[] _burnDamagePerSecondByLevel = { 0.5f, 1.5f, 2.5f };
    // Duration: 2.0, 3.0, 4.0
    [SerializeField] private float[] _burnDurationByLevel = { 2.0f, 3.0f, 4.0f };

    private bool _isBursting = false;

    protected override void Start()
    {
        base.Start();
    }

    // Lấy DPS hiện tại (động)
    public float GetCurrentBurnDPS()
    {
        // CurrentLevel là 1-based (1, 2, 3), mảng là 0-based (0, 1, 2)
        int index = CurrentLevel - 1;

        if (index >= 0 && index < _burnDamagePerSecondByLevel.Length)
        {
            return _burnDamagePerSecondByLevel[index];
        }
        return 0f;
    }

    // Lấy Duration hiện tại (động)
    public float GetCurrentBurnDuration()
    {
        int index = CurrentLevel - 1;

        if (index >= 0 && index < _burnDurationByLevel.Length)
        {
            return _burnDurationByLevel[index];
        }
        return 0f;
    }

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

        // Lấy chỉ số DOT hiện tại (động theo cấp độ)
        float currentDPS = GetCurrentBurnDPS();
        float currentDuration = GetCurrentBurnDuration();
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

                bullet.SetProperties(damage, speed, _bulletSplashRadius * 0.5f);

                Debug.Log($"[FireTower] Fired bullet: DPS={currentDPS}, Duration={currentDuration}");

                bullet.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(_flameBurstDelay / _flameBurstCount);
        }

        _isBursting = false;
    }

    private void SpawnFlameParticle(Vector3 position, Quaternion rotation)
    {
        // Logic Particle System (giữ nguyên)
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