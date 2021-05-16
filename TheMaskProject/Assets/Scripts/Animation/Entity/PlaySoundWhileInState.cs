﻿using Entity.Manager;
using UnityEngine;

namespace Animation.Entity
{
    public class PlaySoundWhileInState : StateMachineBehaviour
    {
        [Header("Audio")] 
        public string soundName;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponent<BaseEntityManager>().entityAudioController.PlaySound(soundName);
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponent<BaseEntityManager>().entityAudioController.StopSound(soundName);
        }
    }
}