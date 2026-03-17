using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface ISoundComponent
{
    public void PlaySound(SoundData data, Action onEndOfClip = null);
    public bool IsSoundPlaying(SoundData data);
    public void StopSound(SoundData data);
}
