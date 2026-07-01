using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private CanvasGroup _canvasGroup;
    private Vector3 _startPos;
    private Vector3 _offSet = new Vector3(0, 1f, 0);
    private Vector3 _randomOffset;
    private float _timer;
    private float _lifeTime;
    private float _fadeTime;
    private float _arcHeight;
    private System.Action<FloatingDamageText> OnReturnToPool;

    public void Initialize(int amount, Color color, float lifeTime, float fadeTime, float arcHeight, 
        System.Action<FloatingDamageText> returnToPool)
    {
        Initialize(amount.ToString(), color, lifeTime, fadeTime, arcHeight, returnToPool);
    }

    public void Initialize(string text, Color color, float lifeTime, float fadeTime, float arcHeight,
        System.Action<FloatingDamageText> returnToPool)
    {
        _text.text = text;
        _text.color = color;
        _lifeTime = lifeTime;
        _startPos = transform.position + _offSet;
        _randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f);
        _timer = 0f;
        _fadeTime = fadeTime;
        _arcHeight = arcHeight;
        OnReturnToPool = returnToPool;

        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);

        StartCoroutine(AnimateText());
    }

    public void SetAmount(int amount)
    {
        _text.text = amount.ToString();
    }

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private IEnumerator AnimateText()
    {
        while (_timer < _lifeTime)
        {
            _timer += Time.deltaTime;
            float t = _timer/_lifeTime;

            Vector3 pos = Vector3.Lerp(_startPos, _startPos + _randomOffset, t);
            pos.y += Mathf.Sin(t*Mathf.PI) * _arcHeight;
            transform.position = pos;

            if (_timer > _lifeTime - _fadeTime)
            {
                float fadeTime = 1f - ((_timer - (_lifeTime - _fadeTime)) / _fadeTime);
                _canvasGroup.alpha = fadeTime;
            }
            
            yield return null;
        }

        OnReturnToPool?.Invoke(this);
    }
}
