using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel";
    private const string LastLevelKey = "LastLevel";
    private const int MaxConfiguredLevel = 20;

    private class LevelConfig
    {
        public int MaxLives;
        public int TotalEnemy;
        public float SpawnDelay;
    }

    private static readonly Dictionary<int, LevelConfig> LevelConfigs = new Dictionary<int, LevelConfig>
    {
        { 1, new LevelConfig { MaxLives = 3, TotalEnemy = 15, SpawnDelay = 5f } },
        { 2, new LevelConfig { MaxLives = 3, TotalEnemy = 18, SpawnDelay = 4.8f } },
        { 3, new LevelConfig { MaxLives = 2, TotalEnemy = 20, SpawnDelay = 4f } },
        { 4, new LevelConfig { MaxLives = 3, TotalEnemy = 22, SpawnDelay = 3.9f } },
        { 5, new LevelConfig { MaxLives = 3, TotalEnemy = 25, SpawnDelay = 3.7f } },
        { 6, new LevelConfig { MaxLives = 2, TotalEnemy = 28, SpawnDelay = 3.5f } },
        { 7, new LevelConfig { MaxLives = 2, TotalEnemy = 30, SpawnDelay = 3.3f } },
        { 8, new LevelConfig { MaxLives = 2, TotalEnemy = 33, SpawnDelay = 3.1f } },
        { 9, new LevelConfig { MaxLives = 2, TotalEnemy = 36, SpawnDelay = 2.9f } },
        { 10, new LevelConfig { MaxLives = 2, TotalEnemy = 40, SpawnDelay = 2.7f } },
        { 11, new LevelConfig { MaxLives = 2, TotalEnemy = 42, SpawnDelay = 2.5f } },
        { 12, new LevelConfig { MaxLives = 2, TotalEnemy = 45, SpawnDelay = 2.4f } },
        { 13, new LevelConfig { MaxLives = 2, TotalEnemy = 48, SpawnDelay = 2.3f } },
        { 14, new LevelConfig { MaxLives = 1, TotalEnemy = 50, SpawnDelay = 2.2f } },
        { 15, new LevelConfig { MaxLives = 1, TotalEnemy = 55, SpawnDelay = 2.1f } },
        { 16, new LevelConfig { MaxLives = 1, TotalEnemy = 60, SpawnDelay = 2.0f } },
        { 17, new LevelConfig { MaxLives = 1, TotalEnemy = 65, SpawnDelay = 1.9f } },
        { 18, new LevelConfig { MaxLives = 1, TotalEnemy = 70, SpawnDelay = 1.8f } },
        { 19, new LevelConfig { MaxLives = 1, TotalEnemy = 75, SpawnDelay = 1.7f } },
        { 20, new LevelConfig { MaxLives = 1, TotalEnemy = 80, SpawnDelay = 1.6f } }
    };

    // Fungsi Singleton
    private static LevelManager _instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager> ();
            }
            return _instance;
        }
    }

    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;
    [SerializeField] private Tower[] _towerPrefabs;

    private List<Tower> _spawnedTowers = new List<Tower> ();

    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    private List<Enemy> _spawnedEnemies = new List<Enemy> ();
    private float _runningSpawnDelay;

    private List<Bullet> _spawnedBullets = new List<Bullet> ();
    private readonly List<SpriteRenderer> _mapSpriteRenderers = new List<SpriteRenderer> ();
    private readonly Dictionary<SpriteRenderer, Color> _originalSpriteColors = new Dictionary<SpriteRenderer, Color> ();
    private Camera _mainCamera;
    private Color _defaultCameraColor;
    private int _currentLevelIndex = 1;

    public bool IsOver { get; private set; }

    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    private int _currentLives;
    private int _enemyCounter;

    private enum MapTheme
    {
        Default,
        Snow,
        Lava
    }

    private void Awake()
    {
        ConfigureLevelSettings ();
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _defaultCameraColor = _mainCamera.backgroundColor;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        CacheMapSprites ();
        ApplyMapTheme ();
        SetCurrentLives (_maxLives);
        SetTotalEnemy (_totalEnemy);
        InstantiateAllTowerUI ();
    }

    private void ConfigureLevelSettings ()
    {
        string sceneName = SceneManager.GetActiveScene ().name;
        if (!TryParseLevelIndex (sceneName, out _currentLevelIndex))
        {
            _currentLevelIndex = Mathf.Clamp (PlayerPrefs.GetInt (LastLevelKey, 1), 1, MaxConfiguredLevel);
        }

        if (LevelConfigs.TryGetValue (_currentLevelIndex, out LevelConfig config))
        {
            _maxLives = config.MaxLives;
            _totalEnemy = config.TotalEnemy;
            _spawnDelay = config.SpawnDelay;
        }

        int highestUnlocked = Mathf.Max (PlayerPrefs.GetInt (HighestUnlockedKey, 1), 1);
        PlayerPrefs.SetInt (HighestUnlockedKey, Mathf.Max (highestUnlocked, 1));
        PlayerPrefs.SetInt (LastLevelKey, _currentLevelIndex);
        PlayerPrefs.Save ();
    }

    private bool TryParseLevelIndex (string sceneName, out int levelIndex)
    {
        levelIndex = 0;
        const string prefix = "Level";
        if (!sceneName.StartsWith (prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string indexPart = sceneName.Substring (prefix.Length);
        if (int.TryParse (indexPart, out int parsed))
        {
            levelIndex = Mathf.Clamp (parsed, 1, MaxConfiguredLevel);
            return true;
        }

        return false;
    }

    private void CacheMapSprites ()
    {
        _mapSpriteRenderers.Clear ();
        _originalSpriteColors.Clear ();

        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer> ();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                continue;
            }

            string spriteName = spriteRenderer.sprite.name.ToLowerInvariant ();
            if (spriteName.Contains ("road") || spriteName.Contains ("tile") || spriteName.Contains ("plate") || spriteName.Contains ("placement") || spriteName.Contains ("ground"))
            {
                _mapSpriteRenderers.Add (spriteRenderer);
                if (!_originalSpriteColors.ContainsKey (spriteRenderer))
                {
                    _originalSpriteColors.Add (spriteRenderer, spriteRenderer.color);
                }
            }
        }
    }

    private MapTheme GetThemeForLevel (int levelIndex)
    {
        if (levelIndex >= 16)
        {
            return MapTheme.Lava;
        }

        if (levelIndex >= 11)
        {
            return MapTheme.Snow;
        }

        return MapTheme.Default;
    }

    private void ApplyMapTheme ()
    {
        MapTheme theme = GetThemeForLevel (_currentLevelIndex);
        foreach (SpriteRenderer spriteRenderer in _mapSpriteRenderers)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            if (!_originalSpriteColors.TryGetValue (spriteRenderer, out Color defaultColor))
            {
                defaultColor = Color.white;
            }

            if (theme == MapTheme.Default)
            {
                spriteRenderer.color = defaultColor;
                continue;
            }

            string spriteName = spriteRenderer.sprite != null ? spriteRenderer.sprite.name.ToLowerInvariant () : string.Empty;
            switch (theme)
            {
                case MapTheme.Snow:
                    spriteRenderer.color = spriteName.Contains ("road") ? new Color (0.85f, 0.92f, 1f, defaultColor.a) : new Color (0.92f, 0.97f, 1f, defaultColor.a);
                    break;
                case MapTheme.Lava:
                    spriteRenderer.color = spriteName.Contains ("road") ? new Color (1f, 0.58f, 0.3f, defaultColor.a) : new Color (0.95f, 0.35f, 0.18f, defaultColor.a);
                    break;
            }
        }

        ApplyThemeToCamera (theme);
    }

    private void ApplyThemeToCamera (MapTheme theme)
    {
        if (_mainCamera == null)
        {
            return;
        }

        switch (theme)
        {
            case MapTheme.Snow:
                _mainCamera.backgroundColor = new Color (0.35f, 0.5f, 0.7f, 1f);
                break;
            case MapTheme.Lava:
                _mainCamera.backgroundColor = new Color (0.15f, 0.03f, 0.02f, 1f);
                break;
            default:
                _mainCamera.backgroundColor = _defaultCameraColor;
                break;
        }
    }

    private void UnlockNextLevelIfNeeded ()
    {
        if (_currentLevelIndex >= MaxConfiguredLevel)
        {
            return;
        }

        int highestUnlocked = Mathf.Max (PlayerPrefs.GetInt (HighestUnlockedKey, 1), 1);
        int targetLevel = Mathf.Clamp (_currentLevelIndex + 1, 1, MaxConfiguredLevel);
        if (targetLevel > highestUnlocked)
        {
            PlayerPrefs.SetInt (HighestUnlockedKey, targetLevel);
            PlayerPrefs.Save ();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Jika menekan tombol R, fungsi restart akan terpanggil
        if (Input.GetKeyDown (KeyCode.R))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        }

        if (IsOver)
        {
            return;
        }

        // Counter untuk spawn enemy dalam jeda waktu yang ditentukan
        // Time.unscaledDeltaTime adalah deltaTime yang independent, tidak terpengaruh oleh apapun kecuali game object itu sendiri,
        // jadi bisa digunakan sebagai penghitung waktu
        _runningSpawnDelay -= Time.unscaledDeltaTime;
        if (_runningSpawnDelay <= 0f)
        {
            SpawnEnemy ();
            _runningSpawnDelay = _spawnDelay;
        }

        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy (_spawnedEnemies);
            tower.SeekTarget ();
            tower.ShootTarget ();
        }

        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }

            // Kenapa nilainya 0.1? Karena untuk lebih mentoleransi perbedaan posisi,
            // akan terlalu sulit jika perbedaan posisinya harus 0 atau sama persis
            if (Vector2.Distance (enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex (enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition (_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives (1);
                    enemy.gameObject.SetActive (false);
                }
            }

            else
            {
                enemy.MoveToTarget ();
            }
        }
    }

    // Menampilkan seluruh Tower yang tersedia pada UI Tower Selection
    private void InstantiateAllTowerUI ()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate (_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI> ();
            newTowerUI.SetTowerPrefab (tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    // Mendaftarkan Tower yang di-spawn agar bisa dikontrol oleh LevelManager
    public void RegisterSpawnedTower (Tower tower)
    {
        _spawnedTowers.Add (tower);
    }

    private void SpawnEnemy ()
    {
        SetTotalEnemy (--_enemyCounter);
        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemies.Find (e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver (true);
            }
            return;
        }

        int randomIndex = UnityEngine.Random.Range (0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString ();
        GameObject newEnemyObj = _spawnedEnemies.Find (e => !e.gameObject.activeSelf && e.name.Contains (enemyIndexString))?.gameObject;
        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate (_enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy> ();
        if (!_spawnedEnemies.Contains (newEnemy))
        {
            _spawnedEnemies.Add (newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition (_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex (1);
        newEnemy.gameObject.SetActive (true);
    }

    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos ()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine (_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

    public Bullet GetBulletFromPool (Bullet prefab)
    {
        GameObject newBulletObj = _spawnedBullets.Find (b => !b.gameObject.activeSelf && b.name.Contains (prefab.name))?.gameObject;
        if (newBulletObj == null)
        {
            newBulletObj = Instantiate (prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet> ();
        if (!_spawnedBullets.Contains (newBullet))
        {
            _spawnedBullets.Add (newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt (Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                if (Vector2.Distance (enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth (damage);
                }
            }
        }
    }

    public void ReduceLives (int value)
    {
        SetCurrentLives (_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver (false);
        }
    }

    public void SetCurrentLives (int currentLives)
    {
        // Mathf.Max fungsi nya adalah mengambil angka terbesar
        // sehingga _currentLives di sini tidak akan lebih kecil dari 0
        _currentLives = Mathf.Max (currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy (int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max (_enemyCounter, 0)}";
    }

    public void SetGameOver (bool isWin)
    {
        IsOver = true;
        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive (true);
        if (isWin)
        {
            UnlockNextLevelIfNeeded ();
        }
    }
}
