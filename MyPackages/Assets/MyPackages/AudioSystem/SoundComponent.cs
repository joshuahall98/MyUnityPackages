using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundComponent : MonoBehaviour, ISoundComponent
{

    UniqueSoundID UUID = new UniqueSoundID();

    public void PlaySound(SoundData data, Action onEndOfClip = null)
    {
        if (data.AudioScriptableObject == null) return; //fails quitely if empty

        StartCoroutine(DelayTimer(data, onEndOfClip));
    }

    private IEnumerator DelayTimer(SoundData data, Action onEndOfClip = null)
    {
        yield return new WaitForSeconds(data.Delay);

        var playLocation = data.PlayLocation;

        if(playLocation == null)
        {
            playLocation = transform;
        }

        AdvancedAudioSystemManager.Instance.PlaySound(data.AudioScriptableObject, UUID, playLocation, data.FollowTransform, onEndOfClip);
    }

    public bool IsSoundPlaying(SoundData data)
    {
        return AdvancedAudioSystemManager.Instance.IsSoundPlaying(data.AudioScriptableObject, UUID);
    }

    public void StopSound(SoundData data)
    {
        if (data.AudioScriptableObject == null) return; //fails quitely if empty

        AdvancedAudioSystemManager.Instance.StopSound(data.AudioScriptableObject, UUID);
    }

    public void AdjustVolume(SoundData data, float targetVolume, float duration = 0f)
    {
        if(data.AudioScriptableObject == null) return; //fails quitely if empty

        AdvancedAudioSystemManager.Instance.AdjustVolume(data.AudioScriptableObject, UUID, targetVolume, duration);
    }

    public void AdjustPitch(SoundData data, float targetPitch, float duration = 0f)
    {
        if (data.AudioScriptableObject == null) return; //fails quitely if empty

        AdvancedAudioSystemManager.Instance.AdjustPitch(data.AudioScriptableObject, UUID, targetPitch, duration);
    }
}
