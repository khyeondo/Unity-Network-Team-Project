using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationScript : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("attack");
        animator.ResetTrigger("attack_power");
        animator.ResetTrigger("combo_attack1");
        animator.ResetTrigger("combo_attack2");
        animator.ResetTrigger("attack_charge");

        if(stateInfo.IsName("knockback"))
        {
            animator.applyRootMotion = false;
        }
        if (stateInfo.IsName("run"))
        {
            animator.applyRootMotion = true;            
        }
        
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool isOnGround = animator.GetComponent<CharacterController>().isOnGround;

        if (isOnGround == false)
        {
            if ((stateInfo.IsName("knockback") || stateInfo.IsName("airborne")) &&
            stateInfo.normalizedTime > 0.3f)
                animator.speed = 0f;

            if (stateInfo.IsName("jump") && stateInfo.normalizedTime > 0.7f)
                animator.speed = 0f;
        }
        else
        {
            animator.speed = 1f;
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("knockback"))
        {
            
        }
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
