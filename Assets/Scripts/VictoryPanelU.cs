// VictoryPanelUI.cs

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
        if (PlayerPrefs.GetInt(UnlockKey, 0) == 1)
        {
            if (_unlockNotificationPanel != null)
            {
                _unlockNotificationPanel.SetActive(false);
            }
            return;
        }
        if (_unlockedTowerPrefab == null)
        {
            if (_unlockNotificationPanel != null) _unlockNotificationPanel.SetActive(false);
            Debug.LogWarning("[TOWER UNLOCK] _unlockedTowerPrefab chưa được gán. Bỏ qua thông báo.");
            return;
        }

        if (_unlockNotificationPanel != null)
        {
            _unlockNotificationPanel.SetActive(true);
        }

        if (_towerImageUI != null)
        {
            _towerImageUI.sprite = _unlockedTowerPrefab.GetTowerHeadIcon();
            _towerImageUI.gameObject.SetActive(true);
        }

        if (_messageTextUI != null)
        {
            _messageTextUI.text = $"NEW TOWER UNLOCKED:\n{_unlockedTowerPrefab.gameObject.name.Replace("(Clone)", "").Trim()}!";
        }

        PlayerPrefs.SetInt(UnlockKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"[TOWER UNLOCK] Thông báo Tower {_unlockedTowerPrefab.gameObject.name} lần đầu thành công.");
    }
}