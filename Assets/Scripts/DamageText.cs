using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f; // Đã tăng tốc độ di chuyển
    [SerializeField] private float _duration = 1.5f; // Đã tăng thời gian tồn tại
    [SerializeField] private Color _defaultColor = Color.red; // Đặt màu mặc định là Đỏ
    [SerializeField] private TMP_Text _textComponent;

    private float _timeElapsed = 0f;

    // >> CHÚ Ý: Hàm nhận tham số là float thay vì int <<
    public void SetDamageValue(float damage)
    {
        if (_textComponent == null)
        {
            _textComponent = GetComponent<TMP_Text>();
            if (_textComponent == null)
            {
                Debug.LogError("DamageText script không tìm thấy TMP_Text component!");
                Destroy(gameObject);
                return;
            }
        }

        // --- LOGIC HIỂN THỊ SÁT THƯƠNG VÀ MÀU SẮC ---
        if (damage < 0f)
        {
            // Hồi máu (Sát thương âm)
            _textComponent.text = Mathf.Abs(damage).ToString("F1");
            _textComponent.color = Color.green; // Hiện màu Xanh lá
        }
        else
        {
            // Sát thương (Sát thương dương)
            // "F1" để hiển thị một chữ số sau dấu thập phân (ví dụ: 0.5)
            _textComponent.text = damage.ToString("F1");
            _textComponent.color = _defaultColor; // Hiện màu đã đặt (ví dụ: Đỏ)
        }
        // --- END LOGIC ---

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        // Di chuyển Text lên (sử dụng Space.World)
        transform.Translate(Vector3.up * _moveSpeed * Time.deltaTime, Space.World);

        // Logic mờ dần (Alpha fade)
        _timeElapsed += Time.deltaTime;
        float alpha = 1f - (_timeElapsed / _duration);

        if (_textComponent != null)
        {
            Color tempColor = _textComponent.color;
            tempColor.a = alpha;
            _textComponent.color = tempColor;
        }
    }
}