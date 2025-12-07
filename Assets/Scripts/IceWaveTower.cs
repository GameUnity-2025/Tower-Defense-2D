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

    [Tooltip("Kích thước tối đa của sóng băng (final scale).")]
    [SerializeField] private float _waveSize = 3.0f;

    // === CÁC MẢNG CHỈ SỐ NÂNG CẤP ===
    // Cấp 1 (index 0), Cấp 2 (index 1), Cấp 3 (index 2)
    [Header("Upgrade Stats")]
    [Tooltip("Sát thương cho Level 1, 2, 3. (1, 2, 3)")]
    [SerializeField] private float[] _damageByLevel = new float[] { 1f, 2f, 3f };
    [Tooltip("Lượng làm chậm (0.x) cho Level 1, 2, 3. (0.3, 0.4, 0.5)")]
    [SerializeField] private float[] _slowAmountByLevel = new float[] { 0.3f, 0.4f, 0.5f };

    private float _timeSinceLastWave;

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
            Debug.LogError("IceWaveProjectile Prefab is missing!...");
            return;
        }

        // Lấy chỉ số tương ứng với cấp độ hiện tại (CurrentLevel bắt đầu từ 1, mảng bắt đầu từ 0)
        int index = CurrentLevel - 1;

        // Kiểm tra an toàn
        float currentDamage = (index >= 0 && index < _damageByLevel.Length) ? _damageByLevel[index] : _damageByLevel[0];
        float currentSlowAmount = (index >= 0 && index < _slowAmountByLevel.Length) ? _slowAmountByLevel[index] : _slowAmountByLevel[0];

        float waveFinalSize = _waveSize;

        GameObject waveObject = Instantiate(_iceWaveProjectilePrefab, transform.position, Quaternion.identity);
        IceWaveProjectile projectile = waveObject.GetComponent<IceWaveProjectile>();

        if (projectile != null)
        {
            // TRUYỀN CÁC CHỈ SỐ ĐÃ NÂNG CẤP VÀO PROJECTILE
            projectile.Initialize(waveFinalSize, currentDamage, currentSlowAmount);
        }
        else
        {
            Debug.LogError("IceWaveProjectile component not found on the instantiated prefab!...");
            Destroy(waveObject);
        }
    }

    // GHI ĐÈ: Lấy Sát thương hiện tại để hiển thị trong Panel
    public override float GetShootPower()
    {
        int index = CurrentLevel - 1;
        if (index >= 0 && index < _damageByLevel.Length)
        {
            return _damageByLevel[index];
        }
        return 0;
    }

    // HÀM MỚI: Lấy Slow Amount hiện tại để hiển thị trong Panel
    public float GetSlowAmount()
    {
        int index = CurrentLevel - 1;
        if (index >= 0 && index < _slowAmountByLevel.Length)
        {
            return _slowAmountByLevel[index];
        }
        return 0;
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