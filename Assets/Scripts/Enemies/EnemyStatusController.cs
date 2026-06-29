using System;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyStatusController : MonoBehaviour
{
    private EnemyAI _ai;
    private CancellationTokenSource _stunCts;

    public bool IsStunned { get; private set; }

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
    }

    public void Stun(float duration)
    {
        if (duration <= 0f)
            return;

        CancelStun();
        _stunCts = new CancellationTokenSource();
        _ = StunAsync(duration, _stunCts.Token);
    }

    private async Awaitable StunAsync(float duration, CancellationToken token)
    {
        IsStunned = true;
        _ai.SetStateChangeBlock(true);

        try
        {
            await Awaitable.WaitForSecondsAsync(duration, token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsStunned = false;
            _ai.SetStateChangeBlock(false);
        }
    }

    private void CancelStun()
    {
        if (_stunCts == null)
            return;

        _stunCts.Cancel();
        _stunCts.Dispose();
        _stunCts = null;
    }

    private void OnDestroy()
    {
        CancelStun();
    }
}
