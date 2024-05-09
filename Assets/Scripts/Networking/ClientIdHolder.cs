using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientIdHolder : NetworkBehaviour
{
    public ulong clientId;

    private void Awake()
    {
        clientId = NetworkManager.Singleton.LocalClientId;
    }

}
