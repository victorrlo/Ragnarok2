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
    private float _lifetime;
    private float _fadeTime;
    private float _arcHeight;
    private System.Action<FloatingDamageText> OnReturnToPool;

    public void Initialize(int amount, Color color, float lifetime, float fadeTime, float arcHeight, System.Action<FloatingDamageText> returnToPool)
    {
        _text.text = amount.ToString();
        _text.color = color;
        _lifetime = lifetime;
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

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private IEnumerator AnimateText()
    {
        while (_timer < _lifetime)
        {
            _timer += Time.deltaTime;
            float t = _timer/_lifetime;

            Vector3 pos = Vector3.Lerp(_startPos, _startPos + _randomOffset, t);
            pos.y += Mathf.Sin(t*Mathf.PI) * _arcHeight;
            transform.position = pos;

            if (_timer > _lifetime - _fadeTime)
            {
                float fadeTime = 1f - ((_timer - (_lifetime - _fadeTime)) / _fadeTime);
                _canvasGroup.alpha = fadeTime;
            }
            
            yield return null;
        }

        OnReturnToPool?.Invoke(this);
    }

    // private void Update()
    // {
    //     _timer += Time.deltaTime;
    //     float t = _timer / _lifetime;

    //     Vector3 offset = Vector3.Lerp(Vector3.zero, _randomOffset, t);
    //     offset.y += _arcHeight * Mathf.Sin(Mathf.PI * t); // arc shape
    //     transform.position = _startPos + offset;

    //     if (_timer < _lifetime - _fadeTime)
    //     {
    //         float fadeT = 1- (_timer - (_lifetime - _fadeTime)) / _fadeTime;
    //         _canvasGroup.alpha = Mathf.Clamp01(fadeT);
    //     }

    //     if (_timer >= _lifetime)
    //     {
    //         OnReturnToPool?.Invoke(this);
    //     }
    // }
}
