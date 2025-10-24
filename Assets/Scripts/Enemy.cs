using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int _maxHealth = 1;
    [SerializeField] protected float _moveSpeed = 1f;
    [SerializeField] protected SpriteRenderer _healthBar;
    [SerializeField] protected SpriteRenderer _healthFill;
    protected int _currentHealth;

    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }

    protected virtual void OnEnable()
    {
        _currentHealth = _maxHealth;
        _healthFill.size = _healthBar.size;
    }

    public virtual void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, _moveSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        _healthBar.transform.parent = null;

        Vector3 distance = TargetPosition - transform.position;
        if (Mathf.Abs(distance.y) > Mathf.Abs(distance.x))
        {
            transform.rotation = Quaternion.Euler(0f, 0f, distance.y > 0 ? 90f : -90f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, distance.x > 0 ? 0f : 180f);
        }
        _healthBar.transform.parent = transform;
    }

    public void SetCurrentPathIndex(int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }

    public virtual void ReduceEnemyHealth(int damage)
    {
        _currentHealth -= damage;
        AudioPlayer.Instance.PlaySFX("hit-enemy");

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            gameObject.SetActive(false);
            AudioPlayer.Instance.PlaySFX("enemy-die");
            LevelManager.Instance.AddEnergy(20);

            // GỌI KIỂM TRA THẮNG
            LevelManager.Instance.CheckWinCondition();
        }

        float healthPercentage = (float)_currentHealth / _maxHealth;
        _healthFill.size = new Vector2(healthPercentage * _healthBar.size.x, _healthBar.size.y);
    }
}