using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingHealText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private CanvasGroup _canvasGroup;
    private Vector3 _startPos;
    private Vector3 _offSet = new Vector3(0, 1f, 0);
    private float _timer;
    private float _lifeTime;
    private float _fadeTime;
    private System.Action<FloatingHealText> OnReturnToPool;

    public void Initialize(float amount, float lifeTime, float fadeTime, 
        System.Action<FloatingHealText> returnToPool, Color color)
    {
        int roundedAmount = (int)Math.Round(amount);
        _text.text = roundedAmount.ToString();
        _text.color = color;
        _lifeTime = lifeTime;
        _startPos = transform.position + _offSet;
        _timer = 0f;
        _fadeTime = fadeTime;
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
        while (_timer < _lifeTime)
        {
            _timer += Time.deltaTime;
            float t = _timer/_lifeTime;

            Vector3 pos = Vector3.Lerp(_startPos, _startPos + _offSet, t);
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
