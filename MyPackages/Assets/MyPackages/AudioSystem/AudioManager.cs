using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Joshua 

namespace AudioSystem
{

    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        private class AudioReference
        {
            public AudioScriptableObject scriptableObjectReference;
            public GameObject gameObjectReference;
            public GameObject audioSource;
            public Coroutine clipLength;
            public UnityEvent endOfClip;
            public float volume;
        }

        public static AudioManager AudioManagerInstance;

        List<AudioReference> audioReferences = new List<AudioReference>();
        Queue<GameObject> audioPool = new Queue<GameObject>();

        [SerializeField] GameObject audioObjPrefab;

        [SerializeField] Transform audioPoolContainer;
        [SerializeField] Transform activeSounds;

        [SerializeField, Tooltip("This int controls the max number of one type of audio clip that can be played before the oldest audio clip is cancelled")] 
        int StackingAudioLimiter = 5;

        int activeAudioPriority = 0;
        float reductionAmount = 0.5f;

        private void Awake()
        {
            AudioManagerInstance = this;
        }

        #region -- PUBLIC METHODS --

        /// <summary>
        /// Play a sound.
        /// </summary>
        public void PlaySound(AudioScriptableObject sound, GameObject gameObject, UnityAction fireEventWhenSoundFinished = null)
        {
            if (sound == null)
            {
                Debug.LogError($"You are missing a sound SO from the gameobject {gameObject.name}");
                return;
            }

            if (sound.singleInstanceAudio)
            {
                foreach (AudioReference reference in audioReferences)
                {
                    if (reference.scriptableObjectReference == sound)
                    {
                        return;
                    }
                }
            }

            //priority system
            if (audioReferences.Count >= 32)
            {
                foreach(AudioReference reference in audioReferences)
                {
                    if(sound.audioPriority > reference.scriptableObjectReference.audioPriority)
                    {
                        StopSound(reference.scriptableObjectReference, reference.gameObjectReference);
                    }
                }
            }

            GameObject obj;

            //take obj from audio pool, if there are none create a new object.
            if (audioPool.Count > 0)
            {
                obj = audioPool.Dequeue();
            }
            else
            {
                obj = Instantiate(audioObjPrefab);
            }

            var chosenAudioClip = RandomUtility.ObjectPoolCalculator(sound.audioClips);

            AudioFloodPrevention(chosenAudioClip);

            var audioSource = obj.GetComponent<AudioSource>();

            PopulateTheAudioSource(sound, gameObject, obj, chosenAudioClip, audioSource);

            CreateAudioReference(sound, gameObject, obj, audioSource, chosenAudioClip);

            audioSource.Play();

            if(fireEventWhenSoundFinished != null)
            {
                FireEventWhenSoundFinished(sound, gameObject, fireEventWhenSoundFinished);
            }
            
            var fadeIn = sound.fadeIn;
            var fadeInDuration = sound.fadeInDuration;

            if (fadeIn)
            {
                StartCoroutine(FadeIn(audioSource, fadeInDuration, audioSource.volume));
            }
        }

        /// <summary>
        /// Stops an active sound.
        /// </summary>
        public void StopSound(AudioScriptableObject sound, GameObject gameObject)
        {

            foreach (var s in audioReferences)
            {
                if (s.scriptableObjectReference == sound && s.gameObjectReference == gameObject)
                {
                    s.audioSource.transform.SetParent(audioPoolContainer);
                    audioPool.Enqueue(s.audioSource);

                    var audioSource = s.audioSource.GetComponent<AudioSource>();

                    if (sound.fadeOut)
                    {
                        StartCoroutine(FadeOut(audioSource, sound.fadeOutDuration));
                    }
                    else
                    {
                        audioSource.Stop();
                    }

                    if (s.clipLength != null)
                    {
                        StopCoroutine(s.clipLength);
                    }

                    if (s.endOfClip != null)
                    {
                        s.endOfClip.Invoke();
                    }

                    audioReferences.Remove(s);

                    return;
                }
            }

            Debug.LogError("Sound: " + sound + " is not active.");
        }

        /// <summary>
        /// Stops all audio
        /// </summary>
        public void StopAllAudio()
        {
            foreach (AudioReference s in audioReferences)
            {
                StopSound(s.scriptableObjectReference, s.gameObjectReference);

                /*s.audioSource.transform.SetParent(audioPoolContainer);
                audioPool.Enqueue(s.audioSource);
                s.audioSource.GetComponent<AudioSource>().Stop();

                if (s.endOfClip != null)
                {
                    s.endOfClip.Invoke();
                }

                if (s.clipLength != null)
                {
                    StopCoroutine(s.clipLength);
                }

                audioReferences.Remove(s);*/
            }
        }


        /// <summary>
        /// Call this method to stop all audio that has been called from a gameobject
        /// </summary>
        /// <param name="gameObject"></param>
        public void StopAllAudioFromGameObject(GameObject gameObject)
        {
            foreach(AudioReference s in audioReferences)
            {
                if(s.gameObjectReference == gameObject)
                {
                    StopSound(s.scriptableObjectReference, s.gameObjectReference);
                }
            }
        }

        /// <summary>
        /// Call this method when you pause the game.
        /// </summary>
        public void PauseAllAudio()
        {
            foreach (AudioReference s in audioReferences)
            {
                if (!s.scriptableObjectReference.playWhilePaused)
                {
                    s.audioSource.GetComponent<AudioSource>().Pause();
                }
            }
        }

        /// <summary>
        ///  Call this method when you unpause the game.
        /// </summary>
        public void UnPauseAllAudio()
        {
            foreach (AudioReference s in audioReferences)
            {
                if (!s.scriptableObjectReference.playWhilePaused)
                {
                    s.audioSource.GetComponent<AudioSource>().Play();
                }
            }
        }

        /// <summary>
        /// Allows you to delay the activation of a sound.
        /// </summary>
        public void DelayedPlaySound(float delay, AudioScriptableObject sound, GameObject gameObject)
        {
            if (sound == null)
            {
                Debug.LogError($"You are missing a sound SO from the gameobject {gameObject.name}");
                return;
            }

            StartCoroutine(SoundDelay(delay, sound, gameObject));
        }

        /// <summary>
        /// Checks to see if the requested sound exists and is playing.
        /// </summary>
        public bool IsSoundPlaying(AudioScriptableObject sound, GameObject gameObject)
        {
            if (sound == null)
            {
                Debug.LogError($"You are missing a sound SO from the gameobject {gameObject.name}");
                return false;
            }

            foreach (var s in audioReferences)
            {
                if (s.scriptableObjectReference == sound && s.gameObjectReference == gameObject)
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
                Debug.LogError($"You are missing a sound SO from the gameobject {gameObject.name}");
                return;
            }

            var fadeDuration = 1f;

            if (systemIsActive && sound.audioPriority >= activeAudioPriority)
            {
                activeAudioPriority = sound.audioPriority;

                foreach (var s in audioReferences)
                {
                    if(s.scriptableObjectReference.audioPriority < activeAudioPriority)
                    {
                        var audioSource = s.audioSource.GetComponent<AudioSource>();
                        var volume = audioSource.volume;
                        StartCoroutine(FadeOut(audioSource, fadeDuration, (volume * reductionAmount)));
                    }
                }
            }
            else
            {
                foreach (var s in audioReferences)
                {
                    if (s.scriptableObjectReference.audioPriority < activeAudioPriority)
                    {
                        var audioSource = s.audioSource.GetComponent<AudioSource>();
                        StartCoroutine(FadeIn(s.audioSource.GetComponent<AudioSource>(), fadeDuration, s.volume, audioSource.volume));
                    }
                }

                activeAudioPriority = 0;
            }
        }

        #endregion

        #region -- PRIVATE METHODS --

        /// <summary>
        /// Fires an event when an audio source stops playing.
        /// </summary>
        private void FireEventWhenSoundFinished(AudioScriptableObject sound, GameObject gameObject, UnityAction reference)
        {
            //reverse the list so it grabs the most recent sound that was added for tracking
            var reversableList = audioReferences;
            reversableList.Reverse();

            foreach (var s in reversableList)
            {
                if (s.scriptableObjectReference == sound && s.gameObjectReference == gameObject)
                {
                    s.endOfClip.AddListener(reference);
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

            foreach(var s in audioReferences)
            {
                if (s.scriptableObjectReference == reference.scriptableObjectReference && s.gameObjectReference == reference.gameObjectReference)
                {
                    s.audioSource.transform.SetParent(audioPoolContainer);

                    if(s.endOfClip != null)
                    {
                        s.endOfClip.Invoke();
                    }

                    audioPool.Enqueue(s.audioSource);
                    audioReferences.Remove(s);

                    yield break;
                }
            }
        }

        private IEnumerator SoundDelay(float delay, AudioScriptableObject sound, GameObject gameObject)
        {
            yield return new WaitForSeconds(delay);

            PlaySound(sound, gameObject);
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
        /// Prevent audio flooding by deleting the oldest audio source when the limit is exceeded.
        /// </summary>
        private void AudioFloodPrevention(AudioList audioListVariable)
        {
            var stack = 0;

            foreach (AudioReference s in audioReferences)
            {
                if (s.audioSource == audioListVariable.audioClip)
                {
                    stack++;
                }
            }

            if (stack > StackingAudioLimiter)
            {
                foreach (AudioReference s in audioReferences)
                {
                    if (s.audioSource == audioListVariable.audioClip)
                    {
                        StopSound(s.scriptableObjectReference, s.gameObjectReference);

                        /*s.audioSource.transform.SetParent(audioPoolContainer);
                        audioPool.Enqueue(s.audioSource);

                        var audioSource = s.audioSource.GetComponent<AudioSource>();

                        audioSource.Stop();
                        
                        if (s.clipLength != null)
                        {
                            StopCoroutine(s.clipLength);
                        }
                        audioReferences.Remove(s);*/

                        return;
                    }
                }
            }
        }

        private void PopulateTheAudioSource(AudioScriptableObject sound, GameObject gameObject, GameObject obj, AudioList audioListVariable, AudioSource audioSource)
        {
            audioSource.clip = audioListVariable.audioClip;
            
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
            audioSource.rolloffMode = sound.rolloffMode;
            audioSource.minDistance = sound.minDistance;
            audioSource.maxDistance = sound.maxDistance;
            audioSource.dopplerLevel = sound.dopplerLevel;
            
            if (audioSource.spatialBlend == 0)
            {
                obj.transform.SetParent(activeSounds);
                obj.transform.position = activeSounds.position;
            }
            else
            {
                obj.transform.SetParent(gameObject.transform);
                obj.transform.position = gameObject.transform.position;
            }
        }

        private void CreateAudioReference(AudioScriptableObject sound, GameObject gameObject, GameObject obj, AudioSource audioSource, AudioList audioListVariable)
        {
            var createdObjReference = new AudioReference();

            createdObjReference.scriptableObjectReference = sound;
            createdObjReference.gameObjectReference = gameObject;
            createdObjReference.audioSource = obj;
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
}
