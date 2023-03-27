using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public List<AttackSO> attacks;

    private int chainCounter;
    private bool queuedAttack;
    private float previousAttackChainEnd;
    
    // I personally do not use this variable since I am using a state machine that handles initializing a new attack and running it
    // but I have added it for this example. It should work fine like this too
    private bool attackInProgress;
    
    public float openQueueTime = 0.3f; // How far into the attack animation before we start looking for input [0.0 - 1.0]
    public float attackCooldown = 0.5f; // How long before we can start a new attack chain once the previous chain has reset
    public float resetChainTime = 1f; // How long it takes for the chain counter to reset to 0

    // Call this method only once to start a new attack chain
    public bool InitializeAttack()
    {
        // Check if an attack is not already in progress and attack is off cooldown
        if (!attackInProgress && Time.time - previousAttackChainEnd >= attackCooldown)
        {
            CancelInvoke("ResetChain");
            Attack();
            return true;
        }
        return false;
    }

    void Update()
    {
        if (attackInProgress)
        {
            LookForQueuedAttack();
            CheckIfAttackFinished();
        }
    }

    void Attack()
    {
        // Set attack animation and play it
        animator.runtimeAnimatorController = attacks[chainCounter].animatorOV;
        animator.Play("Attack", 0, 0);
    }
    
    void LookForQueuedAttack()
    {
        // Look for input once the animation has played X% (openQueueTime) of the current animation
        if (queuedAttack == false && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= openQueueTime)
        {
            if (Input.GetButtonDown("Attack"))
            {
                queuedAttack = true;
            }
        }
    }
    
    void CheckIfAttackFinished()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f // This checks if the animation is finished
            && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            chainCounter++;
            if (queuedAttack && chainCounter != attacks.Count) // If there is a queued attack and we are not at the end of the attack chain
            {
                Attack();
                queuedAttack = false;
            }
            else
            {
                EndAttack();
            }
        }
    }
    
    void EndAttack()
    {
        attackInProgress = false;
        queuedAttack = false;
        if (chainCounter == attacks.Count) // If we are at the end of the attack chain, reset it immediately rather than use Invoke()
        {
            ResetChain();
        }
        else
        {
            Invoke("ResetChain", resetChainTime);
        }
    }

    void ResetChain()
    {
        chainCounter = 0;
        previousAttackChainEnd = Time.time;
    }
}
