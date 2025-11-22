using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CastingBar : MonoBehaviour
{
    [SerializeField] private Image _castingBarFill;
    private CancellationTokenSource  _cancellationTokenSource;
    public System.Action<GameObject, Skill> OnCastingComplete;
    private Vector3 _offSet = new Vector3(0, 1.5f, 0);

    public void Initialize(GameObject caster, Skill skill, System.Action<CastingBar> returnToPool)
    {
        _castingBarFill.fillAmount = 0f;
        
        _cancellationTokenSource = new CancellationTokenSource();

        AnimateCastingBar(caster, skill, returnToPool).Forget();
    }

    private async UniTask AnimateCastingBar(GameObject caster, Skill skill, System.Action<CastingBar> returnToPool)
    {
        float elapsedTime = 0f;
        float duration = skill.CastingTime;
        gameObject.SetActive(true);

        while (elapsedTime < duration)
        {
            if (caster == null || _cancellationTokenSource.Token.IsCancellationRequested)
            {
                CancelCast();
                returnToPool?.Invoke(this);
                return;
            }

            _castingBarFill.fillAmount = elapsedTime / duration;
            transform.position = caster.transform.position + _offSet;
            await UniTask.Yield(_cancellationTokenSource.Token);
            elapsedTime += Time.deltaTime;
        }

        if (caster == null || _cancellationTokenSource.Token.IsCancellationRequested)
        {
            CancelCast();
            returnToPool?.Invoke(this);
            return;
        }

        _castingBarFill.fillAmount = 1f;
        OnCastingComplete?.Invoke(caster, skill);

        await UniTask.Yield(cancellationToken: _cancellationTokenSource.Token);
        returnToPool?.Invoke(this);
    }

    private void CancelCast()
    {
        _cancellationTokenSource?.Cancel();
        _castingBarFill.fillAmount = 0f;
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
