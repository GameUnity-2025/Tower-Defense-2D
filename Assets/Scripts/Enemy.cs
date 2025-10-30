using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int _maxHealth = 1;
    [SerializeField] protected float _moveSpeed = 1f; // TỐC ĐỘ CỐ ĐỊNH
    [SerializeField] protected SpriteRenderer _healthBar;
    [SerializeField] protected SpriteRenderer _healthFill;

    protected int _currentHealth;
    protected float _baseMoveSpeed;

    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }

    protected virtual void OnEnable()
    {
        _currentHealth = _maxHealth;
        _baseMoveSpeed = _moveSpeed; // Lưu tốc độ gốc

        if (_healthFill != null && _healthBar != null)
            _healthFill.size = _healthBar.size;
    }

    public virtual void MoveToTarget()
    {
        // DÙNG deltaTime → TĂNG TỐC KHI Time.timeScale > 1
        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetPosition,
            _moveSpeed * Time.deltaTime
        );
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        if (_healthBar != null)
            _healthBar.transform.parent = null;

        Vector3 distance = TargetPosition - transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Abs(distance.y) > Mathf.Abs(distance.x)
                ? (distance.y > 0 ? 90f : -90f)
                : (distance.x > 0 ? 0f : 180f)
        );

        if (_healthBar != null)
            _healthBar.transform.parent = transform;
    }

    public void SetCurrentPathIndex(int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }

    public virtual void ReduceEnemyHealth(int damage)
    {
        _currentHealth -= damage;
        AudioPlayer.Instance?.PlaySFX("hit-enemy");

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            UpdateHealthBar();
        }
    }

    protected virtual void Die()
    {
        _currentHealth = 0;
        gameObject.SetActive(false);
        AudioPlayer.Instance?.PlaySFX("enemy-die");
        LevelManager.Instance?.AddEnergy(20);
        LevelManager.Instance?.CheckWinCondition();
    }

    private void UpdateHealthBar()
    {
        if (_healthFill == null || _healthBar == null) return;
        float p = (float)_currentHealth / _maxHealth;
        _healthFill.size = new Vector2(p * _healthBar.size.x, _healthBar.size.y);
    }

    public float GetBaseMoveSpeed() => _baseMoveSpeed;
    public void SetMoveSpeed(float speed) => _moveSpeed = speed;
}