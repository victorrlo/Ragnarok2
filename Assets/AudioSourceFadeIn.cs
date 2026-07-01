using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceFadeIn : MonoBehaviour
{
    [SerializeField] private float _fadeInSeconds = 2f;

    private AudioSource _audioSource;
    private float _targetVolume;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _targetVolume = _audioSource.volume;
        _audioSource.volume = 0f;
        _audioSource.playOnAwake = false;
    }

    private void Start()
    {
        _audioSource.Play();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (_fadeInSeconds <= 0f)
        {
            _audioSource.volume = _targetVolume;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _fadeInSeconds)
        {
            elapsedTime += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, _targetVolume, elapsedTime / _fadeInSeconds);
            yield return null;
        }

        _audioSource.volume = _targetVolume;
    }
}
