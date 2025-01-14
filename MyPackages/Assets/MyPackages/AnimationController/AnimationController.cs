using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [System.Serializable]
    private class CurrentlyPlayingAnimations
    {
        public string animation;
        public int layer;
    }

    List<CurrentlyPlayingAnimations> currentlyPlayingAnimations = new List<CurrentlyPlayingAnimations>();

    [SerializeField] Animator animator;

    public void PlayAnimation(string animation, float transitionTime = 0f, int layer = 0)
    {
        
        //check if animation is already playing
        for(int i = 0; i < currentlyPlayingAnimations.Count; i++)
        {
            if (animation == currentlyPlayingAnimations[i].animation && layer == currentlyPlayingAnimations[i].layer)
            {
                return;
            }
        }

        //remove the previous layer reference
        for (int i = 0; i < currentlyPlayingAnimations.Count; i++)
        {
            if (layer == currentlyPlayingAnimations[i].layer)
            {
                currentlyPlayingAnimations.Remove(currentlyPlayingAnimations[i]);
            }
        }

        animator.CrossFade(animation.ToString(), transitionTime, layer);

        CurrentlyPlayingAnimations reference = new CurrentlyPlayingAnimations();

        reference.animation = animation; 
        reference.layer = layer;

        currentlyPlayingAnimations.Add(reference);
        
    }

    /*public void PlaySameAnimation(Animator animator, string animation, float transitionTime, int layer)
    {
        animator.CrossFade(animation, transitionTime, layer);

        playerAnimState = nextState;
    }

    public bool IsAnimationPlaying(PlayerAnimState stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsAnimationDone(PlayerAnimState stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/
}
