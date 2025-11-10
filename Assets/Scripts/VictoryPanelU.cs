using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VictoryPanelUI : MonoBehaviour
{
    [Header("Tower Unlock Configuration")]
    [Tooltip("Prefab Tower sẽ được mở khóa khi thắng màn này.")]
    [SerializeField] private Tower _unlockedTowerPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject _unlockNotificationPanel;
    [SerializeField] private Image _towerImageUI;
    [SerializeField] private TMP_Text _messageTextUI;

    private string UnlockKey => "TowerUnlocked_" + (_unlockedTowerPrefab != null ? _unlockedTowerPrefab.gameObject.name : "None");

    public void CheckAndShowUnlockNotification()
    {
        // 1. Kiểm tra xem Tower đã được mở khóa chưa
        bool isAlreadyUnlocked = PlayerPrefs.GetInt(UnlockKey, 0) == 1;

        if (isAlreadyUnlocked)
        {
            // Nếu đã mở khóa, chỉ cần đảm bảo thông báo mở khóa bị TẮT
            if (_unlockNotificationPanel != null)
            {
                _unlockNotificationPanel.SetActive(false);
            }
            // Đảm bảo ẩn riêng Image Tower nếu nó không phải con của panel thông báo
            if (_towerImageUI != null && _towerImageUI.gameObject.activeSelf)
            {
                _towerImageUI.gameObject.SetActive(false);
            }
            return; // Thoát khỏi hàm vì đã mở khóa, không cần thông báo gì thêm
        }

        // 2. Kiểm tra có Tower để mở khóa không
        if (_unlockedTowerPrefab == null)
        {
            // Nếu không có Tower để mở khóa, đảm bảo ẩn mọi thứ liên quan đến việc mở khóa
            if (_unlockNotificationPanel != null) _unlockNotificationPanel.SetActive(false);
            if (_towerImageUI != null) _towerImageUI.gameObject.SetActive(false);
            Debug.LogWarning("[TOWER UNLOCK] _unlockedTowerPrefab chưa được gán. Bỏ qua thông báo.");
            return;
        }

        // 3. Nếu chưa mở khóa VÀ có Tower để mở khóa -> Kích hoạt hiển thị thông báo

        // Kích hoạt Panel chính
        if (_unlockNotificationPanel != null)
        {
            _unlockNotificationPanel.SetActive(true);
        }

        // Cấu hình Image Tower
        if (_towerImageUI != null)
        {
            _towerImageUI.sprite = _unlockedTowerPrefab.GetTowerHeadIcon();
            _towerImageUI.gameObject.SetActive(true);
        }

        // Cấu hình Text
        if (_messageTextUI != null)
        {
            _messageTextUI.text = $"NEW TOWER UNLOCKED:\n{_unlockedTowerPrefab.gameObject.name.Replace("(Clone)", "").Trim()}!";
        }

        // Lưu trạng thái mở khóa lần đầu
        PlayerPrefs.SetInt(UnlockKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"[TOWER UNLOCK] Thông báo Tower {_unlockedTowerPrefab.gameObject.name} lần đầu thành công.");
    }
}