using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static PlayerStateHolder;

public class ConnectionController : NetworkBehaviour
{
    public static ConnectionController Instance { get; private set; }

    public event Action PlayerFinishedCleaningNetworkObjects;

    private GameObject networkManagerObject;
    private GameObject persistingPlayerData;

    private Dictionary<ulong, int> playerConnectionCodeMap = new Dictionary<ulong, int>();
    private int clientActivePlayerCode;
    private int serverActivePlayerCode;

    private const float MAX_ASSIGN_NEW_ACTIVE_CODE_TIMER = 9f;
    private const float UPDATE_PLAYER_ACTIVE_CODE_TIMER = 4f;
    private float assignNewActiveCodeTimer = 0f;
    private float updatePlayerActiveCodeTimer = 0f;

    private System.Random rng;

    bool allPlayersSpawned = false;

    private void Awake()
    {
        Instance = this;

        rng = new System.Random();
    }

    private void Start()
    {
        networkManagerObject = GameObject.Find("NetworkManager");
        persistingPlayerData = GameObject.Find("LobbyPersistingData");

        if(IsServer)
        {
            PlayerTransformHolder.Instance.PlayerTransformsFound += AssignPlayersToCodeMap;
        }
    }

    private void Update()
    {
        if(allPlayersSpawned) 
        {
            assignNewActiveCodeTimer += Time.deltaTime;

            if (assignNewActiveCodeTimer >= MAX_ASSIGN_NEW_ACTIVE_CODE_TIMER)
            {
                assignNewActiveCodeTimer = 0;

                CheckIfClientsDisconnected();
                CheckIfAnyClientsStillConnected();

                int newCode = rng.Next(1, 1001);
                serverActivePlayerCode = newCode;
                AssignNewActivePlayerCodeClientRpc(newCode);
            }
        }

        updatePlayerActiveCodeTimer += Time.deltaTime;

        if (updatePlayerActiveCodeTimer >= UPDATE_PLAYER_ACTIVE_CODE_TIMER)
        {
            updatePlayerActiveCodeTimer = 0;
            UpdatePlayerCurrentActivePlayerCodeServerRpc(NetworkManager.Singleton.LocalClientId, clientActivePlayerCode);
        }
    }

    private void AssignPlayersToCodeMap()
    {
        allPlayersSpawned = true;

        foreach (ulong clientId in playerConnectionCodeMap.Keys)
        {
            playerConnectionCodeMap.Add(clientId, 0);
            AssignNewActivePlayerCodeClientRpc(0);
        }
    }

    private void CheckIfClientsDisconnected()
    {
        foreach (ulong clientId in playerConnectionCodeMap.Keys)
        {
            if (playerConnectionCodeMap[clientId] != serverActivePlayerCode && PlayerStateHolder.Instance.IsPlayerAlive(clientId))
            {
                EndGameHandler.Instance.SurvivorDisconnected(clientId);
                NetworkManager.Singleton.DisconnectClient(clientId);
            }
        }
    }

    // NOTE: player being in disconnected state is different than checking if client is disconnected via the active code
    // State may not be 'disconnected' but client is disconnected (ie. dead)
    public void CheckIfAnyClientsStillConnected()
    {
        foreach (ulong clientId in playerConnectionCodeMap.Keys)
        {
            if (playerConnectionCodeMap[clientId] == serverActivePlayerCode && PlayerTransformHolder.Instance.IsPlayerSurvivorById(clientId))
            {
                Debug.Log("Not this one!");
                return;
            }        
        }
        Debug.Log("Found!");
        EndGameHandler.Instance.EndGameForKiller();
    }

    public void DisconnectServer()
    {
        Debug.Log("Disconnecting Server");
        CleanupPersistingObjects();
    }

    public void DisconnectClient(ulong clientId)
    {
        Debug.Log("Disconnecting Client");
        CleanupPersistingObjectsClientRpc(clientId);
    }

    public void CleanupPersistingObjects()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(networkManagerObject);
        Destroy(persistingPlayerData);
        PlayerFinishedCleaningNetworkObjects?.Invoke();
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdatePlayerCurrentActivePlayerCodeServerRpc(ulong clientId, int currentPlayerCode)
    {
        Debug.Log("IDDDD: " + clientId + " Player Code: " + currentPlayerCode);
        playerConnectionCodeMap[clientId] = currentPlayerCode;
    }

    [ClientRpc]
    private void AssignNewActivePlayerCodeClientRpc(int newCode)
    {
        clientActivePlayerCode = newCode;
    }

    [ClientRpc]
    public void CleanupPersistingObjectsClientRpc(ulong clientId)
    {
        if(clientId == NetworkManager.Singleton.LocalClientId) 
        {
            CleanupPersistingObjects();
        }
    }
}
