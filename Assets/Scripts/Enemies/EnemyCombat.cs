using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private Coroutine _currentBehaviorCoroutine;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);
    }

    private void OnEnable()
    {
        _enemyContext.EventBus.OnStartAttack.OnRaised += Attack;
    }

    private void OnDisable()
    {
        _enemyContext.EventBus.OnStartAttack.OnRaised -= Attack;
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

    private void Attack(EnemyStartAttackData data)
    {
        PlayerCombat player;
        data._target.TryGetComponent<PlayerCombat>(out player);

        if (player != null)
            if (_enemyContext != null)
                player.TakeDamage(_enemyContext.Stats.Attack);
    }

    // public void StartAttacking(EnemyStartAttackData data)
    // {
    //     var player = data._target;

    //     SwitchToBehavior(Attacking(player));
    // }

    // private IEnumerator Attacking(GameObject target)
    // {
    //     var player = target.GetComponent<PlayerCombat>();

    //     if (player != null)
    //     {
    //         while(true)
    //         {
    //             if (_enemyContext == null) yield break;

    //             Debug.Log("Player taking damage!");
    //             player.TakeDamage(_enemyContext.Stats.Attack);
    //             yield return new WaitForSeconds(_enemyContext.Stats.AttackSpeed);
    //         }
    //     }
    // }

    // private void SwitchToBehavior(IEnumerator newBehavior)
    // {
    //     if (_currentBehaviorCoroutine != null)
    //     {
    //         StopCoroutine(_currentBehaviorCoroutine);
    //     }

    //     _currentBehaviorCoroutine = StartCoroutine(newBehavior);
    // }

    private void Die()
    {
        _enemyContext.AI.CurrentState?.Exit();
        Destroy(gameObject);
    }
}
