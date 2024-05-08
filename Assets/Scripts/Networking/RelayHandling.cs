using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

public class RelayHandling : MonoBehaviour
{
    public static RelayHandling Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}