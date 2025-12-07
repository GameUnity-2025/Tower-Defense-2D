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

    // --- THÊM: Biến cho Hiệu ứng Làm Chậm (Slow) ---
    [SerializeField] private GameObject _slowEffectPrefab; // Prefab VFX làm chậm

    private GameObject _activeSlowEffect;
    private Coroutine _slowCoroutine;
    [HideInInspector] public bool IsSlowed = false;
    // ------------------------------------------------

    [HideInInspector] public bool IsBurning = false;
    protected int _currentHealth;
    protected float _baseMoveSpeed; // Tốc độ gốc, dùng để khôi phục sau hiệu ứng

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
        _baseMoveSpeed = _moveSpeed; // Lưu tốc độ gốc

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

        // Reset Slow Effect
        if (_activeSlowEffect != null)
        {
            Destroy(_activeSlowEffect);
            _activeSlowEffect = null;
        }
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
            _slowCoroutine = null;
        }
        // ----------------------------------------

        IsBurning = false;
        IsSlowed = false;
        _burnTimer = 0f;
        _damagePerSecond = 0f;
        _damageAccumulator = 0f;

        // Đặt lại tốc độ di chuyển về tốc độ gốc
        _moveSpeed = _baseMoveSpeed;
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
        // Sử dụng _moveSpeed đã được điều chỉnh bởi hiệu ứng slow
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
        StopSlowEffect(); // Dừng hiệu ứng slow khi chết

        gameObject.SetActive(false);
        AudioPlayer.Instance?.PlaySFX("enemy-die");
        LevelManager.Instance?.AddEnergy(20);

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

    // --- LOGIC HIỆU ỨNG LÀM CHẬM (SLOW) ---

    /// <summary>
    /// Áp dụng hiệu ứng làm chậm lên kẻ địch.
    /// </summary>
    /// <param name="slowAmount">Phần trăm làm chậm (ví dụ: 0.5f = 50%).</param>
    /// <param name="duration">Thời gian hiệu ứng kéo dài (giây).</param>
    public void ApplySlow(float slowAmount, float duration)
    {
        // Tính tốc độ mới (ví dụ: slowAmount = 0.4f -> tốc độ còn 60%)
        float newSpeed = _baseMoveSpeed * (1f - Mathf.Clamp01(slowAmount));

        // Nếu kẻ địch đã bị chậm và hiệu ứng mới KHÔNG chậm hơn, ta không làm gì cả
        if (IsSlowed && newSpeed >= _moveSpeed)
        {
            // Tuy nhiên, ta vẫn kéo dài thời gian hiệu ứng hiện tại (nếu cần)
            if (_slowCoroutine != null)
            {
                // Dừng coroutine cũ và bắt đầu coroutine mới với duration dài hơn
                StopCoroutine(_slowCoroutine);
                _slowCoroutine = StartCoroutine(SlowDurationCoroutine(duration));
            }
            return;
        }

        // Dừng coroutine cũ để bắt đầu hiệu ứng mới/mạnh hơn
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
        }

        // Áp dụng tốc độ mới, cờ trạng thái, và bắt đầu VFX
        _moveSpeed = newSpeed;
        IsSlowed = true;
        StartSlowEffect();
        _slowCoroutine = StartCoroutine(SlowDurationCoroutine(duration));
    }

    private IEnumerator SlowDurationCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Khôi phục tốc độ và trạng thái
        _moveSpeed = _baseMoveSpeed;
        IsSlowed = false;
        StopSlowEffect();
        _slowCoroutine = null;
    }

    // --- VFX CHO HIỆU ỨNG SLOW ---
    public void StartSlowEffect()
    {
        if (_slowEffectPrefab != null && _activeSlowEffect == null)
        {
            // Gắn hiệu ứng VFX là con của Enemy
            _activeSlowEffect = Instantiate(_slowEffectPrefab, transform.position, Quaternion.identity, transform);

            ParticleSystem ps = _activeSlowEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = true;
                ps.Play();
            }
        }
    }

    public void StopSlowEffect()
    {
        if (_activeSlowEffect != null)
        {
            ParticleSystem ps = _activeSlowEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Ngừng phát hạt, nhưng cho phép các hạt hiện tại kết thúc
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Hủy đối tượng VFX sau 2 giây (để các hạt kết thúc tự nhiên)
            Destroy(_activeSlowEffect, 2f);
            _activeSlowEffect = null;
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
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            Destroy(_activeBurnEffect, 2f);
            _activeBurnEffect = null;
        }
    }

    public float GetBaseMoveSpeed()
    {
        return _baseMoveSpeed;
    }

    // Hàm SetMoveSpeed cũ đã được thay thế bằng logic ApplySlow
    // public void SetMoveSpeed(float newSpeed)
    // {
    //     _moveSpeed = newSpeed;
    // }
}