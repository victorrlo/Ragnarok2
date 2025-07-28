using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private int _currentHealth; // 1
    // private FloatingDamage _floatingDamageSpawn;
    private EnemyStats _monsterStats; // 2

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        // _floatingDamageSpawn = GetComponent<FloatingDamage>();
        _monsterStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        _currentHealth = _monsterStats.MaxHP;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount; // acho que dá para tirar isso daqui também 1
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _monsterStats.TakeDamage(amount); // usar evento igual abaixo 2

        // _enemyAI.ChangeState(new AttackingState()); usarei eventos para evitar isso aqui 3
        

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public IEnumerator Attack(GameObject player)
    {
        player.GetComponent<PlayerCombat>().TakeDamage(1);
        yield return new WaitForSeconds(_enemyContext.Stats.AttackSpeed);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
