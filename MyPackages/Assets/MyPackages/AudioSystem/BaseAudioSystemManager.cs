using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static AudioSourceCreator;

//I want to look at audio instancing per UUID call in the future, similar to something like gunshots, what is the best way to handle it if stop called early.

public class BaseAudioSystemManager : MonoBehaviour
{
    public class ActiveSound
    {
        private AudioReference reference;

        public ActiveSound(AudioReference audioReference)
        {
            reference = audioReference;
        }

        public void AddListener(Action listener)
        {
            reference.AddListener(listener);
        }

        public void RemoveListener(Action listener)
        {
            reference.RemoveListener(listener);
        }
    }

    public class AudioReference
    {
        public AudioScriptableObject ScriptableObjectReference { get; }
        public UniqueSoundID UUID { get; }
        public GameObject AudioSourceObject { get; }
        public AudioSource AudioSource => AudioSourceObject.GetComponent<AudioSource>();
        public AudioObjType Type { get; }
        public Coroutine ClipLength { get; }
        public float DefaultVolume { get; }

        private Action EndOfClip;

        public AudioReference(AudioScriptableObject scriptableObjectReference, UniqueSoundID UUID, GameObject audioSourceObject, AudioObjType type, Coroutine clipLength, float volume)
        {
            ScriptableObjectReference = scriptableObjectReference;
            this.UUID = UUID;
            AudioSourceObject = audioSourceObject;
            Type = type;
            ClipLength = clipLength;
            DefaultVolume = volume;
        }

        public void EndOfClipReached()
        {
            EndOfClip?.Invoke();
        }

        public void RemoveAllListeners()
        {
            EndOfClip = null;
        }

        public void AddListener(Action listener)
        {
            EndOfClip += listener;
        }

        public void RemoveListener(Action listener)
        {
            EndOfClip -= listener;
        }
    }

    public static BaseAudioSystemManager Instance { get; protected set; }

    protected List<AudioReference> audioReferences = new List<AudioReference>();

    Transform audioPoolContainer;
    Transform activeSounds;

    protected virtual void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioPoolContainer = new GameObject("AudioPoolContainer").transform;
        audioPoolContainer.SetParent(this.transform, false);

        activeSounds = new GameObject("ActiveSounds").transform;
        activeSounds.SetParent(this.transform, false);
    }

    /// <summary>
    /// Base play sound call
    /// </summary>
    protected ActiveSound PlaySound(AudioScriptableObject sound, UniqueSoundID UUID, Vector3 location, bool followTransform = false, Transform transformLocation = null)
    {
        if (sound == null)
        {
            Debug.LogError($"You are missing a sound scriptable object");
            return null;
        }

        var chosenAudioVariant = RandomUtility.ObjectPoolCalculator(sound.AudioClips);

        var audioData = new AudioSourceData(sound.AudioMixerGroup, sound.PitchShift, sound.MinPitchShift, sound.MaxPitchShift, sound.Loop, sound.Pan, sound.SpatialBlend, sound.DopplerLevel, sound.MinDistance, sound.MaxDistance, sound.VolumeRollOffMode, sound.VolumeRollOffCurve);

        var type = AudioObjType.STATIC;

        if (followTransform)
        {
            type = AudioObjType.FOLLOW;
        }

        var (obj, audioSource) = GenerateAudioSource(audioData, chosenAudioVariant, location, type, transformLocation);

        obj.name = sound.name;

        obj.transform.SetParent(activeSounds);

        var audioReference = CreateAudioReference(sound, UUID, obj, audioSource, chosenAudioVariant, type);

        // consider a separate implementation for PlayScheduled, for more insync audio calls
        //Play functions well for one shot audio
        audioSource.Play();

        return new ActiveSound(audioReference);
    }

    /// <summary>
    /// Base stop sound call.
    /// </summary>
    protected void StopSound(AudioScriptableObject sound, UniqueSoundID UUID)
    {
        if (sound == null)
        {
            UnityEngine.Debug.LogError($"You are missing a sound scriptable object");
            return;
        }

        if (UUID == null)
        {
            UnityEngine.Debug.LogError($"You are missing the UUID");
            return;
        }

        //make sure if a call to stop a sound occurs, it stops all instances of that sound for the UUID
        for (int i = audioReferences.Count - 1; i >= 0; i--)
        {
            var audioReference = audioReferences[i];

            if (audioReference.UUID.soundID == UUID.soundID && audioReference.ScriptableObjectReference == sound)
            {
                audioReference.AudioSourceObject.transform.SetParent(audioPoolContainer);

                ClearAudioSource(audioReference.Type, audioReference.AudioSourceObject);

                if (audioReference.ClipLength != null)
                {
                    StopCoroutine(audioReference.ClipLength);
                }

                audioReference.EndOfClipReached();
                audioReference.RemoveAllListeners();

                audioReferences.Remove(audioReference); 
            }
        }
    }

    private AudioReference CreateAudioReference(AudioScriptableObject sound, UniqueSoundID UUID, GameObject obj, AudioSource audioSource, AudioVariant audioVariant, AudioObjType type)
    {
        Coroutine clipLength = null;

        if (!audioSource.loop)
        {
            var lengthCalculatingPitch = audioSource.clip.length / Math.Abs(audioVariant.pitch);

            clipLength = StartCoroutine(Countdown(lengthCalculatingPitch, sound, UUID));
        }

        var createdObjReference = new AudioReference(sound, UUID, obj, type, clipLength, audioVariant.volume);

        audioReferences.Add(createdObjReference);

        return createdObjReference;
    }

    /// <summary>
    /// This will turn any non-looping audio into a oneshot
    /// </summary>
    IEnumerator Countdown(float seconds, AudioScriptableObject sound, UniqueSoundID UUID)
    {
        yield return new WaitForSeconds(seconds);

        if (!TryGetOldestAudioReference(sound, UUID, out var audioReference))
        {
            yield break;
        }

        audioReference.AudioSourceObject.transform.SetParent(audioPoolContainer);

        audioReference.EndOfClipReached();
        audioReference.RemoveAllListeners();

        ClearAudioSource(audioReference.Type, audioReference.AudioSourceObject);

        audioReferences.Remove(audioReference);
    }

    protected bool TryGetOldestAudioReference(AudioScriptableObject sound, UniqueSoundID UUID, out AudioReference audioReference)
    {
        foreach(var reference in audioReferences)
        {
            if (reference.UUID == UUID && reference.ScriptableObjectReference == sound)
            {
                audioReference = reference;
                return true;
            }
        }

        audioReference = null;
        return false;
    }

    protected bool TryGetNewestAudioReference(AudioScriptableObject sound, UniqueSoundID UUID, out AudioReference audioReference)
    {
        for (int i = audioReferences.Count - 1; i >= 0; i--)
        {
            var reference = audioReferences[i];

            if (reference.UUID == UUID && reference.ScriptableObjectReference == sound)
            {
                audioReference = reference;
                return true;
            }
        }

        audioReference = null;
        return false;
    }
}
