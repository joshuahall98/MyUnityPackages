using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundData
{
    [SerializeField] AudioScriptableObject audioScriptableObject;
    public AudioScriptableObject AudioScriptableObject => audioScriptableObject;

    [SerializeField, Tooltip("If left empty, this will default to the callers transform")] Transform playLocation;
    public Transform PlayLocation => playLocation;

    [SerializeField] bool followTransform = false;
    public bool FollowTransform => followTransform;

    [SerializeField] float delay;
    public float Delay => delay;
}
