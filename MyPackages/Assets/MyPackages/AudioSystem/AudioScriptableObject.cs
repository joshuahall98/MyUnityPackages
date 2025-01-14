using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioList
{
    public AudioClip audioClip;
    [Range(0f, 1f)]
    public float volume = 1;
    [Range(.1f, 3f)]
    public float pitch = 1;
}

[CreateAssetMenu(fileName = "Audio", menuName = "ScriptableObject/Audio")]
public class AudioScriptableObject : ScriptableObject
{
    public List<ObjectPool<AudioList>> audioClips;

    [Header ("Basic Controls")]
    public AudioMixerGroup audioMixerGroup;
    public bool loop = false;

    [Header ("Fade Controls")]
    public bool fadeIn = false;
    public float fadeInDuration = 1;
    public bool fadeOut = false;
    public float fadeOutDuration = 1;

    [Range(0f, 1f) , Header ("3D Controls")]
    public float spatialBlend = 0;
    [Range(0f, 5f)]
    public float dopplerLevel = 0;
    [Range(-1f, 1f)]
    public float pan = 0;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    public float minDistance = 1;
    public float maxDistance = 30;

    [Header("Advanced Controls")]
    [Range(1, 10), Tooltip("This determines the importance of the audio")]
    public int audioPriority = 5;
    [Tooltip("If this is set to true, only one instance of this audio can be active")]
    public bool singleInstanceAudio = false;
    [Tooltip("Allows the audio to play while the game is paused")]
    public bool playWhilePaused = false;

    [HideInInspector]
    public AudioSource source;
}
