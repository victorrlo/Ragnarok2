using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEmoteManager : MonoBehaviour
{
    [SerializeField] private GameObject _emote;
    #region Emotes
    [SerializeField] private Sprite _emoteFailSprite;
    [SerializeField] private RuntimeAnimatorController _emoteFailAnimator;
    #endregion
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Coroutine _emoteCoroutine;

    private void Awake()
    {
        _spriteRenderer = _emote.GetComponent<SpriteRenderer>();
        _animator = _emote.GetComponent<Animator>();
        _emote.SetActive(false);
    }

    private void Start()
    {
        ItemController.Instance.FailedUsingApple += ShowFailEmote;
    }

    private void LateUpdate()
    {
        if (GameObject.FindWithTag("Player") == null || Camera.main == null) return;
    }

    private void OnDestroy()
    {
        ItemController.Instance.FailedUsingApple -= ShowFailEmote;
    }

    private void ShowFailEmote(bool failed)
    {
        Debug.Log("Try showing failed emote!");
        if (failed)
        {
            Debug.Log("Showing failed emote!");
            _spriteRenderer.sprite = _emoteFailSprite;
            _animator.runtimeAnimatorController = _emoteFailAnimator;
            _emote.SetActive(true);
            _animator.Play("emote-fail_Clip", -1, 0f);

            if (_emoteCoroutine != null)
                StopCoroutine(_emoteCoroutine);
            _emoteCoroutine = StartCoroutine(DisableAfterAnimation());
        }
    }

    private IEnumerator DisableAfterAnimation()
    {
        yield return null;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        yield return new WaitForSeconds(animationLength);

        _emote.SetActive(false);
        _emoteCoroutine = null;
    }
}
