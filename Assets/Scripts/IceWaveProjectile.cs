using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class IceWaveProjectile : MonoBehaviour
{
    // CÁC BIẾN STATS (GIỮ NGUYÊN)
    [Header("ICE WAVE STATS")]
    [Tooltip("Sát thương cơ bản mỗi khi xung kích chạm.")]
    [SerializeField] private float _damage = 15f;
    [Tooltip("Phần trăm làm chậm (ví dụ: 0.4f = 40% làm chậm).")]
    [SerializeField] private float _slowAmount = 0.4f;
    [Tooltip("Thời gian làm chậm kéo dài (giây).")]
    [SerializeField] private float _slowDuration = 2.0f;
    [Tooltip("Thời gian tồn tại của sóng băng (thời gian lan rộng).")]
    [SerializeField] private float _lifetime = 0.5f;

    [Header("KÍCH THƯỚC & COLLIDER")]
    [Tooltip("Kích thước tối đa mặc định (Width & Height) của Sprite Sliced.")]
    [SerializeField] private float _defaultSpriteSize = 7.0f;

    [Tooltip("Kích thước cuối cùng của Box Collider 2D (Ví dụ: 3.0).")]
    [SerializeField] private float _finalColliderSize = 3.0f;

    // <<< BIẾN MỚI: Tốc độ Lan rộng (1.0 là tốc độ bình thường)
    [Tooltip("Hệ số nhân tốc độ lan rộng (1.0 = mặc định). Giá trị cao hơn làm sóng lan nhanh hơn.")]
    [SerializeField] private float _growthSpeedMultiplier = 1.0f;

    // CÁC BIẾN THAM CHIẾU
    [Header("COLLIDER & VISUALS")]
    [Tooltip("Box Collider 2D hoặc Circle Collider 2D của sóng băng.")]
    [SerializeField] private BoxCollider2D _waveCollider;

    [Tooltip("Sprite Renderer để hiển thị hình ảnh sóng băng.")]
    [SerializeField] private SpriteRenderer _waveRenderer;

    private List<Enemy> _hitEnemies = new List<Enemy>();
    private float _finalSpriteRange;

    public void Initialize(float waveRange)
    {
        _hitEnemies.Clear();
        _finalSpriteRange = (waveRange > 0) ? waveRange : _defaultSpriteSize;

        // Đặt kích thước ban đầu về 0
        if (_waveRenderer != null)
        {
            _waveRenderer.size = Vector2.zero;
        }
        if (_waveCollider != null)
        {
            _waveCollider.size = Vector2.zero;
        }

        transform.localScale = Vector3.one;

        // Bắt đầu Coroutine tăng kích thước và tự hủy
        StartCoroutine(ScaleOverTime(_lifetime));
    }

    private IEnumerator ScaleOverTime(float duration)
    {
        float timer = 0f;

        if (_waveCollider != null)
        {
            _waveCollider.enabled = true;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime * _growthSpeedMultiplier; // ÁP DỤNG HỆ SỐ TỐC ĐỘ LAN RỘNG
            float ratio = timer / duration; // Tỷ lệ từ 0 đến 1

            // 1. TÍNH TOÁN VÀ ÁP DỤNG KÍCH THƯỚC SPRITE
            float currentSpriteSize = Mathf.Lerp(0f, _finalSpriteRange, ratio);
            if (_waveRenderer != null)
            {
                _waveRenderer.size = Vector2.one * currentSpriteSize;
            }

            // 2. TÍNH TOÁN VÀ ÁP DỤNG KÍCH THƯỚC COLLIDER
            float currentColliderSize = Mathf.Lerp(0f, _finalColliderSize, ratio);
            if (_waveCollider != null)
            {
                _waveCollider.size = Vector2.one * currentColliderSize;
            }

            yield return null;
        }

        // Đảm bảo kích thước cuối cùng được thiết lập chính xác
        if (_waveRenderer != null)
        {
            _waveRenderer.size = Vector2.one * _finalSpriteRange;
        }
        if (_waveCollider != null)
        {
            _waveCollider.size = Vector2.one * _finalColliderSize;
        }

        // Gọi hàm tự hủy
        DestroyProjectile();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null && !_hitEnemies.Contains(enemy))
            {
                // 1. Gây Sát Thương
                enemy.ReduceEnemyHealth(Mathf.FloorToInt(_damage));

                // 2. Áp dụng Slow
                if (enemy.gameObject.activeSelf)
                {
                    enemy.ApplySlow(_slowAmount, _slowDuration);
                }

                // 3. Đánh dấu kẻ địch đã trúng đòn
                _hitEnemies.Add(enemy);
            }
        }
    }

    public void DestroyProjectile()
    {
        // Tắt Collider trước khi hủy
        if (_waveCollider != null)
        {
            _waveCollider.enabled = false;
        }
        Destroy(gameObject);
    }
}