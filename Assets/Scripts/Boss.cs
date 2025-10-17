using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private int _bossMaxHealth = 1; // Máu tối đa cao hơn

    //[SerializeField] private float _bossMoveSpeed = 1f; // Tốc độ nhanh hơn
    [SerializeField] private GameObject _projectilePrefab; // Prefab đạn của Boss

    [SerializeField] private float _attackRate = 1f; // Tần suất bắn (giây)
    [SerializeField] private float _projectileSpeed = 1f; // Tốc độ đạn

    private float _attackTimer;

    protected override void OnEnable()
    {
        // Gọi OnEnable của Enemy để khởi tạo máu và thanh máu
        _maxHealth = _bossMaxHealth;
        //_moveSpeed = _bossMoveSpeed;
        _attackTimer = 0f;
        base.OnEnable();
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
        // Có thể thêm logic đặc biệt, ví dụ: rung lắc nhẹ khi di chuyển
        transform.position += new Vector3(
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.05f, 0.05f),
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
            // Tạo đạn tại vị trí của Boss
            GameObject projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            // Thêm lực để đạn di chuyển (giả sử hướng ngẫu nhiên hoặc hướng về phía trước)
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.right;
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
        for (int i = 0; i < 3; i++)
        {
            if (_healthFill != null)
            {
                _healthFill.enabled = false;
                yield return new WaitForSeconds(0.2f);
                _healthFill.enabled = true;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}