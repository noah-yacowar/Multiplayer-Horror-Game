using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Netcode.NetworkManager;

public class PlayerNetworkSpawner : NetworkBehaviour
{
    public static PlayerNetworkSpawner Instance { get; private set; }

    [SerializeField] GameObject playerPrefabA;
    [SerializeField] GameObject playerPrefabB;
    private ObservableList<ulong> connectedPlayerIDs;
    public event Action AllPlayersSpawned;
    private bool allPlayersSpawnedEventInvoked = false;
    public int playerPrefabsSpawnedAndSet = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        connectedPlayerIDs = new ObservableList<ulong>();

        if (IsServer)
        {
            connectedPlayerIDs.ItemAdded += OnClientLoaded;
        }

        UpdateClientLoadedStatusServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void Update()
    {
        if(!allPlayersSpawnedEventInvoked && playerPrefabsSpawnedAndSet == PersistingPlayerData.Instance.GetPlayerCount())
        {
            allPlayersSpawnedEventInvoked = true;
            NotifyAllClientsSpawnedClientRpc();
        }
    }

    public override void OnDestroy()
    {
        if(IsServer && connectedPlayerIDs.Count > 1) 
        {
            //No Longer update on connected client, as clients are already connected: NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }

    private void OnClientLoaded(ulong clientId)
    {
        // Spawn player for connected client
        SpawnPlayerForClient(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        GameObject playerPrefab = DeterminePlayerPrefab(clientId);
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.transform.position = new Vector3(0, 1, 0);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private GameObject DeterminePlayerPrefab(ulong clientId)
    {
        // Example logic: choose prefab based on clientId
        if(clientId == 0) return playerPrefabA;
        else return playerPrefabB;
    }

    [ClientRpc]
    private void NotifyAllClientsSpawnedClientRpc()
    {
        AllPlayersSpawned?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyServerClientHasLoadedPlayerPrefabServerRpc()
    {
        playerPrefabsSpawnedAndSet++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateClientLoadedStatusServerRpc(ulong newPlayerId) 
    {
        connectedPlayerIDs.Add(newPlayerId);
    }
}
