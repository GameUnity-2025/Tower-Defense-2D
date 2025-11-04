using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected int _bulletPower;
    protected float _bulletSpeed;        // ĐỔI: protected
    protected float _bulletSplashRadius; // ĐỔI: protected
    protected Enemy _targetEnemy;

    // THÊM: Getter để FireBullet truy cập
    public float BulletSpeed => _bulletSpeed;
    public int BulletPower => _bulletPower;

    private void FixedUpdate()
    {
        if (LevelManager.Instance.IsOver) return;

        if (_targetEnemy != null)
        {
            if (!_targetEnemy.gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                _targetEnemy = null;
                return;
            }

            // Di chuyển đến enemy
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetEnemy.transform.position,
                _bulletSpeed * Time.fixedDeltaTime
            );

            // Quay đầu theo hướng
            Vector3 direction = _targetEnemy.transform.position - transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targetEnemy == null) return;

        if (collision.gameObject.Equals(_targetEnemy.gameObject))
        {
            // Splash damage
            if (_bulletSplashRadius > 0f)
            {
                LevelManager.Instance.ExplodeAt(transform.position, _bulletSplashRadius, _bulletPower);
            }
            else
            {
                _targetEnemy.ReduceEnemyHealth(_bulletPower);
            }

            gameObject.SetActive(false);
            _targetEnemy = null;
        }
    }

    public void SetProperties(int bulletPower, float bulletSpeed, float bulletSplashRadius)
    {
        _bulletPower = bulletPower;
        _bulletSpeed = bulletSpeed;
        _bulletSplashRadius = bulletSplashRadius;
    }

    public void SetTargetEnemy(Enemy enemy)
    {
        _targetEnemy = enemy;
    }

    // THÊM: Reset khi tái sử dụng từ pool
    protected virtual void OnEnable()
    {
        // Sẽ được override bởi FireBullet
    }
}