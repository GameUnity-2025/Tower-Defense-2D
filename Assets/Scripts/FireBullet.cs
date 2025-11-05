using UnityEngine;

using System.Collections;



public class FireBullet : Bullet

{

    [Header("=== FIRE BULLET SETTINGS ===")]

    [SerializeField] private float _burnDamagePerSecond = 30f;

    [SerializeField] private float _burnDuration = 2f;

    [SerializeField] private float _maxTravelDistance = 5f;

    [SerializeField] private float _damageInterval = 0.1f;

    [SerializeField] private LayerMask _enemyLayer = -1;

    [SerializeField] private float _damageRadius = 0.5f; 



    private float _travelledDistance = 0f;

    private float _lastDamageTime = 0f;

    private Vector2 _direction;

    private int _debugDamageCount = 0;



    protected override void OnEnable()

    {

        base.OnEnable();

        _targetEnemy = null;

        _direction = transform.up;

        _travelledDistance = 0f;

        _lastDamageTime = 0f;

        _debugDamageCount = 0;



        Debug.Log($"[FireBullet] Spawned! Power: {BulletPower}, Speed: {BulletSpeed}, Direction: {_direction}");

    }



    private void Update()

    {

        if (!gameObject.activeSelf) return;



        float moveDistance = BulletSpeed * Time.deltaTime;

        transform.Translate(_direction * moveDistance, Space.World);

        _travelledDistance += moveDistance;



        if (Time.time - _lastDamageTime >= _damageInterval)

        {

            DamageEnemiesInArea();

            _lastDamageTime = Time.time;

        }



        if (_travelledDistance >= _maxTravelDistance)

        {

            Debug.Log($"[FireBullet] Destroyed after {_travelledDistance:F1}m");

            gameObject.SetActive(false);

        }

    }



    private void DamageEnemiesInArea()

    {

        // Vòng tròn va chạm AoE

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _damageRadius, _enemyLayer);

        Debug.Log($"[FireBullet] Checked {hits.Length} enemies at pos {transform.position}");



        foreach (Collider2D hit in hits)

        {

            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null)

            {

                enemy.ApplyBurnEffect(_burnDuration, _burnDamagePerSecond);

            }

        }

    }



    private void OnDrawGizmosSelected()

    {

        if (!gameObject.activeSelf) return;



        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, _damageRadius); // Vẽ vòng tròn này



        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, _direction * 2f);

    }



    private void OnDrawGizmos()

    {

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        Gizmos.DrawSphere(transform.position, _damageRadius); // Vẽ vòng tròn này

    }

}