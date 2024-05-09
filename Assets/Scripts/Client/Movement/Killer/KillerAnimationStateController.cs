using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Collections.Unicode;

public class KillerAnimationStateController : NetworkBehaviour
{
    Animator animator;

    private const string IDLE_ANIMATION_NAME = "Idle";
    private const string WALK_ANIMATION_NAME = "Walk";
    private const string RUN_ANIMATION_NAME = "Run";
    private const string ATTACK_ANIMATION_NAME = "Attack";

    private const string IDLE_TRANSITION_NAME = "isIdling";
    private const string WALK_TRANSITION_NAME = "isWalking";
    private const string RUN_TRANSITION_NAME = "isRunning";
    private const string ATTACK_TRANSITION_NAME = "isAttacking";

    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if(IsOwner)
        {
            // Subscribe to the end attack animation event
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[3];
            AnimationEvent animationEvent = new AnimationEvent();
            animationEvent.time = clip.length * 0.73f; // Event at the end of animation
            animationEvent.functionName = "OnAttackAnimationComplete";
            clip.AddEvent(animationEvent);
        }
    }

    private void Update()
    {
        HandleAnimations();
    }

    public void PlayerIsAttacking()
    {
        SetPlayerToAttackingServerRpc();
    }

    public void PlayerStoppedAttacking()
    {
        SetPlayerToIdlingFromAttackingServerRpc();
    }

    void OnAttackAnimationComplete()
    {
        SetPlayerToIdlingFromAttackingServerRpc();
    }

    private void HandleAnimations()
    {
        if (IsOwner)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            HandleIdleTransitions(horizontalInput, verticalInput);
            HandleWalkTransitions(horizontalInput, verticalInput);
            HandleRunTransitions(horizontalInput, verticalInput);
        }
    }

    private void HandleIdleTransitions(float horizontalInput, float verticleInput)
    {
        if (!IsAnimatorInIdlingState()) return;

        bool isWalkingButtonPressed = horizontalInput != 0 || verticleInput != 0;

        if (isWalkingButtonPressed)
        {
            SetPlayerToWalkingFromIdlingServerRpc();
        }
    }

    private void HandleWalkTransitions(float horizontalInput, float verticleInput)
    {
        if (!IsAnimatorInWalkingState()) return;

        bool isRunningButtonPressed = Input.GetKey(runKey);
        bool isWalkingButtonPressed = horizontalInput != 0 || verticleInput != 0;

        if (!isWalkingButtonPressed)
        {
            SetPlayerToIdleingFromWalkingServerRpc();
        }
        else if (isRunningButtonPressed)
        {
            SetPlayerToRunningFromWalkingServerRpc();
        }
    }

    private void HandleRunTransitions(float horizontalInput, float verticleInput)
    {
        if (!IsAnimatorInRunningState()) return;

        bool isRunningButtonPressed = Input.GetKey(runKey);
        bool isWalkingButtonPressed = horizontalInput != 0 || verticleInput != 0;

        if (!isWalkingButtonPressed)
        {
            SetPlayerToIdlingFromRunningServerRpc();
        }
        else if (!isRunningButtonPressed || !isWalkingButtonPressed)
        {
            SetPlayerToWalkingFromRunningServerRpc();
        }
    }

    [ClientRpc]
    private void SetPlayerToWalkingFromIdlingClientRpc()
    {
        animator.SetBool(WALK_TRANSITION_NAME, true);
    }

    [ServerRpc]
    public void SetPlayerToWalkingFromIdlingServerRpc()
    {
        animator.SetBool(WALK_TRANSITION_NAME, true);
        SetPlayerToWalkingFromIdlingClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToIdleingFromWalkingClientRpc()
    {
        animator.SetBool(WALK_TRANSITION_NAME, false);
    }

    [ServerRpc]
    public void SetPlayerToIdleingFromWalkingServerRpc()
    {
        SetPlayerToIdleingFromWalkingClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToIdlingFromRunningClientRpc()
    {
        animator.SetBool(RUN_TRANSITION_NAME, false);
        animator.SetBool(WALK_TRANSITION_NAME, false);
    }

    [ServerRpc]
    public void SetPlayerToIdlingFromRunningServerRpc()
    {
        SetPlayerToIdlingFromRunningClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToRunningFromWalkingClientRpc()
    {
        animator.SetBool(RUN_TRANSITION_NAME, true);
    }

    [ServerRpc]
    public void SetPlayerToRunningFromWalkingServerRpc()
    {
        SetPlayerToRunningFromWalkingClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToWalkingFromRunningClientRpc()
    {
        animator.SetBool(RUN_TRANSITION_NAME, false);
    }

    [ServerRpc]
    public void SetPlayerToWalkingFromRunningServerRpc()
    {
        SetPlayerToWalkingFromRunningClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToAttackingClientRpc()
    {
        animator.SetBool(ATTACK_TRANSITION_NAME, true);
    }

    [ServerRpc]
    public void SetPlayerToAttackingServerRpc()
    {
        SetPlayerToAttackingClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToIdlingFromAttackingClientRpc()
    {
        animator.SetBool(ATTACK_TRANSITION_NAME, false);
    }

    [ServerRpc]
    public void SetPlayerToIdlingFromAttackingServerRpc()
    {
        SetPlayerToIdlingFromAttackingClientRpc();
    }

    public bool IsAnimatorInWalkingState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(WALK_ANIMATION_NAME);
    }

    public bool IsAnimatorInIdlingState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(IDLE_ANIMATION_NAME);
    }

    public bool IsAnimatorInRunningState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(RUN_ANIMATION_NAME);
    }

    public bool IsAnimatorInAttackingState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(ATTACK_ANIMATION_NAME);
    }
}