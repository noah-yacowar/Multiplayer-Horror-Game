using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GeneratorController : NetworkBehaviour
{
    public int genID;
    public bool isFullyRepaired = false;

    [SerializeField] private const float NEEDED_PROGRESS_FOR_COMPLETION = 15;
    private NetworkVariable<float> curProgress = new NetworkVariable<float>(0);
    private float lastProgressUpdate = 0;
    [SerializeField] float progressInterval = 5f;

    public event Action<int, float> UpdatedGenProgress;
    public event Action<ulong> PlayerIsRepairing;
    public event Action<ulong> PlayerStoppedRepairing;
    
    [SerializeField] private List<Transform> repairPositions = new List<Transform>();
    private Dictionary<int, ulong> availableRepairPositionsMap = new Dictionary<int, ulong>();
    const ulong AVAILABLE_POSITION = 999;
    private bool wasPlayerPreviouslyRepairing = false;
    private Transform generatorCenter;


    private NetworkList<ulong> activePlayers;
    float timer = 0;

    [SerializeField] KeyCode repairKey = KeyCode.E;

    private void Awake()
    {
        activePlayers = new NetworkList<ulong>();
        generatorCenter = transform.Find("GeneratorCenter");

        for (int pos = 0; pos < repairPositions.Count; pos++)
        {
            availableRepairPositionsMap[pos] = AVAILABLE_POSITION;
        }
    }

    private void Start()
    {
        PlayerStateHolder.Instance.OnPlayerDeath += RemovePlayerFromGen;
    }

    public bool IsGeneratorRepaired()
    {
        return isFullyRepaired;
    }

    private void RemovePlayerFromGen(ulong id)
    {
        if(activePlayers.Contains(id))
        {
            //Add logic to handle updating the available repair positions
            activePlayers.Remove(id);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (activePlayers.Count == 0 || timer < 3) return;

        timer = 0;

        if (IsClient)
        {
            IsPlayerRepairing();
        }

        if (curProgress.Value != lastProgressUpdate)
        {
            UpdateGenUI(genID, curProgress.Value);
            lastProgressUpdate = curProgress.Value;
        }
    }

    private void UpdateGenUI(int genID, float newProgress)
    {
        UpdatedGenProgress?.Invoke(genID, newProgress);
    }

    private void IsPlayerRepairing()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Transform playerTransform = PlayerTransformHolder.Instance.GetPlayerTransformById(clientId);

        if (activePlayers.Contains(clientId) && Input.GetKey(repairKey) && !isFullyRepaired)
        {
            AddToProgressServerRpc(progressInterval);

            playerTransform.GetComponent<SurvivorAnimationStateController>().PlayerIsRepairingGenerator();
            
            if(!wasPlayerPreviouslyRepairing)
            {
                for (int pos = 0; pos < repairPositions.Count; pos++)
                {
                    if (availableRepairPositionsMap[pos] == AVAILABLE_POSITION)
                    {
                        wasPlayerPreviouslyRepairing = true;
                        
                        playerTransform.position = repairPositions[pos].position;
                        UpdateGeneratorPositionServerRpc(pos, clientId);

                        // Calculate the direction to the target, ignoring changes in the Y-axis
                        Vector3 direction = generatorCenter.transform.position - playerTransform.position;
                        direction.y = 0; // Ignore changes in the Y-axis

                        if (direction != Vector3.zero)
                        {
                            // Calculate the angle between the current transform's forward vector and the direction to the target
                            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                            // Set the rotation to face the target on the X-Z plane
                            playerTransform.Find("Armature").rotation = Quaternion.Euler(-90, angle, 0);
                        }

                        break;
                    }
                }
            } 
        }
        else if (activePlayers.Contains(clientId))
        {
            playerTransform.GetComponent<SurvivorAnimationStateController>().PlayerStoppedRepairingGenerator();

            if (wasPlayerPreviouslyRepairing)
            {
                for (int pos = 0; pos < repairPositions.Count; pos++)
                {
                    if (availableRepairPositionsMap[pos] == clientId)
                    {
                        wasPlayerPreviouslyRepairing = false;
                        UpdateGeneratorPositionServerRpc(pos, AVAILABLE_POSITION);
                        break;
                    }
                }      
            }
        }
    }

    [ClientRpc]
    private void CompleteGenAndDisableClientRpc()
    {
        isFullyRepaired = true;
    }

    [ClientRpc]
    private void UpdateGeneratorPositionClientRpc(int position, ulong newId)
    {
        availableRepairPositionsMap[position] = newId;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGeneratorPositionServerRpc(int position, ulong newId)
    {
        UpdateGeneratorPositionClientRpc(position, newId);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;

        if (other.gameObject.CompareTag("Survivor"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                // Get the NetworkConnection of the owner client
                ulong ownerId = networkObject.OwnerClientId;

                // Get the client ID of the owner client
                AddNewPlayerToGenServerRpc(ownerId);
            }
            else
            {
                Debug.Log("Collided GameObject does not have a NetworkObject component.");
            }
        }
        else if(other.gameObject.CompareTag("Killer"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Get the NetworkConnection of the owner client
                ulong ownerId = networkObject.OwnerClientId;

                // Get the client ID of the owner client
                AddNewPlayerToGenServerRpc(ownerId);
            }
            else
            {
                Debug.Log("Collided GameObject does not have a NetworkObject component.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.CompareTag("Survivor"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                // Get the NetworkConnection of the owner client
                ulong ownerId = networkObject.OwnerClientId;

                // Get the client ID of the owner client
                RemovePlayerFromGenServerRpc(ownerId);            
            }
            else
            {
                Debug.Log("Collided GameObject does not have a NetworkObject component.");
            }
        }
        else if (other.gameObject.CompareTag("Killer"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Get the NetworkConnection of the owner client
                ulong ownerId = networkObject.OwnerClientId;

                // Get the client ID of the owner client
                RemovePlayerFromGenServerRpc(ownerId);
            }
            else
            {
                Debug.Log("Collided GameObject does not have a NetworkObject component.");
            }
        }
    }

    [ServerRpc]
    private void AddNewPlayerToGenServerRpc(ulong newClient)
    {
        // Check if the list already contains the value
        if (!activePlayers.Contains(newClient))
        {
            // Add the value to the list if it's not already present
            activePlayers.Add(newClient);
        }
    }

    [ServerRpc]
    private void RemovePlayerFromGenServerRpc(ulong newClient)
    {
        // Check if the list already contains the value
        if (activePlayers.Contains(newClient))
        {
            // Add the value to the list if it's not already present
            activePlayers.Remove(newClient);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void AddToProgressServerRpc(float addProgress)
    {
        curProgress.Value += addProgress;

        if(curProgress.Value > NEEDED_PROGRESS_FOR_COMPLETION) 
        {
            curProgress.Value = NEEDED_PROGRESS_FOR_COMPLETION;
            CompleteGenAndDisableClientRpc();
        }
    }
}
