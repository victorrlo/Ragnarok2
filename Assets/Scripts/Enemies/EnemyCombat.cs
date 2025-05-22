using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private EnemyMovement _enemyAi;
    [SerializeField] private int _maxHealth = 30;
    private int _currentHealth;
    private FloatingDamage _floatingDamageSpawn;

    private void Awake()
    {
        _floatingDamageSpawn = GetComponent<FloatingDamage>();
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _enemyAi.OnDamagedByPlayer();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Vector3Int cell = GridManager.Instance.WorldToCell(transform.position);
        // GridOccupancyManager.Instance.UnregisterOccupant(cell, gameObject);
        Destroy(gameObject);
    }
}
