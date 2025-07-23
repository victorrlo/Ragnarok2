using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private EnemyDamageEvent _onDamagedEvent;
    private int _currentHealth; // 1
    // private FloatingDamage _floatingDamageSpawn;
    private EnemyStats _monsterStats; // 2

    private void Awake()
    {
        // _floatingDamageSpawn = GetComponent<FloatingDamage>();
        _monsterStats = GetComponent<EnemyStats>(); // 2
    }

    private void Start()
    {
        _currentHealth = _monsterStats.MaxHP; // 1
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount; // acho que dá para tirar isso daqui também 1
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _monsterStats.TakeDamage(amount); // usar evento igual abaixo 2

        // _enemyAI.ChangeState(new AttackingState()); usarei eventos para evitar isso aqui 3
        var data = new EnemyDamageEventData(gameObject, amount);
        _onDamagedEvent.Raise(data);

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
        Destroy(gameObject);
    }
}
