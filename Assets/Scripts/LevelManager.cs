using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance = null;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<LevelManager>();
            return _instance;
        }
    }

    /* ---------- UI & TOWERS ---------- */
    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;
    private List<Tower> _spawnedTowers = new List<Tower>();
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private Transform _damageTextContainer;

    /* ---------- ENEMIES ---------- */
    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;
    [SerializeField] private Boss _bossPrefab;
    private bool _hasBossSpawned = false;
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private float _runningSpawnDelay;
    private List<Bullet> _spawnedBullets = new List<Bullet>();
    public List<Enemy> GetEnemies() => _spawnedEnemies;

    // ** KHAI BÁO BIẾN ĐẾM MỚI **
    private int _activeEnemyCount = 0;
    private bool _isBossActive = false;

    /* ---------- GAME STATE ---------- */
    public bool IsOver { get; private set; }
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;
    [SerializeField] private GameObject _panel; // Panel Game Over/Victory
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;
    [SerializeField] private TMPro.TextMeshProUGUI _energyInfo;

    private int _currentLives;
    private int _enemyCounter;

    /* ---------- ENERGY ---------- */
    [SerializeField] private int _initialEnergy = 300;
    private int _currentEnergy;
    [SerializeField] private int _energyIncreasePerSecond = 10;
    [SerializeField] private int _energyFromEnemy = 20;
    private float _energyTimer = 0f;

    public int GetCurrentEnergy() => _currentEnergy;


    /* ============================================================= */
    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        _currentEnergy = _initialEnergy;
        SetEnergy(_currentEnergy);
        InstantiateAllTowerUI();
        _runningSpawnDelay = _spawnDelay;
        _hasBossSpawned = false;
        IsOver = false;

        // ** KHỞI TẠO BIẾN ĐẾM **
        _activeEnemyCount = 0;
        _isBossActive = false;

        if (_panel != null) _panel.SetActive(false);
        InitializeExistingTowers();
        // ** Đảm bảo Time.timeScale được đặt về 1 khi bắt đầu màn chơi **
        Time.timeScale = 1f;
    }

    private void InitializeExistingTowers()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            Tower[] existingTowers = FindObjectsOfType<Tower>();

            foreach (Tower t in existingTowers)
            {
                if (!_spawnedTowers.Contains(t))
                {
                    RegisterSpawnedTower(t);
                    if (t.PlacePosition == null)
                    {
                        t.SetPlacePosition(t.transform.position);
                    }
                    t.LockPlacement();
                }
            }
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (IsOver || Time.timeScale <= 0f) return;

        // SPAWN NORMAL ENEMY
        if (_enemyCounter > 0)
        {
            _runningSpawnDelay -= Time.deltaTime;
            if (_runningSpawnDelay <= 0f)
            {
                SpawnNormalEnemy();
                _runningSpawnDelay = _spawnDelay;
            }
        }

        // TOWERS
        foreach (Tower t in _spawnedTowers)
        {
            if (t == null) continue;
            t.CheckNearestEnemy();
            t.SeekTarget();
            t.ShootTarget();
        }

        // ENEMIES (Sử dụng for ngược để tránh InvalidOperationException)
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy e = _spawnedEnemies[i];

            if (!e.gameObject.activeSelf) continue;

            if (Vector2.Distance(e.transform.position, e.TargetPosition) < 0.1f)
            {
                e.SetCurrentPathIndex(e.CurrentPathIndex + 1);
                if (e.CurrentPathIndex < _enemyPaths.Length)
                    e.SetTargetPosition(_enemyPaths[e.CurrentPathIndex].position);
                else
                {
                    // Logic khi Enemy ĐI HẾT PATH
                    if (e is Boss)
                    {
                        // If Boss đi qua đích -> THUA
                        SetGameOver(false);

                        // Cập nhật trạng thái trước khi tắt
                        if (e.gameObject.activeSelf)
                        {
                            e.gameObject.SetActive(false);
                            // Cần gọi DecreaseActiveEnemyCount để cập nhật biến _isBossActive
                            DecreaseActiveEnemyCount(e);
                        }
                        return; // Thoát ngay
                    }
                    else
                    {
                        // If normal enemy đi qua đích -> Mất Mạng
                        ReduceLives(1);

                        // Cập nhật trạng thái trước khi tắt
                        if (e.gameObject.activeSelf)
                        {
                            e.gameObject.SetActive(false);
                            // Cần gọi DecreaseActiveEnemyCount để cập nhật biến _activeEnemyCount
                            DecreaseActiveEnemyCount(e);
                        }
                    }
                }
            }
            else
            {
                e.MoveToTarget();
            }
        }

        // ENERGY
        _energyTimer += Time.deltaTime;
        if (_energyTimer >= 1f)
        {
            AddEnergy(_energyIncreasePerSecond);
            _energyTimer = 0f;
        }
    }

    /* ============================================================= */
    private void SpawnNormalEnemy()
    {
        SetTotalEnemy(--_enemyCounter);
        int idx = Random.Range(0, _enemyPrefabs.Length);
        string nameKey = (idx + 1).ToString();
        GameObject obj = _spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(nameKey))?.gameObject;
        if (obj == null) obj = Instantiate(_enemyPrefabs[idx].gameObject);
        Enemy enemy = obj.GetComponent<Enemy>();
        if (!_spawnedEnemies.Contains(enemy)) _spawnedEnemies.Add(enemy);
        enemy.transform.position = _enemyPaths[0].position;
        enemy.SetTargetPosition(_enemyPaths[1].position);
        enemy.SetCurrentPathIndex(1);
        enemy.gameObject.SetActive(true);
        _activeEnemyCount++; // Tăng đếm khi spawn
    }

    // HÀM ĐƯỢC GỌI KHI ENEMY BỊ TIÊU DIỆT HOẶC ĐI QUA ĐÍCH
    public void EnemyKilledOrPassed()
    {
        // Giảm đếm khi Enemy/Boss bị tiêu diệt bởi Tower
        if (!IsOver)
        {
            // Chỉ gọi kiểm tra điều kiện thắng
            CheckWinCondition();
        }
    }

    // ** HÀM MỚI: GIẢM BIẾN ĐẾM KHI KẺ THÙ CHẾT **
    public void DecreaseActiveEnemyCount(Enemy enemy)
    {
        if (enemy is Boss)
        {
            _isBossActive = false;
        }
        else
        {
            // Đảm bảo không giảm dưới 0
            _activeEnemyCount = Mathf.Max(0, _activeEnemyCount - 1);
        }
        // Gọi kiểm tra điều kiện thắng sau khi cập nhật biến đếm
        EnemyKilledOrPassed();
    }


    // ** HÀM SỬA ĐỔI: SỬ DỤNG BIẾN ĐẾM ĐỂ KIỂM TRA THẮNG **
    public void CheckWinCondition()
    {
        if (IsOver) return;

        // 1. LOGIC SPAWN BOSS:
        if (_bossPrefab != null && !_hasBossSpawned && _enemyCounter <= 0 && _activeEnemyCount <= 0)
        {
            SpawnBoss();
            _hasBossSpawned = true;
            return;
        }

        // 2. LOGIC THẮNG: (Không còn quái nào để spawn, và không còn quái nào đang hoạt động)
        if (_enemyCounter <= 0 && _activeEnemyCount <= 0 && !_isBossActive)
        {
            SetGameOver(true);
        }
    }

    private void SpawnBoss()
    {
        if (_bossPrefab == null) return;

        GameObject go = Instantiate(_bossPrefab.gameObject);
        Boss boss = go.GetComponent<Boss>();
        if (boss == null)
        {
            Debug.LogError("Boss Prefab does not have a Boss component!");
            Destroy(go);
            return;
        }

        if (!_spawnedEnemies.Contains(boss)) _spawnedEnemies.Add(boss);
        boss.transform.position = _enemyPaths[0].position;
        boss.SetTargetPosition(_enemyPaths[1].position);
        boss.SetCurrentPathIndex(1);
        boss.gameObject.SetActive(true);
        _isBossActive = true; // Đánh dấu Boss đang hoạt động

        if (_statusInfo != null)
        {
            _statusInfo.text = "BOSS APPEARED!";
            Invoke("ClearBossMessage", 2f);
        }
    }

    private void ClearBossMessage()
    {
        if (!IsOver) _statusInfo.text = "";
    }

    /* ============================================================= */
    private void InstantiateAllTowerUI()
    {
        Tower[] presetTowers = TowerPresetManager.Instance?.GetCurrentTowerPreset();

        if (presetTowers == null || presetTowers.Length == 0)
        {
            Debug.LogError("Tower Preset Manager not found or preset is empty! Check Menu Scene configuration.");
            return;
        }
        foreach (Tower t in presetTowers)
        {
            if (t == null) continue;

            GameObject ui = Instantiate(_towerUIPrefab, _towerUIParent);
            TowerUI tui = ui.GetComponent<TowerUI>();
            tui.SetTowerPrefab(t);
            ui.name = t.name;
        }
    }
    public void RegisterSpawnedTower(Tower t) => _spawnedTowers.Add(t);
    public void RegisterSpawnedTowerRemoval(Tower tower) => _spawnedTowers.Remove(tower);

    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject obj = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;
        if (obj == null) obj = Instantiate(prefab.gameObject);
        Bullet b = obj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(b)) _spawnedBullets.Add(b);
        return b;
    }

    public void ExplodeAt(Vector2 pos, float radius, int dmg)
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy e = _spawnedEnemies[i];
            if (e.gameObject.activeSelf && Vector2.Distance(e.transform.position, pos) <= radius)
            {
                e.ReduceEnemyHealth(dmg);
            }
        }
    }

    public void ReduceLives(int v)
    {
        SetCurrentLives(_currentLives - v);
        if (_currentLives <= 0) SetGameOver(false);
    }

    public void SetCurrentLives(int v)
    {
        _currentLives = Mathf.Max(v, 0);
        if (_livesInfo != null)
            _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy(int v)
    {
        _enemyCounter = v;
        if (_totalEnemyInfo != null)
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }

    public void SetGameOver(bool win)
    {
        if (IsOver) return; // Bảo vệ: Không gọi 2 lần

        IsOver = true;
        // ** QUAN TRỌNG: Dừng game **
        Time.timeScale = 0f;

        if (_statusInfo != null)
            _statusInfo.text = win ? "You Win!" : "You Lose!";

        // HIỂN THỊ PANEL
        if (_panel != null)
        {
            _panel.SetActive(true);
        }
        else
        {
            Debug.LogError("GAME OVER PANEL (_panel) IS NOT ASSIGNED IN THE INSPECTOR!");
        }

        if (win)
        {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            int nextLevel = currentLevel + 1;

            // Logic lưu PlayerPrefs giữ nguyên
            int unlockedLevel = PlayerPrefs.GetInt("LastLevel", 1);
            if (nextLevel > unlockedLevel)
            {
                PlayerPrefs.SetInt("LastLevel", nextLevel);
            }
            int maxCompletedLevel = PlayerPrefs.GetInt("MaxCompletedLevel", 0);
            if (currentLevel > maxCompletedLevel)
            {
                PlayerPrefs.SetInt("MaxCompletedLevel", currentLevel);
            }

            // ** GỌI VICTORY PANEL (Đã sửa lỗi gọi lặp) **
            if (_panel != null)
            {
                // Tìm kiếm component trong chính Panel và các con (kể cả con đang tắt)
                VictoryPanelUI victoryUI = _panel.GetComponentInChildren<VictoryPanelUI>(true);

                if (victoryUI != null)
                {
                    victoryUI.CheckAndShowUnlockNotification();
                }
                else
                {
                    Debug.LogWarning("VictoryPanelUI component not found on the _panel GameObject or its children. Skipping tower unlock notification.");
                }
            }
            PlayerPrefs.Save();
        }
    }

    public void OnPlaceTowerButtonClicked()
    {
        TowerPlacement p = FindObjectOfType<TowerPlacement>();
        if (p != null)
        {
            Tower t = p.GetPlacedTower();
            if (t != null && CanPlaceTower(t))
            {
                p.LockTowerPlacement();
            }
        }
    }

    public void AddEnergy(int v)
    {
        _currentEnergy += v;
        SetEnergy(_currentEnergy);
    }

    public void SetEnergy(int v)
    {
        _currentEnergy = v;
        if (_energyInfo == null)
        {
            GameObject obj = GameObject.Find("EnergyText");
            if (obj != null)
                _energyInfo = obj.GetComponent<TMPro.TextMeshProUGUI>();
        }
        if (_energyInfo != null)
            _energyInfo.text = $"Energy: {_currentEnergy}";
    }

    public bool CanPlaceTower(Tower t)
    {
        if (t == null) return false;
        return _currentEnergy >= t.EnergyCost;
    }

    public void ShowDamageText(int damage, Vector3 position)
    {
        if (_damageTextPrefab == null) return;
        Vector3 spawnPosition = position;
        spawnPosition.y += 0.5f;
        spawnPosition.z = -1f;
        GameObject damageTextGO = Instantiate(_damageTextPrefab, spawnPosition, Quaternion.identity);
        if (_damageTextContainer != null)
        {
            damageTextGO.transform.SetParent(_damageTextContainer, true);
            damageTextGO.transform.localScale = Vector3.one;
        }

        DamageText damageText = damageTextGO.GetComponent<DamageText>();
        if (damageText != null)
        {
            damageText.SetDamageValue(damage);
        }
    }
}