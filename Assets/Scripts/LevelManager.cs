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
    [SerializeField] private Enemy[] _enemyPrefabs;   // CHỈ QUÁI THƯỜNG
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    [SerializeField] private Boss _bossPrefab;       // KÉO BOSS PREFAB VÀO ĐÂY
    private bool _hasBossSpawned = false;
    private bool _bossRequired = false;              // true nếu đang ở màn 5

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
    private int _enemyCounter;                       // còn bao nhiêu quái THƯỜNG phải spawn

    /* ---------- ENERGY ---------- */
    [SerializeField] private int _initialEnergy = 300;
    private int _currentEnergy;
    [SerializeField] private int _energyIncreasePerSecond = 10;
    [SerializeField] private int _energyFromEnemy = 20;
    private float _energyTimer = 0f;

    /* ============================================================= */
    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        _currentEnergy = _initialEnergy;
        SetEnergy(_currentEnergy);
        InstantiateAllTowerUI();

        _runningSpawnDelay = _spawnDelay;

        // Xác định có cần Boss không
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

        /* ---- SPAWN BOSS KHI ĐỦ ĐIỀU KIỆN ---- */
        if (!_hasBossSpawned && _bossRequired && _enemyCounter <= 0 && NoNormalEnemiesActive())
        {
            SpawnBoss();
            _hasBossSpawned = true;
        }

        /* ---- TOWERS ---- */
        foreach (Tower t in _spawnedTowers)
        {
            t.CheckNearestEnemy(_spawnedEnemies);
            t.SeekTarget();
            t.ShootTarget();
        }

        /* ---- ENEMIES (cả thường + Boss) ---- */
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

    private bool NoNormalEnemiesActive()
    {
        foreach (Enemy e in _spawnedEnemies)
            if (e.gameObject.activeSelf && !(e is Boss))
                return false;
        return true;
    }

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

        Debug.Log("BOSS SPAWNED!");
        // AudioPlayer.Instance?.PlaySFX("boss-appear");
    }

    /* ============================================================= */
    /***  CHỈ GỌI TỪ Enemy.ReduceEnemyHealth()  ***/
    public void CheckWinCondition()
    {
        // 1. Đã spawn hết quái thường
        // 2. Không còn Enemy nào đang active
        // 3. Nếu màn cần Boss → Boss phải đã được spawn
        bool noEnemies = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;

        if (_enemyCounter <= 0 && noEnemies &&
            (!_bossRequired || _hasBossSpawned))
        {
            SetGameOver(true);
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
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy(int v)
    {
        _enemyCounter = v;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }

    public void SetGameOver(bool win)
    {
        IsOver = true;
        _statusInfo.text = win ? "You Win!" : "You Lose!";
        _panel.SetActive(true);

        if (win)
        {
            int cur = SceneManager.GetActiveScene().buildIndex;
            int next = Mathf.Min(cur + 1, SceneManager.sceneCountInBuildSettings - 1);
            PlayerPrefs.SetInt("LastLevel", next);
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
                p.LockTowerPlacement();
        }
    }

    public void AddEnergy(int v) { _currentEnergy += v; SetEnergy(_currentEnergy); }
    public void SetEnergy(int v) { _currentEnergy = v; _energyInfo.text = $"Energy: {_currentEnergy}"; }

    public bool CanPlaceTower(Tower t)
    {
        if (_currentEnergy >= t.EnergyCost)
        {
            AddEnergy(-t.EnergyCost);
            return true;
        }
        Debug.Log("Not enough energy!");
        return false;
    }
}