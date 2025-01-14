using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundComponent : MonoBehaviour, ISoundComponent
{

    UniqueSoundID UUID = new UniqueSoundID();

    public void PlaySound(AudioScriptableObject audioScriptableObject, Vector3 location, UnityAction fireEventWhenSoundFinished = null)
    {
        AudioManager.AudioManagerInstance.PlaySound(audioScriptableObject, UUID, location, fireEventWhenSoundFinished);
    }

    public void PlaySound(AudioScriptableObject audioScriptableObject, Transform transformLocation, bool followTransform = false, UnityAction fireEventWhenSoundFinished = null)
    {
        AudioManager.AudioManagerInstance.PlaySound(audioScriptableObject, UUID, transformLocation, followTransform, fireEventWhenSoundFinished);
    }

    public void StopSound(AudioScriptableObject audioScriptableObject)
    {
        AudioManager.AudioManagerInstance.StopSound(audioScriptableObject, UUID);
    }

    public bool IsSoundPlaying(AudioScriptableObject audioScriptableObject)
    {
        return AudioManager.AudioManagerInstance.IsSoundPlaying(audioScriptableObject, UUID);
    }

    public void DynamicVolumePrioritySystem(AudioScriptableObject audioScriptableObject, bool systemIsActive)
    {
        AudioManager.AudioManagerInstance.DynamicVolumePrioritySystem(audioScriptableObject, systemIsActive);
    }
}
