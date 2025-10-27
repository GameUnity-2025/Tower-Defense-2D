using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private int _bossMaxHealth = 500; // Máu tối đa cao hơn
    [SerializeField] private GameObject _projectilePrefab; // Prefab đạn của Boss
    [SerializeField] private float _attackRate = 1.5f; // Tần suất bắn (giây)
    [SerializeField] private float _projectileSpeed = 3f; // Tốc độ đạn
    private float _attackTimer;

    private Vector2 _originalHealthBarSize = new Vector2(0.75f, 0.15f);

    private void Awake()
    {
        // Ghi nhớ kích thước gốc (hoặc đặt thủ công)
        if (_healthBar != null)
            _originalHealthBarSize = _healthBar.size;
    }

    protected override void OnEnable()
    {
        _maxHealth = _bossMaxHealth;
        base.OnEnable();

        // Tự động mở rộng thanh máu theo _maxHealth
        if (_healthBar != null && _healthFill != null)
        {
            float scaleFactor = _maxHealth / 100f;
            Vector2 baseSize = _healthBar.size;
            _healthBar.size = new Vector2(baseSize.x * scaleFactor, baseSize.y);
            _healthFill.size = _healthBar.size;
        }

        // Mỗi lần clone hoặc reset enemy, ép lại size đúng
        if (_healthBar != null)
            _healthBar.size = _originalHealthBarSize;
        if (_healthFill != null)
            _healthFill.size = _originalHealthBarSize;

        _attackTimer = 0f;
    }

    private void Update()
    {
        // Di chuyển đến mục tiêu
        MoveToTarget();

        // Tấn công định kỳ
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackRate)
        {
            Attack();
            _attackTimer = 0f;
        }
    }

    public override void MoveToTarget()
    {
        // Gọi hàm di chuyển của Enemy
        base.MoveToTarget();

        // Rung lắc nhẹ khi di chuyển (hiệu ứng boss)
        transform.position += new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            0f
        ) * Time.deltaTime;
    }

    public override void ReduceEnemyHealth(int damage)
    {
        base.ReduceEnemyHealth(damage);

        // Hiệu ứng nhấp nháy thanh máu khi máu dưới 20%
        if (_currentHealth > 0 && _currentHealth <= _maxHealth * 0.2f && _healthFill != null)
        {
            StartCoroutine(FlashHealthBar());
        }
    }

    private void Attack()
    {
        if (_projectilePrefab != null)
        {
            GameObject projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Bắn theo 8 hướng (hoặc ngẫu nhiên)
                float angle = Random.Range(0f, 360f);
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                rb.velocity = direction * _projectileSpeed;
            }

            if (AudioPlayer.Instance != null)
            {
                AudioPlayer.Instance.PlaySFX("boss-attack");
            }
        }
    }

    private IEnumerator FlashHealthBar()
    {
        for (int i = 0; i < 5; i++)
        {
            if (_healthFill != null)
            {
                _healthFill.enabled = false;
                yield return new WaitForSeconds(0.15f);
                _healthFill.enabled = true;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}