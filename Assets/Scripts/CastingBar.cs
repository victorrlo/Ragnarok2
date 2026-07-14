using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class CastingBar : MonoBehaviour
{
    [SerializeField] private GameObject _castingBar;
    [SerializeField] private Image _castingBarFill;
    private CancellationTokenSource  _cancellationTokenSource;
    public System.Action<GameObject, Skill> OnCastingComplete;
    public GameObject CurrentCaster { get; private set; }
    private Vector3 _offSet = new Vector3(0, 2f, 0);
    private int _invocationId;

    private void Awake()
    {
        if (_castingBar == null && _castingBarFill != null)
            _castingBar = _castingBarFill.transform.parent.gameObject;
    }

    public async void Initialize(GameObject caster, Skill skill, System.Action<CastingBar> returnToPool)
    {
        CurrentCaster = caster;
        float duration = skill != null ? Mathf.Max(0f, skill.CastingTime) : 0f;
        bool hasCastTime = duration > 0f;

        if (_castingBarFill != null)
            _castingBarFill.fillAmount = 0f;

        if (_castingBar != null)
            _castingBar.SetActive(hasCastTime);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        int invocationId = ++_invocationId;

        await AnimateCastingBar(caster, skill, returnToPool, invocationId);
    }

    private async Awaitable AnimateCastingBar(GameObject caster, Skill skill, System.Action<CastingBar> returnToPool, int invocationId)
    {
        float elapsedTime = 0f;
        float duration = skill != null ? Mathf.Max(0f, skill.CastingTime) : 0f;
        bool completionRaised = false;
        gameObject.SetActive(true);

        try
        {
            while (elapsedTime < duration)
            {
                if (invocationId != _invocationId)
                    return;

                if (caster == null || _cancellationTokenSource.Token.IsCancellationRequested)
                {
                    CancelCast();
                    ReturnToPoolIfCurrent(returnToPool, invocationId);
                    return;
                }

                if (_castingBarFill != null)
                    _castingBarFill.fillAmount = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);

                transform.position = caster.transform.position + _offSet;

                if (!completionRaised && elapsedTime >= duration)
                {
                    completionRaised = true;
                    OnCastingComplete?.Invoke(caster, skill);
                }

                await Awaitable.NextFrameAsync(_cancellationTokenSource.Token);
                elapsedTime += Time.deltaTime;
            }

            if (invocationId != _invocationId)
                return;

            if (caster == null || _cancellationTokenSource.Token.IsCancellationRequested)
            {
                CancelCast();
                ReturnToPoolIfCurrent(returnToPool, invocationId);
                return;
            }

            if (_castingBarFill != null)
                _castingBarFill.fillAmount = 1f;

            if (!completionRaised)
                OnCastingComplete?.Invoke(caster, skill);

            await Awaitable.NextFrameAsync(_cancellationTokenSource.Token);
            ReturnToPoolIfCurrent(returnToPool, invocationId);
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private void CancelCast()
    {
        _cancellationTokenSource?.Cancel();
        if (_castingBarFill != null)
            _castingBarFill.fillAmount = 0f;
    }

    public void ClearCaster()
    {
        CurrentCaster = null;
    }

    private void ReturnToPoolIfCurrent(System.Action<CastingBar> returnToPool, int invocationId)
    {
        if (invocationId != _invocationId)
            return;

        returnToPool?.Invoke(this);
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
