using UnityEngine;
using System.Collections;
public class TitleMenuAudioControl : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 2.0f; // Duration of the fade in seconds
    [SerializeField] private float playDuration = 1.0f; // Play for 5 seconds before fading

    private float lastPlayedTime = 0f;
    [SerializeField] private float delay = 1f;
    private bool _isPlaying = false;

    private void Start()
    {
        // Initialize with current time so it doesn't play immediately
        PlaySoundWithFade();
    }

    private void Update()
    {
        if (Time.time - lastPlayedTime >= delay && !_isPlaying)
        {
            PlaySoundWithFade();
        }
    }

    private void PlaySoundWithFade()
    {
        _isPlaying = true;
        audioSource.volume = 1f;
        audioSource.Play();
        StartCoroutine(FadeOutAudioClip(audioSource, playDuration, fadeDuration));
    }

    private IEnumerator FadeOutAudioClip(AudioSource source, float delayBeforeFade, float fadeTime)
    {
        // Wait for the initial play duration
        yield return new WaitForSeconds(delayBeforeFade);
        
        // Then start the fade-out coroutine
        yield return StartCoroutine(FadeOut(source, fadeTime));
    }

    public IEnumerator FadeOut(AudioSource audioSource, float fadingTime)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadingTime)
        {
            elapsedTime += Time.deltaTime;
            // Linearly interpolate the volume from startVolume down to 0
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadingTime);
            yield return null; // Wait until the next frame
        }

        // Ensure volume is set to 0 and stop the audio source
        audioSource.volume = 0;
        audioSource.Stop();
        lastPlayedTime = Time.time; 
        _isPlaying = false;
    }
}
