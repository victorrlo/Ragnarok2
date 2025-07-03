using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private EnemyMovementOLD _enemyMovementOLD;
    // [SerializeField] private int _maxHealth = 30;
    private int _currentHealth;
    private FloatingDamage _floatingDamageSpawn;
    private EnemyStats _monsterStats;

    private void Awake()
    {
        _floatingDamageSpawn = GetComponent<FloatingDamage>();
        _monsterStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        _currentHealth = _monsterStats.MaxHP;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _enemyMovementOLD.OnDamagedByPlayer();
        _monsterStats.TakeDamage(amount);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public IEnumerator Attack(GameObject player)
    {
        yield return new WaitForSeconds(0f);
        player.GetComponent<PlayerCombat>().TakeDamage(1);
    }

    private void Die()
    {
        // Vector3Int cell = GridManager.Instance.WorldToCell(transform.position);
        // GridOccupancyManager.Instance.UnregisterOccupant(cell, gameObject);
        Destroy(gameObject);
    }
}
