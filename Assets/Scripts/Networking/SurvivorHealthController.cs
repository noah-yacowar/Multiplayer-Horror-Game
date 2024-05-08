using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SurvivorHealthController : NetworkBehaviour
{
    private float serverCheckSurvivorHealthTimer;
    private const float CHECK_SURVIVOR_HEALTH_MAX_TIMER = 2f;

    void Start()
    {
        serverCheckSurvivorHealthTimer = 0;
    }

    void Update()
    {
        if(IsServer)
        {
            serverCheckSurvivorHealthTimer += Time.deltaTime;

            if(serverCheckSurvivorHealthTimer >= CHECK_SURVIVOR_HEALTH_MAX_TIMER)
            {
                serverCheckSurvivorHealthTimer = 0;

                ServerCheckSurvivorHealthStatus();
            }
        }
    }

    private void ServerCheckSurvivorHealthStatus() 
    {
        foreach (Transform playerTransform in PlayerTransformHolder.Instance.GetAllPlayerTransforms()) 
        {
            ulong playerOwnerId = playerTransform.GetComponent<NetworkObject>().OwnerClientId;
            bool isPlayerSurvivor = PlayerTransformHolder.Instance.IsPlayerSurvivorById(playerOwnerId);
            
            if(isPlayerSurvivor && playerTransform.GetComponent<SurvivorHealth>().health <= 0)
            {
                EndGameHandler.Instance.SurvivorDead(playerOwnerId);
            }
        }
    }
}
