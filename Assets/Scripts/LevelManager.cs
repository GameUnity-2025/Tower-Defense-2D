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
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;
    [SerializeField] private Tower[] _towerPrefabs;

    private List<Tower> _spawnedTowers = new List<Tower>();

    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private float _runningSpawnDelay;

    private List<Bullet> _spawnedBullets = new List<Bullet>();

    public bool IsOver { get; private set; }

    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;
    [SerializeField] private TMPro.TextMeshProUGUI _energyInfo; // Text mới để hiển thị năng lượng

    private int _currentLives;
    private int _enemyCounter;

    [SerializeField] private int _initialEnergy = 300; // Năng lượng ban đầu có thể chỉnh trong Inspector
    private int _currentEnergy; // Năng lượng hiện tại, khởi tạo từ _initialEnergy
    [SerializeField] private int _energyIncreasePerSecond = 10; // Tăng năng lượng theo thời gian
    [SerializeField] private int _energyFromEnemy = 20; // Năng lượng từ giết enemy
    private float _energyTimer = 0f;

    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        _currentEnergy = _initialEnergy; // Khởi tạo năng lượng hiện tại từ giá trị ban đầu
        SetEnergy(_currentEnergy); // Cập nhật năng lượng ban đầu
        InstantiateAllTowerUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (IsOver || Time.timeScale <= 0f) // Ngừng khi game pause hoặc kết thúc
        {
            return;
        }

        _runningSpawnDelay -= Time.deltaTime; // Sử dụng Time.deltaTime để đồng bộ với pause
        if (_runningSpawnDelay <= 0f)
        {
            SpawnEnemy();
            _runningSpawnDelay = _spawnDelay;
        }

        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }

        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }

            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives(1);
                    enemy.gameObject.SetActive(false);
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }

        // Tăng năng lượng theo thời gian
        _energyTimer += Time.unscaledDeltaTime; // Sử dụng unscaled để tăng ngay cả khi pause
        if (_energyTimer >= 1f)
        {
            AddEnergy(_energyIncreasePerSecond);
            _energyTimer = 0f;
        }
    }

    private void InstantiateAllTowerUI()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();
            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    public void RegisterSpawnedTower(Tower tower)
    {
        _spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        SetTotalEnemy(--_enemyCounter);
        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver(true);
            }
            return;
        }

        int randomIndex = Random.Range(0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();
        GameObject newEnemyObj = _spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
        if (!_spawnedEnemies.Contains(newEnemy))
        {
            _spawnedEnemies.Add(newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition(_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }

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
        GameObject newBulletObj = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;
        if (newBulletObj == null)
        {
            newBulletObj = Instantiate(prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(newBullet))
        {
            _spawnedBullets.Add(newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                if (Vector2.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        SetCurrentLives(_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver(false);
        }
    }

    public void SetCurrentLives(int currentLives)
    {
        _currentLives = Mathf.Max(currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;
        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive(true);
        if (isWin)
        {
            int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            int nextLevelIndex = Mathf.Min(currentLevelIndex + 1, SceneManager.sceneCountInBuildSettings - 1);
            PlayerPrefs.SetInt("LastLevel", nextLevelIndex);
            PlayerPrefs.Save();
        }
    }

    public void OnPlaceTowerButtonClicked()
    {
        TowerPlacement placement = FindObjectOfType<TowerPlacement>(); // Tìm TowerPlacement gần nhất
        if (placement != null)
        {
            Tower tower = placement.GetPlacedTower(); // Giả sử TowerPlacement có GetPlacedTower()
            if (tower != null)
            {
                Debug.Log($"Trying to place tower. Current Energy: {_currentEnergy}, Tower Energy Cost: {tower.EnergyCost}");
                if (CanPlaceTower(tower))
                {
                    Debug.Log("Enough energy, attempting to lock placement.");
                    placement.LockTowerPlacement();
                }
                else
                {
                    Debug.Log("Not enough energy, placement aborted.");
                }
            }
            else
            {
                Debug.Log("No tower selected for placement.");
            }
        }
        else
        {
            Debug.Log("No TowerPlacement found.");
        }
    }

    // Thêm phương thức kiểm tra và quản lý năng lượng
    public void AddEnergy(int amount)
    {
        _currentEnergy += amount;
        SetEnergy(_currentEnergy);
    }

    public void SetEnergy(int energy)
    {
        _currentEnergy = energy;
        _energyInfo.text = $"Energy: {_currentEnergy}";
    }

    public bool CanPlaceTower(Tower tower)
    {
        if (_currentEnergy >= tower.EnergyCost)
        {
            AddEnergy(-tower.EnergyCost); // Trừ năng lượng
            return true;
        }
        else
        {
            Debug.Log("Not enough energy to place tower!");
            return false;
        }
    }
}