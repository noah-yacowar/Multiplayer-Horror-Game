using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class SurvivorNetwork : NetworkBehaviour
{
    private Transform playerCameraTransform;
    private Transform playerRigTransform;

    public override void OnNetworkSpawn()
    {
        playerRigTransform = transform.Find("Armature");

        if (IsOwner)
        {
            playerCameraTransform = transform.Find("Camera Center");
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
            UpdatePlayerLocationClientRpc(playerRigTransform.transform.localRotation, transform.localPosition);
        }

        if (IsOwner)
        {
            RequestUpdatePositionServerRpc(playerRigTransform.transform.localRotation, transform.localPosition);
        }
    }

    [ServerRpc]
    private void RequestUpdatePositionServerRpc(Quaternion rotation, Vector3 position)
    {
        // Apply the vertical rotation to the cameraTransform's localEulerAngles
        playerRigTransform.localRotation = rotation;
        transform.localPosition = position;
        Debug.Log("I am the culprit!");
    }

    [ClientRpc]
    private void UpdatePlayerLocationClientRpc(Quaternion rotation, Vector3 position)
    {
        if (!IsOwner && !IsServer)
        {
            playerRigTransform.localRotation = rotation;
            transform.localPosition = position;
        }
    }
}


