using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneratorCompletionCheck : NetworkBehaviour
{
    private const float MAX_CHECK_COMPLETION_TIMER = 10f;
    private float checkCompletionTimer = MAX_CHECK_COMPLETION_TIMER;

    void Update()
    {
        checkCompletionTimer += Time.deltaTime;

        if(checkCompletionTimer >= MAX_CHECK_COMPLETION_TIMER) 
        {
            checkCompletionTimer = 0;
            CheckAndHandleAllGensComplete();
        }
    }

    private void CheckAndHandleAllGensComplete()
    {
        if (IsServer)
        {
            GeneratorController[] generators = FindObjectsOfType<GeneratorController>();
            foreach (GeneratorController genController in generators)
            {
                if(!genController.IsGeneratorRepaired())
                {
                    return;
                }
            }

            foreach(ulong clientId in PersistingPlayerData.Instance.GetAllPlayerClientIDs())
            {
                if(PlayerTransformHolder.Instance.IsPlayerSurvivorById(clientId) && PlayerStateHolder.Instance.IsPlayerAlive(clientId)) 
                {
                    EndGameHandler.Instance.SurvivorEscaped(clientId);
                }
            }
        }
    }
}
