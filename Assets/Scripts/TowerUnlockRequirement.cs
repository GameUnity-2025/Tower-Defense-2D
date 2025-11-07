using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerUnlockRequirement : MonoBehaviour
{
    [Tooltip("Số level tối thiểu phải hoàn thành để mở khóa tháp này. Nhập 0 nếu tháp luôn mở khóa.")]
    [SerializeField]
    private int _requiredLevel = 0;

    public int RequiredLevel => _requiredLevel;
}