using System;
using System.Threading;
using UnityEngine;

public class DamageReaction : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float reactionChance = 0.5f;
    private float _reactionSuppressCount = 0;

    private CancellationTokenSource _cts;
    private int _reactionVersion;

    public bool IsReacting { get; private set; }
    public bool BlocksMovement => IsReacting;

    public event Action OnReactionStarted;
    public event Action OnReactionFinished;

    public async Awaitable SuppressReactionFor(float seconds, CancellationToken token)
    {
        _reactionSuppressCount++;

        try
        {
            await Awaitable.WaitForSecondsAsync(seconds, token);
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            _reactionSuppressCount = Mathf.Max(0, _reactionSuppressCount - 1);
        }
    }

    public bool React()
    {
        if (_reactionSuppressCount > 0)
            return false;

        if (UnityEngine.Random.value > reactionChance)
            return false;

        CancelActiveReaction();

        int version = ++_reactionVersion;
        _cts = new CancellationTokenSource();

        _ = ReactAsync(_cts.Token, version);
        return true;
    }

    private async Awaitable ReactAsync(CancellationToken token, int version)
    {
        try
        {
            IsReacting = true;
            OnReactionStarted?.Invoke();

            await Awaitable.WaitForSecondsAsync(duration, token);

            if (version != _reactionVersion) return;

            IsReacting = false;
            OnReactionFinished?.Invoke();
        }
        catch (OperationCanceledException)
        {
            if (version != _reactionVersion) return;

            IsReacting = false;
            OnReactionFinished?.Invoke();
        }
    }

    private void CancelActiveReaction()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void OnDestroy()
    {
        CancelActiveReaction();
    }
}
