using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KillerAttack : NetworkBehaviour
{
    public event Action<ulong> PlayerWasHit;
    public event Action<ulong> PlayerWasMissed; // Needs implementation

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            //if (!canHit)
            //    return;

            if (other.transform.tag == "Survivor")
            {
                ulong clientId = other.transform.GetComponent<NetworkObject>().OwnerClientId;
                PlayerWasHitServerRpc(clientId);
            }
        }
    }

    [ServerRpc]
    private void PlayerWasHitServerRpc(ulong clientId)
    {
        PlayerWasHit?.Invoke(clientId);
    }                                                       
}
