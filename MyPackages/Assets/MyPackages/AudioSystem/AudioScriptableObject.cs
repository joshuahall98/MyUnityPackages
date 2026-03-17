using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

[Serializable]
public class AudioVariant
{
    public AudioClip audioClip;
    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
}

[Serializable]
public class FadeControls
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

[CreateAssetMenu(fileName = "Audio", menuName = "ScriptableObject/Audio")]
public class AudioScriptableObject : ScriptableObject
{
    [SerializeField] List<ObjectPool<AudioVariant>> audioClips;
    public List<ObjectPool<AudioVariant>> AudioClips => audioClips;

    [SerializeField, HideInInspector] private int previousClipCount = 0;
    //[SerializeField, HideInInspector] bool defaultValuesApplied = false;

    [Header ("Basic Controls")]
    [SerializeField] AudioMixerGroup audioMixerGroup;
    public AudioMixerGroup AudioMixerGroup => audioMixerGroup;
    [SerializeField] bool loop = false;
    public bool Loop => loop;
    

    [Header("Advanced Controls")]
    /*[Range(1, 10), Tooltip("This determines the importance of the audio")]
    public int audioPriority = 5;
    [Tooltip("If this is set to true, only one instance of this audio can be active")]
    public bool singleInstanceAudio = false;
    public bool logStackTrace;*/
    [SerializeField, Tooltip("Allows the audio to play while the game is paused")] bool playWhilePaused = false;
    public bool PlayWhilePaused => playWhilePaused;

    [SerializeField] bool pitchShift;
    public bool PitchShift => pitchShift;
    [SerializeField, CustomRange(-3, 3), ShowIf("pitchShift")] float minPitchShift = 0.9f;
    public float MinPitchShift => minPitchShift;
    [SerializeField, CustomRange(-3, 3), ShowIf("pitchShift")] float maxPitchShift = 1.1f;
    public float MaxPitchShift => maxPitchShift;

    [SerializeField] FadeControls fadeControls;
    public FadeControls FadeControls => fadeControls;


    [Header("3D Controls")]
    [SerializeField, Range(0f, 1f)] float spatialBlend = 0;
    public float SpatialBlend => spatialBlend;
    [SerializeField, Range(0f, 5f)] float dopplerLevel = 0;
    public float DopplerLevel => dopplerLevel;
    [SerializeField, Range(-1f, 1f)] float pan = 0;
    public float Pan => pan;
    [HideMinimumDistanceValue(AudioRolloffMode.Custom)]
    [SerializeField] float minDistance = 1;
    public float MinDistance => minDistance;
    [SerializeField] float maxDistance = 30;
    public float MaxDistance => maxDistance;
    [SerializeField]
    AudioRolloffMode volumeRollOffMode = AudioRolloffMode.Linear;
    public AudioRolloffMode VolumeRollOffMode => volumeRollOffMode;
    [SerializeField] AnimationCurve volumeRollOffCurve;
    public AnimationCurve VolumeRollOffCurve => volumeRollOffCurve;



    //due to how unity handles generics, we need to intialize the values onvalidate
    private void OnValidate()
    {
        if (audioClips == null)
            return;

        // Detect newly added items
        if (audioClips.Count > previousClipCount)
        {
            for (int i = previousClipCount; i < audioClips.Count; i++)
            {
                var pool = audioClips[i];
                if (pool?.obj == null)
                    continue;

                pool.obj.volume = 1f;
                pool.obj.pitch = 1f;
                pool.weight = 1;
            }
        }

        // Update count for future validation checks
        previousClipCount = audioClips.Count;
    }
}
