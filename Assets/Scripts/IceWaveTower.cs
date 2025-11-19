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

    [Tooltip("Kích thước tối đa của sóng băng (ảnh hưởng đến Scale và phạm vi va chạm).")]
    [SerializeField] private float _waveSize = 3.0f; // BIẾN MỚI

    private float _timeSinceLastWave;
    private Enemy _targetEnemy;

    protected override void Start()
    {
        base.Start();
        _timeSinceLastWave = 0f;
    }

    protected void Update()
    {
        _timeSinceLastWave += Time.deltaTime;

        _targetEnemy = FindTarget();

        if (_targetEnemy != null && _timeSinceLastWave >= _waveDelay)
        {
            GenerateIceWave();
            _timeSinceLastWave = 0f;
        }
    }

    private Enemy FindTarget()
    {
        // SỬ DỤNG VẬT LÝ 2D: Physics2D.OverlapCircleAll
        float range = GetShootDistance();
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Trả về kẻ địch đầu tiên tìm thấy trong tầm bắn của tháp
                    return enemy;
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

        GameObject waveObject = Instantiate(_iceWaveProjectilePrefab, transform.position, Quaternion.identity);
        IceWaveProjectile projectile = waveObject.GetComponent<IceWaveProjectile>();

        if (projectile != null)
        {
            // GỌI HÀM KHỞI TẠO MỚI VỚI KÍCH THƯỚC
            projectile.Initialize(_waveSize);
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
        // Gizmos.DrawWireSphere(transform.position, GetShootDistance()); 
    }
}