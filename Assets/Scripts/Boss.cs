using Assets.Scripts;
using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private int _bossMaxHealth = 500;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _attackRate = 1.5f;
    [SerializeField] private float _projectileSpeed = 3f;

    private float _attackTimer;
    private Vector2 _originalHealthBarSize = new Vector2(0.75f, 0.15f);

    private void Awake()
    {
        if (_healthBar != null)
            _originalHealthBarSize = _healthBar.size;
    }

    protected override void OnEnable()
    {
        _maxHealth = _bossMaxHealth;
        base.OnEnable();

        if (_healthBar != null && _healthFill != null)
        {
            float scale = _maxHealth / 100f;
            _healthBar.size = new Vector2(_originalHealthBarSize.x * scale, _originalHealthBarSize.y);
            _healthFill.size = _healthBar.size;
        }
        _attackTimer = 0f;
    }

    protected new void Update()
    {
        base.Update();

        MoveToTarget();
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackRate)
        {
            Attack();
            _attackTimer = 0f;
        }
    }

    public override void MoveToTarget()
    {
        base.MoveToTarget();
        transform.position += new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            0f
        ) * Time.unscaledDeltaTime;
    }

    public override void ReduceEnemyHealth(int damage)
    {
        // Gọi logic trừ máu và Die() từ lớp cha
        base.ReduceEnemyHealth(damage);

        if (_currentHealth > 0)
        {
            if (_currentHealth <= _maxHealth * 0.2f && _healthFill != null)
                StartCoroutine(FlashHealthBar());
        }
    }

    private void Attack()
    {
        if (_projectilePrefab == null) return;
        GameObject p = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rb.velocity = dir * _projectileSpeed;
        }
        AudioPlayer.Instance?.PlaySFX("boss-attack");
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