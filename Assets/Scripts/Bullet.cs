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

            // KIỂM TRA VA CHẠM SỚM (Ưu tiên Fix lỗi trên Mobile)
            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
            float hitThreshold = _bulletSpeed * Time.fixedDeltaTime;

            if (distanceToTarget <= hitThreshold) // Nếu đạn đã đủ gần (trong khoảng di chuyển tiếp theo)
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

    // HÀM GÂY SÁT THƯƠNG VÀ KẾT THÚC VÒNG ĐỜI ĐẠN (Đã sửa lỗi CS0103)
    private void HitTarget()
    {
        if (_targetEnemy == null || !_targetEnemy.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            _targetEnemy = null;
            return;
        }

        // Splash damage
        if (_bulletSplashRadius > 0f)
        {
            // LevelManager.Instance.ExplodeAt (giả sử hàm này tồn tại)
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
        // GỌI HitTarget nếu va chạm là mục tiêu đã định
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