using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AdvancedAudioSystemManager : BaseAudioSystemManager
{
    [Serializable]
    public class FadeInFadeOutData
    {
        [SerializeField] bool fadeIn = false;
        public bool FadeIn => fadeIn;

        [SerializeField] float fadeInDuration = 1;
        public float FadeInDuration => fadeInDuration;

        [SerializeField] bool fadeOut = false;
        public bool FadeOut => fadeOut;

        [SerializeField] float fadeOutDuration = 1;
        public float FadeOutDuration => fadeOutDuration;
    }

    public new static AdvancedAudioSystemManager Instance
    {
        get => BaseAudioSystemManager.Instance as AdvancedAudioSystemManager;
    }

    /// <summary>
    /// Play a sound and fire an event when the sound finishes or is stopped.
    /// </summary>
    public void PlaySound(AudioScriptableObject sound, UniqueSoundID UUID, Vector3 location, Action onEndOfClip = null)
    {
        var activeSound = PlaySound(sound, UUID, location, false, null);

        if (onEndOfClip != null)
        {
            activeSound.AddListener(onEndOfClip);
        }

        if (sound.FadeControls.FadeIn)
        {
            if(TryGetNewestAudioReference(sound, UUID, out var audioReference))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                activeSound.AddListener(() => cancellationTokenSource.Cancel());
                
                StartCoroutine(FadeIn(audioReference.AudioSource, sound.FadeControls.FadeInDuration, audioReference.DefaultVolume, cancellationTokenSource.Token));
            }
        }
    }

    /// <summary>
    /// Play a sound and position the audio source at a transforms location, define if it follows and fire event when finished.
    /// </summary>
    public void PlaySound(AudioScriptableObject sound, UniqueSoundID UUID, Transform transformToTrack, bool followTransform = false, Action onEndOfClip = null)
    {
        var activeSound = PlaySound(sound, UUID, transformToTrack.position, followTransform, transformToTrack);

        if (onEndOfClip != null)
        {

            activeSound.AddListener(onEndOfClip);
        }

        if (sound.FadeControls.FadeIn)
        {
            if (TryGetNewestAudioReference(sound, UUID, out var audioReference))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                activeSound.AddListener(() => cancellationTokenSource.Cancel());

                StartCoroutine(FadeIn(audioReference.AudioSource, sound.FadeControls.FadeInDuration, audioReference.DefaultVolume, cancellationTokenSource.Token));
            }
        }
    }

    public new async void StopSound(AudioScriptableObject sound, UniqueSoundID UUID)
    {
        try
        {
            if (sound.FadeControls.FadeOut)
            {
                if (TryGetNewestAudioReference(sound, UUID, out var audioReference))
                {
                    // Fade out the sound asynchronously
                    await FadeOutAsync(audioReference.AudioSource, sound.FadeControls.FadeOutDuration);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during fade-out: {ex.Message}");
        }

        base.StopSound(sound, UUID);
    }

    /// <summary>
    /// Stop all audio
    /// </summary>
    public void StopAllAudio()
    {
        for (int i = audioReferences.Count - 1; i >= 0; i--)
        {
            var audioReference = audioReferences[i];

            if (audioReference == null) continue;

            StopSound(audioReference.ScriptableObjectReference, audioReference.UUID);
        }
    }

    /// <summary>
    /// Stop all audio that has been called from a UUID
    /// </summary>
    /// <param name="UUID"></param>
    public void StopAllAudioFromUniqueSoundID(UniqueSoundID UUID)
    {

        for (int i = audioReferences.Count - 1; i >= 0; i--)
        {
            var audioReference = audioReferences[i];

            if (audioReference == null || audioReference.UUID.soundID != UUID.soundID) continue;

            StopSound(audioReference.ScriptableObjectReference, audioReference.UUID);   
        }
    }

    /// <summary>
    /// Call this method when you pause the game.
    /// </summary>
    public void PauseAllAudio()
    {
        foreach (AudioReference audioReference in audioReferences)
        {
            //change the location of this call
            if (!audioReference.ScriptableObjectReference.PlayWhilePaused)
            {
                audioReference.AudioSourceObject.GetComponent<AudioSource>().Pause();
            }
        }
    }

    /// <summary>
    ///  Call this method when you unpause the game.
    /// </summary>
    public void UnPauseAllAudio()
    {
        foreach (AudioReference audioReference in audioReferences)
        {
            if (!audioReference.ScriptableObjectReference.PlayWhilePaused)
            {
                audioReference.AudioSourceObject.GetComponent<AudioSource>().Play();
            }
        }
    }

    /// <summary>
    /// Checks to see if the requested sound exists and is playing.
    /// </summary>
    public bool IsSoundPlaying(AudioScriptableObject sound, UniqueSoundID UUID)
    {
        if (sound == null)
        {
            UnityEngine.Debug.LogError($"You are missing a sound scriptable object");
            return false;
        }

        if (UUID == null)
        {
            UnityEngine.Debug.LogError($"You are missing the UUID");
            return false;
        }

        foreach (var audioReference in audioReferences)
        {
            if (audioReference.ScriptableObjectReference == sound && audioReference.UUID.soundID == UUID.soundID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Fade in audio source.
    /// </summary>
    private IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume, CancellationToken cancellationToken = default)
    {
        float currentTime = 0;

        while (currentTime < duration)
        {
            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                yield break; // Exit the coroutine early if the fade-in is canceled
            }

            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / duration);
            yield return null;
        }
    }

    /// <summary>
    /// Fade out audio source.
    /// </summary>
    public async Task FadeOutAsync(AudioSource audioSource, float duration)
    {
        var currentTime = 0f;
        var currentVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(currentVolume, 0, currentTime / duration);
            await Task.Yield(); // This allows the task to yield back to the Unity main thread
        }

        // Ensure the volume is set to the targetVolume once finished
        audioSource.volume = 0;
    }

    public IEnumerator AdjustVolume(AudioScriptableObject sound, UniqueSoundID UUID, float targetVolume, float duration = 0f)
    {
        if (!TryGetNewestAudioReference(sound, UUID, out var audioReference)) yield break;

        if(duration <= 0f)
        {
            audioReference.AudioSource.volume = targetVolume;
            yield break;
        }

        var currentTime = 0f;
        var currentVolume = audioReference.AudioSource.volume;

        while(currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioReference.AudioSource.volume = Mathf.Lerp(currentVolume, targetVolume, currentTime / duration);
            yield return null;
        }
    }

    public IEnumerator AdjustPitch(AudioScriptableObject sound, UniqueSoundID UUID, float targetPitch, float duration = 0f)
    {
        if (!TryGetNewestAudioReference(sound, UUID, out var audioReference)) yield break;

        if (duration <= 0f)
        {
            audioReference.AudioSource.pitch = targetPitch;
            yield break;
        }

        var currentTime = 0f;
        var currentPitch = audioReference.AudioSource.pitch;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioReference.AudioSource.pitch = Mathf.Lerp(currentPitch, targetPitch, currentTime / duration);
            yield return null;
        }
    }

}
