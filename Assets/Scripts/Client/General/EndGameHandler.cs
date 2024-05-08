using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameHandler : NetworkBehaviour
{
    public static EndGameHandler Instance { get; private set; }

    public const string LOBBY_SCENE_NAME = "Lobby";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ConnectionController.Instance.PlayerFinishedCleaningNetworkObjects += LoadLobbyScene;
    }

    public void SurvivorDead(ulong clientId)
    {
        PlayerStateHolder.Instance.SetPlayerToDead(clientId);
        PlayerTransformHolder.Instance.RemovePlayer(clientId);
        EndGameForSurvivor(clientId);
    }

    public void SurvivorDisconnected(ulong clientId)
    {
        PlayerStateHolder.Instance.SetPlayerToDisconnected(clientId);
        PlayerTransformHolder.Instance.RemovePlayer(clientId);
        EndGameForSurvivor(clientId);
    }

    public void SurvivorEscaped(ulong clientId)
    {
        PlayerStateHolder.Instance.SetPlayerToEscaped(clientId);
        PlayerTransformHolder.Instance.RemovePlayer(clientId);
        EndGameForSurvivor(clientId);
    }

    public void LoadLobbyScene()
    {
        SceneManager.LoadScene(LOBBY_SCENE_NAME, LoadSceneMode.Single);
    }

    public void EndGameForKiller()
    {
        ConnectionController.Instance.DisconnectServer();
    }

    public void EndGameForSurvivor(ulong clientId)
    {
        ConnectionController.Instance.DisconnectClient(clientId);
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayerRequestingLeaveServerRpc(ulong clientId) 
    {
        SurvivorDisconnected(clientId);
    }
}
