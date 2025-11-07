using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected int _bulletPower;
    protected float _bulletSpeed;
    protected float _bulletSplashRadius;
    protected Enemy _targetEnemy;

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

            Vector3 targetPosition = _targetEnemy.transform.position;
            Vector3 currentPosition = transform.position;

            // KIỂM TRA VA CHẠM SỚM (Fix lỗi trên Mobile)
            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
            float hitThreshold = _bulletSpeed * Time.fixedDeltaTime;

            // Lỗi CS0103 xảy ra tại đây (Dòng 37 trong code cũ)
            if (distanceToTarget <= hitThreshold)
            {
                HitTarget();
                return;
            }
            // Hết kiểm tra va chạm sớm

            // Di chuyển đến enemy
            transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                _bulletSpeed * Time.fixedDeltaTime
            );

            // Quay đầu theo hướng
            Vector3 direction = targetPosition - currentPosition;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
        }
    }

    // HÀM GÂY SÁT THƯƠNG VÀ KẾT THÚC VÒNG ĐỜI ĐẠN
    // Sửa lỗi CS0106 và CS1520: Đã thêm 'void' và đặt hàm ngoài FixedUpdate.
    protected virtual void HitTarget()
    {
        if (_targetEnemy == null || !_targetEnemy.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            _targetEnemy = null;
            return;
        }

        // Logic sát thương SPLASH hoặc SÁT THƯƠNG ĐƠN
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


    // OnTriggerEnter2D chỉ còn là cơ chế dự phòng
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lỗi CS0103 xảy ra tại đây (Dòng 88 trong code cũ)
        if (_targetEnemy != null && collision.gameObject.Equals(_targetEnemy.gameObject))
        {
            HitTarget();
        }
    }

    // --- Các hàm thiết lập ---

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

    protected virtual void OnEnable()
    {
        // Sẽ được override bởi các Bullet cụ thể (ví dụ: FireBullet)
    }
}