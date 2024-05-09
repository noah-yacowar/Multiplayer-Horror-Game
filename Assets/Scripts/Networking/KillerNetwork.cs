using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class KillerNetwork : NetworkBehaviour
{
    private Transform playerCameraTransform;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCameraTransform = transform.Find("Camera");
            playerCameraTransform.gameObject.SetActive(true);
            PlayerNetworkSpawner.Instance.NotifyServerClientHasLoadedPlayerPrefabServerRpc();

            /*
            AudioListener audioListener = playerCam.GetComponent<AudioListener>();
            audioListener.enabled = true;

            Camera camera = playerCam.GetComponent<Camera>();
            camera.enabled = true;
            */
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsServer)
        {
            UpdatePlayerLocationClientRpc(transform.localRotation, transform.localPosition);
        }

        if(IsOwner)
        {
            RequestUpdatePositionServerRpc(transform.localRotation, transform.localPosition);
        }
    }

    [ServerRpc]
    private void RequestUpdatePositionServerRpc(Quaternion rotation, Vector3 position)
    {
        // Apply the vertical rotation to the cameraTransform's localEulerAngles
        transform.localRotation = rotation;
        transform.localPosition = position;    
    }

    [ClientRpc]
    private void UpdatePlayerLocationClientRpc(Quaternion rotation, Vector3 position)
    {
        if(!IsOwner && !IsServer)
        {
            transform.localRotation = rotation;
            transform.localPosition = position;
        }
    }
}

