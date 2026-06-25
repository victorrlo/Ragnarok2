using System;
using System.Collections;
using UnityEngine;

public class DamageReaction : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;

    public bool IsReacting {get; private set;}
    public bool BlocksMovement => IsReacting;

    public event Action OnReactionStarted;
    public event Action OnReactionFinished;

    public void React()
    {
        StopAllCoroutines();
        StartCoroutine(ReactRoutine());
    }

    private IEnumerator ReactRoutine()
    {
        IsReacting = true;
        OnReactionStarted?.Invoke();

        yield return new WaitForSeconds(duration);

        IsReacting = false;
        OnReactionFinished?.Invoke();
    }
}
