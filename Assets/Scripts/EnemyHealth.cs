using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Vector3Int cell = NodeManager.Instance.WorldToCell(transform.position);
        GridOccupancyManager.Instance.UnregisterOccupant(cell);
        Destroy(gameObject);
    }
}
