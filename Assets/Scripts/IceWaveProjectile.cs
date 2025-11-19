using UnityEngine;
using System.Collections.Generic;

public class IceWaveProjectile : MonoBehaviour
{
    [Header("ICE WAVE STATS")]
    [Tooltip("Sát thương cơ bản mỗi khi xung kích chạm.")]
    [SerializeField] private float _damage = 15f;
    [Tooltip("Phần trăm làm chậm (ví dụ: 0.4f = 40% làm chậm).")]
    [SerializeField] private float _slowAmount = 0.4f;
    [Tooltip("Thời gian làm chậm kéo dài (giây).")]
    [SerializeField] private float _slowDuration = 2.0f;

    private List<Enemy> _hitEnemies = new List<Enemy>();

    // CẬP NHẬT: Hàm Initialize nhận tham số kích thước
    public void Initialize(float waveRange)
    {
        _hitEnemies.Clear();

        // Thiết lập Scale của Game Object (VFX và Collider con) dựa trên tham số
        // LƯU Ý: Nếu VFX của bạn được điều khiển bằng Animator, bạn cần điều chỉnh 
        // Animator để nó sử dụng transform.localScale này.
        transform.localScale = Vector3.one * waveRange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("IceWave va chạm với: " + other.gameObject.name);

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null && !_hitEnemies.Contains(enemy))
            {
                // 1. Gây Sát Thương
                enemy.ReduceEnemyHealth(Mathf.FloorToInt(_damage));

                // 2. KHẮC PHỤC LỖI COROUTINE: Kiểm tra activeSelf trước khi gọi ApplySlow
                if (enemy.gameObject.activeSelf)
                {
                    enemy.ApplySlow(_slowAmount, _slowDuration);
                }

                // 3. Đánh dấu kẻ địch này đã bị trúng đòn
                _hitEnemies.Add(enemy);
            }
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}