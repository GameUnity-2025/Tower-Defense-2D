using UnityEngine;

public class DualTower : Tower
{
    [Header("=== DUAL TOWER SETTINGS ===")]
    [SerializeField] private Transform _leftMuzzle;
    [SerializeField] private Transform _rightMuzzle;
    [SerializeField] private float _muzzleOffset = 0.3f;

    protected override void Start()
    {
        base.Start();
        SetupMuzzles();
    }

    private void SetupMuzzles()
    {
        if (_leftMuzzle == null || _rightMuzzle == null)
        {
            GameObject left = new GameObject("LeftMuzzle");
            GameObject right = new GameObject("RightMuzzle");

            left.transform.SetParent(transform);
            right.transform.SetParent(transform);

            left.transform.localPosition = new Vector3(-_muzzleOffset, 0, 0);
            right.transform.localPosition = new Vector3(_muzzleOffset, 0, 0);

            _leftMuzzle = left.transform;
            _rightMuzzle = right.transform;
        }
    }

    public override void ShootTarget()
    {
        if (_targetEnemy == null || !_isPlaced || _bulletPrefab == null) return;

        _runningShootDelay -= Time.deltaTime;
        if (_runningShootDelay > 0f) return;

        // SỬA: DÙNG .transform CỦA SpriteRenderer
        if (_towerHead != null)
        {
            Vector3 dir = _targetEnemy.transform.position - _towerHead.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _targetRotation = Quaternion.Euler(0, 0, angle - 90f);
            _towerHead.transform.rotation = Quaternion.RotateTowards(
                _towerHead.transform.rotation, _targetRotation, 180f * Time.deltaTime);
        }

        if (Quaternion.Angle(_towerHead.transform.rotation, _targetRotation) > 10f) return;

        // BẮN TỪ 2 NÒNG
        ShootFromMuzzle(_leftMuzzle);
        ShootFromMuzzle(_rightMuzzle);

        _runningShootDelay = _currentDelay;
    }

    private void ShootFromMuzzle(Transform muzzle)
    {
        Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);
        if (bullet != null)
        {
            bullet.transform.position = muzzle.position;
            bullet.transform.rotation = muzzle.rotation;
            bullet.SetProperties(Mathf.RoundToInt(_currentPower), _bulletSpeed, _bulletSplashRadius);
            bullet.SetTargetEnemy(_targetEnemy);
            bullet.gameObject.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_leftMuzzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_leftMuzzle.position, 0.1f);
        }
        if (_rightMuzzle != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_rightMuzzle.position, 0.1f);
        }
    }
}