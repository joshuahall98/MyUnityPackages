using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Joshua 


public class AudioManager : MonoBehaviour
{
    [Serializable]
    private class AudioReference
    {
        public AudioScriptableObject scriptableObjectReference;
        public UniqueSoundID UUID;
        public GameObject audioSourceObject;
        public AudioObjType type;
        public Coroutine clipLength;
        public UnityEvent endOfClip;
        public float volume;
    }

    enum AudioObjType { STATIC, FOLLOW }

    public static AudioManager AudioManagerInstance;

    List<AudioReference> audioReferences = new List<AudioReference>();
    Queue<GameObject> staticAudioPool = new Queue<GameObject>();
    Queue<GameObject> followAudioPool = new Queue<GameObject>();

    Transform audioPoolContainer;
    Transform activeSounds;

#if UNITY_EDITOR
    LoggingUtility loggingUtility = new LoggingUtility();
#endif

    //move this onto individual objects
    [SerializeField, Tooltip("This int controls the max number of one type of audio clip that can be played before the oldest audio clip is cancelled")] 
    int StackingAudioLimiter = 5;

    [SerializeField] bool logStackTrace;

    int activeAudioPriority = 0;
    float reductionAmount = 0.5f;

    private void Awake()
    {
        AudioManagerInstance = this;

        audioPoolContainer = new GameObject("AudioPoolContainer").transform;
        audioPoolContainer.SetParent(this.transform, false);

        activeSounds = new GameObject("ActiveSounds").transform;
        activeSounds.SetParent(this.transform, false);
    }

    #region -- PUBLIC METHODS --

    /// <summary>
    /// Play a sound and fire an event when the sound finishes or is stopped.
    /// </summary>
    public void PlaySound(AudioScriptableObject sound, UniqueSoundID UUID, Vector3 location, UnityAction fireEventWhenSoundFinished = null)
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        var soundPlayedSuccessfully = SoundPlayed(sound, UUID, location, AudioObjType.STATIC, fullStackTrace);

        if (!soundPlayedSuccessfully)
        {
            return;
        }

        if (fireEventWhenSoundFinished != null)
        {
            FireEventWhenSoundFinished(sound, UUID, fireEventWhenSoundFinished);
        }
    }

    /// <summary>
    /// Play a sound and position the audio source at a transforms location, define if it follows and fire event when finished.
    /// </summary>
    public void PlaySound(AudioScriptableObject sound, UniqueSoundID UUID, Transform transformLocation, bool followTransform = false, UnityAction fireEventWhenSoundFinished = null)
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        var type = AudioObjType.STATIC;

        if (followTransform)
        {
            type = AudioObjType.FOLLOW;
        }

        if (!SoundPlayed(sound, UUID, transformLocation.position, type, fullStackTrace, transformLocation))
        {
            return;
        }

        if (fireEventWhenSoundFinished != null)
        {
            FireEventWhenSoundFinished(sound, UUID, fireEventWhenSoundFinished);
        }
    }

    /// <summary>
    /// Stops an active sound.
    /// </summary>
    public void StopSound(AudioScriptableObject sound, UniqueSoundID UUID)
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();
            
        StoppedSound(sound, UUID, fullStackTrace);
    }

    /// <summary>
    /// Stops all audio
    /// </summary>
    public void StopAllAudio()
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        foreach (AudioReference audioReference in audioReferences)
        {
            StoppedSound(audioReference.scriptableObjectReference, audioReference.UUID, fullStackTrace);
        }
    }


    /// <summary>
    /// Call this method to stop all audio that has been called from a UUID
    /// </summary>
    /// <param name="UUID"></param>
    public void StopAllAudioFromUniqueSoundID(UniqueSoundID UUID)
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        foreach (AudioReference audioReference in audioReferences)
        {
            if(audioReference.UUID.soundID == UUID.soundID)
            {
                StoppedSound(audioReference.scriptableObjectReference, audioReference.UUID, fullStackTrace);
            }
        }
    }

    /// <summary>
    /// Call this method when you pause the game.
    /// </summary>
    public void PauseAllAudio()
    {
        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        foreach (AudioReference audioReference in audioReferences)
        {
            if (!audioReference.scriptableObjectReference.playWhilePaused)
            {
                audioReference.audioSourceObject.GetComponent<AudioSource>().Pause();     
            }
        }
#if UNITY_EDITOR
        if (logStackTrace)
        {
            loggingUtility.LogCleanedUpStackTrace($"STACK TRACE FOR PAUSING ALL AUDIO", fullStackTrace);
        }
#endif
    }

    /// <summary>
    ///  Call this method when you unpause the game.
    /// </summary>
    public void UnPauseAllAudio()
    {

        var fullStackTrace = StackTraceUtility.ExtractStackTrace();

        foreach (AudioReference audioReference in audioReferences)
        {
            if (!audioReference.scriptableObjectReference.playWhilePaused)
            {
                audioReference.audioSourceObject.GetComponent<AudioSource>().Play();    
            }
        }

#if UNITY_EDITOR
        if (logStackTrace)
        {
            loggingUtility.LogCleanedUpStackTrace($"STACK TRACE FOR UNPAUSING ALL AUDIO", fullStackTrace);
        }
#endif
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

        foreach (var audioReference in audioReferences)
        {
            if (audioReference.scriptableObjectReference == sound && audioReference.UUID.soundID == UUID.soundID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// The following method allows for dynamic control over the lowering and highering of all audios based on the priority of the SO parameter .
    /// </summary>
    public void DynamicVolumePrioritySystem(AudioScriptableObject sound, bool systemIsActive)
    {
        if (sound == null)
        {
            UnityEngine.Debug.LogError($"You are missing a sound scriptable object");
            return;
        }

        var fadeDuration = 1f;

        if (systemIsActive && sound.audioPriority >= activeAudioPriority)
        {
            activeAudioPriority = sound.audioPriority;

            foreach (var audioReference in audioReferences)
            {
                if(audioReference.scriptableObjectReference.audioPriority < activeAudioPriority)
                {
                    var audioSource = audioReference.audioSourceObject.GetComponent<AudioSource>();
                    var volume = audioSource.volume;
                    StartCoroutine(FadeOut(audioSource, fadeDuration, (volume * reductionAmount)));
                }
            }
        }
        else
        {
            foreach (var audioReference in audioReferences)
            {
                if (audioReference.scriptableObjectReference.audioPriority < activeAudioPriority)
                {
                    var audioSource = audioReference.audioSourceObject.GetComponent<AudioSource>();
                    StartCoroutine(FadeIn(audioReference.audioSourceObject.GetComponent<AudioSource>(), fadeDuration, audioReference.volume, audioSource.volume));
                }
            }

            activeAudioPriority = 0;
        }
    }

    #endregion

    #region -- PRIVATE METHODS --

    private void StoppedSound(AudioScriptableObject sound, UniqueSoundID UUID, string fullStackTrace)
    {
        foreach (var audioReference in audioReferences)
        {
            if (audioReference.scriptableObjectReference == sound && audioReference.UUID.soundID == UUID.soundID)
            {
                audioReference.audioSourceObject.transform.SetParent(audioPoolContainer);

                if (audioReference.type == AudioObjType.STATIC)
                {
                    staticAudioPool.Enqueue(audioReference.audioSourceObject);
                }
                else
                {
                    audioReference.audioSourceObject.GetComponent<AudioFollowTransform>().RemoveTransform();
                    followAudioPool.Enqueue(audioReference.audioSourceObject);
                }

                var audioSource = audioReference.audioSourceObject.GetComponent<AudioSource>();

                if (sound.fadeOut)
                {
                    StartCoroutine(FadeOut(audioSource, sound.fadeOutDuration));
                }
                else
                {
                    audioSource.Stop();

#if UNITY_EDITOR
                    if(logStackTrace)
                    {
                        loggingUtility.LogCleanedUpStackTrace($"STACKTRACE FOR STOPPED SOUND: {sound.name}", fullStackTrace);
                    }
#endif

                }

                if (audioReference.clipLength != null)
                {
                    StopCoroutine(audioReference.clipLength);
                }

                if (audioReference.endOfClip != null)
                {
                    audioReference.endOfClip.Invoke();
                }

                audioReferences.Remove(audioReference);

                return;
            }
        }

        UnityEngine.Debug.LogError("Sound: " + sound + " is not active.");
    }

    /// <summary>
    /// Fires an event when an audio source stops playing.
    /// </summary>
    private void FireEventWhenSoundFinished(AudioScriptableObject sound, UniqueSoundID UUID, UnityAction reference)
    {
        //reverse the list so it grabs the most recent sound that was added for tracking
        var reversableList = audioReferences;
        reversableList.Reverse();

        foreach (var audioReference in reversableList)
        {
            if (audioReference.scriptableObjectReference == sound && audioReference.UUID.soundID == UUID.soundID)
            {
                audioReference.endOfClip.AddListener(reference);
                return;
            }
        }
    }

    /// <summary>
    /// This will turn any non-looping audio into a oneshot
    /// </summary>
    IEnumerator Countdown(float seconds, AudioReference reference)
    {
        yield return new WaitForSeconds(seconds);

        foreach(var audioReference in audioReferences)
        {
            if (audioReference.scriptableObjectReference == reference.scriptableObjectReference && audioReference.UUID.soundID == reference.UUID.soundID)
            {
                audioReference.audioSourceObject.transform.SetParent(audioPoolContainer);

                if(audioReference.endOfClip != null)
                {
                    audioReference.endOfClip.Invoke();
                }

                if (audioReference.type == AudioObjType.STATIC)
                {
                    staticAudioPool.Enqueue(audioReference.audioSourceObject);
                }
                else
                {
                    audioReference.audioSourceObject.GetComponent<AudioFollowTransform>().RemoveTransform();
                    followAudioPool.Enqueue(audioReference.audioSourceObject);
                }

                audioReferences.Remove(audioReference);

                yield break;
            }
        }
    }

    /// <summary>
    /// Fade in audio source.
    /// </summary>
    private static IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume, float startingVolume = 0)
    {
        float currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startingVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    /// <summary>
    /// Fade out audio source.
    /// </summary>
    private static IEnumerator FadeOut(AudioSource audioSource, float duration, float targetVolume = 0)
    {
        var currentTime = 0f;
        var currentVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(currentVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        if(audioSource.volume <= 0)
        {
            audioSource.Stop();
        }
            
        yield break;
    }

    /// <summary>
    /// Base functionality required when calling a sound
    /// </summary>
    private bool SoundPlayed(AudioScriptableObject sound, UniqueSoundID UUID, Vector3 location, AudioObjType type, string fullStackTrace, Transform transformLocation = null)
    {
        if (sound == null)
        {
            UnityEngine.Debug.LogError($"You are missing a sound scriptable object");
            return false;
        }

        if (sound.singleInstanceAudio)
        {
            foreach (AudioReference reference in audioReferences)
            {
                if (reference.scriptableObjectReference == sound)
                {
                    return false;
                }
            }
        }

        //priority system
        if (audioReferences.Count >= 32)
        {
            foreach (AudioReference reference in audioReferences)
            {
                if (sound.audioPriority > reference.scriptableObjectReference.audioPriority)
                {
                    StopSound(reference.scriptableObjectReference, reference.UUID);
                }
            }
        }

        GameObject obj = AudioObjectType(location, type, transformLocation);

        var chosenAudioClip = RandomUtility.ObjectPoolCalculator(sound.audioClips);

        AudioFloodPrevention(chosenAudioClip);

        var audioSource = obj.GetComponent<AudioSource>();

        PopulateTheAudioSource(sound, UUID, obj, chosenAudioClip, audioSource);

        CreateAudioReference(sound, UUID, obj, audioSource, chosenAudioClip, type);

        audioSource.Play();

#if UNITY_EDITOR
        if (logStackTrace || sound.logStackTrace)
        {
            loggingUtility.LogCleanedUpStackTrace($"STACKTRACE FOR PLAYED SOUND: {sound.name}", fullStackTrace);
        }
#endif

        var fadeIn = sound.fadeIn;
        var fadeInDuration = sound.fadeInDuration;

        if (fadeIn)
        {
            StartCoroutine(FadeIn(audioSource, fadeInDuration, audioSource.volume));
        }

        return true;
    }


    /// <summary>
    /// Creates or re-uses an audio object.
    /// </summary>
    private GameObject AudioObjectType(Vector3 location, AudioObjType type, Transform transformLocation)
    {
        GameObject obj;

        if (type == AudioObjType.STATIC)
        {
            //take obj from audio pool, if there are none create a new object.
            if (staticAudioPool.Count > 0)
            {
                obj = staticAudioPool.Dequeue();
            }
            else
            {
                obj = new GameObject("StaticAudioObject");
                obj.AddComponent<AudioSource>();
            }

            obj.transform.position = location;
        }
        else
        {
            //take obj from audio pool, if there are none create a new object.
            if (followAudioPool.Count > 0)
            {
                obj = followAudioPool.Dequeue();

            }
            else
            {
                obj = new GameObject("FollowAudioObject");
                obj.AddComponent<AudioSource>();
                obj.AddComponent<AudioFollowTransform>();
            }

            obj.transform.position = location;
            obj.GetComponent<AudioFollowTransform>().AssignTransform(transformLocation);
        }

        obj.transform.SetParent(activeSounds);

        return obj;
    }

    /// <summary>
    /// Prevent audio flooding by deleting the oldest audio source when the limit is exceeded.
    /// </summary>
    private void AudioFloodPrevention(AudioList audioListVariable)
    {
        var stack = 0;

        foreach (AudioReference s in audioReferences)
        {
            if (s.audioSourceObject == audioListVariable.audioClip)
            {
                stack++;
            }
        }

        if (stack > StackingAudioLimiter)
        {
            foreach (AudioReference s in audioReferences)
            {
                if (s.audioSourceObject == audioListVariable.audioClip)
                {
                    StopSound(s.scriptableObjectReference, s.UUID);

                    return;
                }
            }
        }
    }

    private void PopulateTheAudioSource(AudioScriptableObject sound, UniqueSoundID UUID, GameObject obj, AudioList audioListVariable, AudioSource audioSource)
    {
        audioSource.clip = audioListVariable.audioClip;
                
        audioSource.playOnAwake = false;
            
        //check if a high priority sound has been played.
        if (activeAudioPriority > 0 && sound.audioPriority < activeAudioPriority)
        {
            audioSource.volume = audioListVariable.volume * reductionAmount;
        }
        else
        {
            audioSource.volume = audioListVariable.volume;
        }

        audioSource.pitch = audioListVariable.pitch;
        audioSource.outputAudioMixerGroup = sound.audioMixerGroup;
        audioSource.loop = sound.loop;
        audioSource.panStereo = sound.pan;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.dopplerLevel = sound.dopplerLevel;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        audioSource.rolloffMode = sound.volumeRollOffMode;

        if (sound.volumeRollOffMode == AudioRolloffMode.Custom)
        {  
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sound.volumeRollOffCurve);
        }
    }

    private void CreateAudioReference(AudioScriptableObject sound, UniqueSoundID UUID, GameObject obj, AudioSource audioSource, AudioList audioListVariable, AudioObjType type)
    {
        var createdObjReference = new AudioReference();

        createdObjReference.scriptableObjectReference = sound;
        createdObjReference.UUID = UUID;
        createdObjReference.type = type;
        createdObjReference.audioSourceObject = obj;
        createdObjReference.volume = audioListVariable.volume;

        Coroutine clipLength = null;

        if (!audioSource.loop)
        {
            var lengthCalculatingPitch = audioSource.clip.length / Math.Abs(audioListVariable.pitch);
            clipLength = StartCoroutine(Countdown(lengthCalculatingPitch, createdObjReference));
        }

        createdObjReference.clipLength = clipLength;

        createdObjReference.endOfClip = new UnityEvent();

        audioReferences.Add(createdObjReference);
    }

    #endregion
}

