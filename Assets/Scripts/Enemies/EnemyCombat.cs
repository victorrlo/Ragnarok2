using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    // private FloatingDamage _floatingDamageSpawn;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);
    }

    private void Start()
    {
    }

    public void TakeDamage(int amount)
    {
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _enemyContext.StatsManager.TakeDamage(amount);

        if (_enemyContext.StatsManager.CurrentHP <= 0)
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
        _enemyContext.AI.CurrentState?.Exit();
        Destroy(gameObject);
    }
}
