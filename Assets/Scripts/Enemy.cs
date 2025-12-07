using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("=== ENEMY STATS ===")]
    [SerializeField] protected int _maxHealth = 100;

    [SerializeField] protected float _moveSpeed = 1f;

    [Header("=== UI/HEALTH ===")]
    [SerializeField] protected SpriteRenderer _healthBar;

    [SerializeField] protected SpriteRenderer _healthFill;

    [Header("=== EFFECTS ===")]
    [SerializeField] private GameObject _burnEffectPrefab;

    private GameObject _activeBurnEffect;

    [HideInInspector] public bool IsBurning = false;
    protected int _currentHealth;
    protected float _baseMoveSpeed;

    private float _burnTimer = 0f;
    private float _damagePerSecond = 0f;
    private float _damageAccumulator = 0f;

    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }
    public int CurrentHealth => _currentHealth;

    // scale gốc lấy từ prefab
    private Vector3 _healthFillBaseScale = Vector3.one;

    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0f, 0.5f, 0f);

    private void Awake()
    {
        if (_healthFill != null)
            _healthFillBaseScale = _healthFill.transform.localScale; // chính là chiều dài bạn set trong prefab
    }

    protected virtual void OnEnable()
    {
        _currentHealth = _maxHealth;
        _baseMoveSpeed = _moveSpeed;

        //if (_healthFill != null && _healthBar != null)
        //    _healthFill.size = _healthBar.size;

        // --- LOGIC MỚI: ĐỒNG BỘ KÍCH THƯỚC ---
        if (_healthFill != null)
        {
            //_healthFill.transform.localScale = Vector3.one;
            _healthFill.transform.localScale = _healthFillBaseScale;
            _healthFill.transform.localPosition = Vector3.zero;
        }

        if (_activeBurnEffect != null)
        {
            Destroy(_activeBurnEffect);
            _activeBurnEffect = null;
        }

        IsBurning = false;
        _burnTimer = 0f;
        _damagePerSecond = 0f; // Reset DPS
        _damageAccumulator = 0f;
    }

    protected virtual void Update()
    {
        // Cập nhật vị trí thanh máu
        if (_healthBar != null)
        {
            _healthBar.transform.position = transform.position + new Vector3(0, 0.5f, 0);

            _healthBar.transform.position = transform.position + _healthBarOffset;
        }

        // --- LOGIC SÁT THƯƠNG THEO THỜI GIAN (DOT) ---
        if (IsBurning)
        {
            _damageAccumulator += _damagePerSecond * Time.deltaTime;

            if (_damageAccumulator >= 1f)
            {
                int burnDmg = Mathf.FloorToInt(_damageAccumulator);
                ReduceEnemyHealth(burnDmg);
                _damageAccumulator -= burnDmg;
            }

            _burnTimer -= Time.deltaTime;

            if (_burnTimer <= 0)
            {
                if (_damageAccumulator > 0)
                {
                    int finalDmg = Mathf.CeilToInt(_damageAccumulator);
                    ReduceEnemyHealth(finalDmg);
                }

                StopBurnEffect();
                SetBurning(false);
                _damageAccumulator = 0f;
            }
        }
        // ------------------------------------------------
    }

    public virtual void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetPosition,
            _moveSpeed * Time.deltaTime
        );
    }

    public void SetTargetPosition(Vector3 position)
    {
        TargetPosition = position;
    }

    public void SetCurrentPathIndex(int index)
    {
        CurrentPathIndex = index;
    }

    public virtual void ReduceEnemyHealth(int damage)
    {
        if (damage <= 0) return;

        _currentHealth -= damage;

        AudioPlayer.Instance?.PlaySFX("hit-enemy");

        LevelManager.Instance.ShowDamageText(damage, transform.position);

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
        if (_currentHealth > 0) return;

        StopBurnEffect();

        gameObject.SetActive(false);
        AudioPlayer.Instance?.PlaySFX("enemy-die");
        LevelManager.Instance?.AddEnergy(20);

        // ** Sửa đổi này kích hoạt logic kiểm tra thắng **
        LevelManager.Instance?.EnemyKilledOrPassed();
        LevelManager.Instance?.DecreaseActiveEnemyCount(this);
    }

    protected virtual void UpdateHealthBar()
    {
        if (_healthFill == null || _healthBar == null) return;

        float ratio = (float)_currentHealth / _maxHealth;
        ratio = Mathf.Clamp01(ratio);

        //Vector2 size = _healthFill.size;
        //size.x = _healthBar.size.x * ratio;
        //_healthFill.size = size;

        //Vector3 position = _healthFill.transform.localPosition;
        //position.x = _healthBar.size.x * (ratio - 1) / 2f;
        //_healthFill.transform.localPosition = position;
        //Vector3 scale = _healthFill.transform.localScale;
        //scale.x = ratio;
        //_healthFill.transform.localScale = scale;

        Vector3 s = _healthFillBaseScale;
        s.x *= ratio;
        _healthFill.transform.localScale = s;
    }

    // --- LOGIC HIỆU ỨNG DOT ---
    public void SetBurning(bool burning)
    {
        IsBurning = burning;
    }

    public void ApplyBurnEffect(float duration, float damagePerSecond)
    {
        _burnTimer = Mathf.Max(_burnTimer, duration);
        _damagePerSecond = damagePerSecond;

        if (!IsBurning)
        {
            SetBurning(true);
            StartBurnEffect();
        }
    }

    // --- BURN EFFECT VISUALS ---
    public void StartBurnEffect()
    {
        if (_burnEffectPrefab != null && _activeBurnEffect == null)
        {
            _activeBurnEffect = Instantiate(_burnEffectPrefab, transform.position, Quaternion.identity, transform);

            ParticleSystem ps = _activeBurnEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = true;
                ps.Play();
            }
        }
    }

    public void StopBurnEffect()
    {
        if (_activeBurnEffect != null)
        {
            ParticleSystem ps = _activeBurnEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false;

                var main = ps.main;
                main.loop = false;
            }

            Destroy(_activeBurnEffect, 2f);
            _activeBurnEffect = null;
        }
    }

    public float GetBaseMoveSpeed()
    {
        return _baseMoveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        _moveSpeed = newSpeed;
    }
}