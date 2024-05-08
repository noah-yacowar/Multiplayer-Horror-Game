using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStateHolder : NetworkBehaviour
{
    public static PlayerStateHolder Instance { get; private set; }

    public event Action<ulong> OnPlayerDeath;
    public event Action<ulong> OnPlayerSurvived;

    public enum PlayerStatusCode
    {
        PLAYER_ALIVE_CODE = 0,
        PLAYER_DEAD_CODE = 1,
        PLAYER_ESCAPED_CODE = 2,
        PLAYER_DISCONNECTED_CODE = 3
    };

    private Dictionary<ulong, PlayerStatusCode> playerConnectionStatusMap = new Dictionary<ulong, PlayerStatusCode>();

    private void Start()
    {
        Instance = this;
        PlayerTransformHolder.Instance.PlayerTransformsFound += AssignPlayerConnectionStatus;
    }

    public bool AreAllSurvivorsOutOfGame()
    {
        foreach (PlayerStatusCode playerStatusCode in playerConnectionStatusMap.Values) 
        {
            if(playerStatusCode == PlayerStatusCode.PLAYER_ALIVE_CODE)
            {
                return false;
            }
        }

        return true;
    }

    private void AssignPlayerConnectionStatus()
    {
        foreach (Transform playerTransform in PlayerTransformHolder.Instance.GetAllPlayerTransforms())
        {
            ulong playerOwnerId = playerTransform.GetComponent<NetworkObject>().OwnerClientId;
            bool isPlayerSurvivor = PlayerTransformHolder.Instance.IsPlayerSurvivorById(playerOwnerId);

            if (isPlayerSurvivor)
            {
                playerConnectionStatusMap.Add(playerOwnerId, PlayerStatusCode.PLAYER_ALIVE_CODE);
            }
        }
    }

    public bool IsPlayerAlive(ulong id)
    {
        return playerConnectionStatusMap[id] == PlayerStatusCode.PLAYER_ALIVE_CODE;
    }

    public bool IsPlayerDead(ulong id)
    {
        return playerConnectionStatusMap[id] == PlayerStatusCode.PLAYER_DEAD_CODE;
    }

    public bool IsPlayerEscaped(ulong id)
    {
        return playerConnectionStatusMap[id] == PlayerStatusCode.PLAYER_ESCAPED_CODE;
    }

    public bool IsPlayerDisconnected(ulong id) 
    {
        return playerConnectionStatusMap[id] == PlayerStatusCode.PLAYER_DISCONNECTED_CODE;
    }
    
    public void SetPlayerToAlive(ulong id)
    {
        SetPlayerToAliveClientRpc(id);
    }

    public void SetPlayerToDead(ulong id)
    {
        SetPlayerToDeadClientRpc(id);
    }

    public void SetPlayerToEscaped(ulong id)
    {
        SetPlayerToEscapedClientRpc(id);
    }

    public void SetPlayerToDisconnected(ulong id)
    {
        SetPlayerToDisconnectedClientRpc(id);
    }

    [ClientRpc]
    private void SetPlayerToAliveClientRpc(ulong id)
    {
        playerConnectionStatusMap[id] = PlayerStatusCode.PLAYER_ALIVE_CODE;
    }

    [ClientRpc]
    private void SetPlayerToDeadClientRpc(ulong id)
    {
        playerConnectionStatusMap[id] = PlayerStatusCode.PLAYER_DEAD_CODE;
        OnPlayerDeath?.Invoke(id);
    }

    [ClientRpc]
    private void SetPlayerToEscapedClientRpc(ulong id)
    {
        playerConnectionStatusMap[id] = PlayerStatusCode.PLAYER_ESCAPED_CODE;
        OnPlayerSurvived?.Invoke(id);
    }

    [ClientRpc]
    private void SetPlayerToDisconnectedClientRpc(ulong id)
    {
        playerConnectionStatusMap[id] = PlayerStatusCode.PLAYER_DISCONNECTED_CODE;
    }
}
