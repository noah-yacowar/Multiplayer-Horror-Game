using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class HandlePlayerDeactivation : MonoBehaviour
{
    private NetworkObject networkObject;
    private SurvivorMovement survivorMovement;
    private SurvivorAnimationStateController survivorAnimationStateController;
    private SurvivorNetwork survivorNetwork;
    private SurvivorHealth survivorHealth;
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        survivorMovement = GetComponent<SurvivorMovement>();
        survivorAnimationStateController = GetComponent<SurvivorAnimationStateController>();
        survivorNetwork = GetComponent<SurvivorNetwork>();
        survivorHealth = GetComponent<SurvivorHealth>();
        playerRigidbody = GetComponent<Rigidbody>();

    }

    public void DisablePlayerPrefab()
    {
        Debug.Log("Disabling Player");
        networkObject.enabled = false;
        survivorMovement.enabled = false;
        survivorAnimationStateController.enabled = false;
        survivorNetwork.enabled = false;
        survivorHealth.enabled = false;
        playerRigidbody.isKinematic = true;
    }
}
