using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);
    }

    private void OnEnable()
    {
        _enemyContext.EventBus.OnStartAttack += StartAttacking;
    }

    private void OnDisable()
    {
        _enemyContext.EventBus.OnStartAttack -= StartAttacking;
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

    private void StartAttacking(StartAttackData data)
    {
        var target = data.target;

        _attackCoroutine = StartCoroutine(IsAttacking(target));
    }

    private IEnumerator IsAttacking(GameObject target)
    {
        while (true)
        {
            if (target == null) 
                yield break;
                
            var playerTargetPos = GridManager.Instance.WorldToCell(target.transform.position);
            var currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            
            if (!DistanceHelper.IsInAttackRange(currentGridPos, playerTargetPos, _enemyContext.Stats.AttackRange))
            {
                Chase(target);
                yield break;
            }

            Attack(target);
            yield return new WaitForSeconds(_enemyContext.Stats.AttackSpeed);
        }
    }

    private void Chase(GameObject target)
    {
        var data = new StartAttackData(this.gameObject, target);
        _enemyContext.EventBus.RaiseOnTargetMovedAway(data);
    }

    private void Attack(GameObject target)
    {
        target.TryGetComponent<PlayerCombat>(out var combat);

        if (target != null)
            if (_enemyContext != null)
            {
                Debug.Log("Causing damage to player");
                combat.TakeDamage(_enemyContext.Stats.Attack);
            }
                
    }

    private void Die()
    {
        _enemyContext.AI.CurrentState?.Exit();
        Destroy(gameObject);
    }
}
