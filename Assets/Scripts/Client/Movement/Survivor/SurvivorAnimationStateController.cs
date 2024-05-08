using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static Unity.Collections.Unicode;

public class SurvivorAnimationStateController : NetworkBehaviour
{
    Animator animator;

    private const string IDLE_ANIMATION_NAME = "Idle";
    private const string WALK_ANIMATION_NAME = "Walk";
    private const string RUN_ANIMATION_NAME = "Run";
    private const string PRE_REPAIR_ANIMATION_NAME = "Bending_To_Repair_Generator";
    private const string REPAIR_ANIMATION_NAME = "Repair_Generator";

    private const string IDLE_TRANSITION_NAME = "isIdling";
    private const string WALK_TRANSITION_NAME = "isWalking";
    private const string RUN_TRANSITION_NAME = "isRunning";
    private const string REPAIR_TRANSITION_NAME = "isRepairing";


    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleAnimations();
    }

    public void PlayerIsRepairingGenerator()
    {
         SetPlayerToRepairingServerRpc();
    }

    public void PlayerStoppedRepairingGenerator()
    {
        SetPlayerToIdlingFromRepairingServerRpc();
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
    private void SetPlayerToRepairingClientRpc()
    {
        animator.SetBool(REPAIR_TRANSITION_NAME, true);
    }

    [ServerRpc]
    public void SetPlayerToRepairingServerRpc()
    {
        SetPlayerToRepairingClientRpc();
    }

    [ClientRpc]
    private void SetPlayerToIdlingFromRepairingClientRpc()
    {
        animator.SetBool(REPAIR_TRANSITION_NAME, false);
    }
    

    [ServerRpc]
    public void SetPlayerToIdlingFromRepairingServerRpc()
    {
        SetPlayerToIdlingFromRepairingClientRpc();
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

    public bool IsAnimatorInRepairingState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(REPAIR_ANIMATION_NAME) || stateInfo.IsName(PRE_REPAIR_ANIMATION_NAME));
    }
}
