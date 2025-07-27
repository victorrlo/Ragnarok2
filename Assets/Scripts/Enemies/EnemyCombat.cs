using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] EnemyEventBus _enemyEventBus;
    private int _currentHealth; // 1
    // private FloatingDamage _floatingDamageSpawn;
    private EnemyStats _monsterStats; // 2

    private void Awake()
    {
        if(_enemyEventBus== null)
            TryGetComponent<EnemyEventBus>(out _enemyEventBus);

        // _floatingDamageSpawn = GetComponent<FloatingDamage>();
        _monsterStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        _currentHealth = _monsterStats.MaxHP;
    }

    // public void TakeDamage(int amount)
    // {
    //     _currentHealth -= amount; // acho que dá para tirar isso daqui também 1
    //     FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
    //     _monsterStats.TakeDamage(amount); // usar evento igual abaixo 2

    //     _enemyAI.ChangeState(new AttackingState()); usarei eventos para evitar isso aqui 3
        

    //     if (_currentHealth <= 0)
    //     {
    //         Die();
    //     }
    // }

    public IEnumerator Attack(GameObject player)
    {
        player.GetComponent<PlayerCombat>().TakeDamage(1);
        yield return new WaitForSeconds(_monsterStats.StatsData.AttackSpeed);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
