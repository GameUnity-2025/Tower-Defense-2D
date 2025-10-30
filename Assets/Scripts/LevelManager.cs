using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private Tower[] _towerPrefabs;
    private List<Tower> _spawnedTowers = new List<Tower>();

    /* ---------- ENEMIES ---------- */
    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;
    [SerializeField] private Boss _bossPrefab;
    private bool _hasBossSpawned = false;
    private bool _bossRequired = false;
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private float _runningSpawnDelay;
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    /* ---------- GAME STATE ---------- */
    public bool IsOver { get; private set; }
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;
    [SerializeField] private GameObject _panel;
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
        _bossRequired = IsBossLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (IsOver || Time.timeScale <= 0f) return;

        /* ---- SPAWN QUÁI THƯỜNG ---- */
        if (_enemyCounter > 0)
        {
            _runningSpawnDelay -= Time.deltaTime;
            if (_runningSpawnDelay <= 0f)
            {
                SpawnNormalEnemy();
                _runningSpawnDelay = _spawnDelay;
            }
        }

        /* ---- TOWERS ---- */
        foreach (Tower t in _spawnedTowers)
        {
            t.CheckNearestEnemy(_spawnedEnemies);
            t.SeekTarget();
            t.ShootTarget();
        }

        /* ---- ENEMIES ---- */
        foreach (Enemy e in _spawnedEnemies)
        {
            if (!e.gameObject.activeSelf) continue;
            if (Vector2.Distance(e.transform.position, e.TargetPosition) < 0.1f)
            {
                e.SetCurrentPathIndex(e.CurrentPathIndex + 1);
                if (e.CurrentPathIndex < _enemyPaths.Length)
                    e.SetTargetPosition(_enemyPaths[e.CurrentPathIndex].position);
                else
                {
                    ReduceLives(1);
                    e.gameObject.SetActive(false);
                }
            }
            else
            {
                e.MoveToTarget();
            }
        }

        /* ---- ENERGY ---- */
        _energyTimer += Time.unscaledDeltaTime;
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
    }

    private bool IsBossLevel() => SceneManager.GetActiveScene().buildIndex == 5;

    private void SpawnBoss()
    {
        if (_bossPrefab == null) { Debug.LogError("Boss Prefab missing!"); return; }
        GameObject go = Instantiate(_bossPrefab.gameObject);
        Enemy boss = go.GetComponent<Enemy>();
        if (!_spawnedEnemies.Contains(boss)) _spawnedEnemies.Add(boss);
        boss.transform.position = _enemyPaths[0].position;
        boss.SetTargetPosition(_enemyPaths[1].position);
        boss.SetCurrentPathIndex(1);
        boss.gameObject.SetActive(true);
        _hasBossSpawned = true;

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
    public void CheckWinCondition()
    {
        if (_enemyCounter > 0) return;

        bool hasActiveEnemy = false;
        bool hasActiveBoss = false;

        foreach (Enemy e in _spawnedEnemies)
        {
            if (e.gameObject.activeSelf)
            {
                hasActiveEnemy = true;
                if (e is Boss) hasActiveBoss = true;
            }
        }

        if (!hasActiveEnemy && (!_bossRequired || _hasBossSpawned))
        {
            SetGameOver(true);
            return;
        }

        if (_bossRequired && !hasActiveBoss && !_hasBossSpawned && !hasActiveEnemy)
        {
            SpawnBoss();
            _hasBossSpawned = true;
        }
    }

    /* ============================================================= */
    private void InstantiateAllTowerUI()
    {
        foreach (Tower t in _towerPrefabs)
        {
            GameObject ui = Instantiate(_towerUIPrefab, _towerUIParent);
            TowerUI tui = ui.GetComponent<TowerUI>();
            tui.SetTowerPrefab(t);
            ui.name = t.name;
        }
    }

    public void RegisterSpawnedTower(Tower t) => _spawnedTowers.Add(t);
    public void RegisterSpawnedTowerRemoval(Tower tower) => _spawnedTowers.Remove(tower);

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

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
        foreach (Enemy e in _spawnedEnemies)
            if (e.gameObject.activeSelf && Vector2.Distance(e.transform.position, pos) <= radius)
                e.ReduceEnemyHealth(dmg);
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
        IsOver = true;
        if (_statusInfo != null)
            _statusInfo.text = win ? "You Win!" : "You Lose!";
        if (_panel != null)
            _panel.SetActive(true);

        if (win)
        {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            int nextLevel = currentLevel + 1;
            int unlockedLevel = PlayerPrefs.GetInt("LastLevel", 1);
            if (nextLevel > unlockedLevel)
            {
                PlayerPrefs.SetInt("LastLevel", nextLevel);
                PlayerPrefs.Save();
                Debug.Log($"[LevelManager] Unlocked Level {nextLevel}");
            }
        }
    }

    public void OnPlaceTowerButtonClicked()
    {
        TowerPlacement p = FindObjectOfType<TowerPlacement>();
        if (p != null)
        {
            Tower t = p.GetPlacedTower();
            if (t != null && CanPlaceTower(t)) // CHỈ KIỂM TRA
            {
                p.LockTowerPlacement(); // TRỪ Ở TOWER
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

    // CHỈ KIỂM TRA, KHÔNG TRỪ
    public bool CanPlaceTower(Tower t)
    {
        if (t == null) return false;
        return _currentEnergy >= t.EnergyCost;
    }
}