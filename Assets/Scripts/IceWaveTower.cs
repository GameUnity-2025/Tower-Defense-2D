using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IceWaveTower : Tower
{
    [Header("Ice Wave Config")]
    [Tooltip("Prefab của đối tượng Sóng Băng (chứa IceWaveProjectile.cs)")]
    [SerializeField] private GameObject _iceWaveProjectilePrefab;

    [Tooltip("Thời gian giữa các lần tạo xung kích (tính bằng giây).")]
    [SerializeField] private float _waveDelay = 2.0f;

    // Biến này quyết định kích thước cuối cùng của sóng băng
    [Tooltip("Kích thước tối đa của sóng băng (final scale).")]
    [SerializeField] private float _waveSize = 3.0f;

    private float _timeSinceLastWave;
    // Đã xóa 'private Enemy _targetEnemy;' để tránh lỗi serialization (Giả định nằm ở lớp Tower)

    protected override void Start()
    {
        base.Start();
        _timeSinceLastWave = 0f;
    }

    protected void Update()
    {
        _timeSinceLastWave += Time.deltaTime;

        // Chỉ kích hoạt GenerateIceWave() khi có kẻ địch trong tầm bắn
        if (FindTarget() != null && _timeSinceLastWave >= _waveDelay)
        {
            GenerateIceWave();
            _timeSinceLastWave = 0f;
        }
    }

    private Enemy FindTarget()
    {
        // SỬ DỤNG GetShootDistance() (Tầm bắn của Tháp) để kiểm tra kích hoạt
        float range = GetShootDistance();
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    return enemy; // Trả về kẻ địch đầu tiên tìm thấy trong tầm bắn
                }
            }
        }
        return null;
    }

    private void GenerateIceWave()
    {
        if (_iceWaveProjectilePrefab == null)
        {
            Debug.LogError("IceWaveProjectile Prefab is missing! Please drag the IceWaveProjectile Prefab onto the tower's Inspector slot.");
            return;
        }

        // Kích thước cuối cùng của sóng được lấy từ biến _waveSize
        float waveFinalSize = _waveSize;

        GameObject waveObject = Instantiate(_iceWaveProjectilePrefab, transform.position, Quaternion.identity);
        IceWaveProjectile projectile = waveObject.GetComponent<IceWaveProjectile>();

        if (projectile != null)
        {
            // Truyền kích thước cuối cùng vào Projectile
            projectile.Initialize(waveFinalSize);
        }
        else
        {
            Debug.LogError("IceWaveProjectile component not found on the instantiated prefab! Make sure the Prefab has the script attached.");
            Destroy(waveObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // Hiển thị Phạm vi Kích hoạt (Tầm bắn của Tháp)
        Gizmos.DrawWireSphere(transform.position, GetShootDistance());

        Gizmos.color = Color.blue;
        // Hiển thị Kích thước Tối đa của Sóng (Kích thước Projectile)
        Gizmos.DrawWireSphere(transform.position, _waveSize);
    }
}