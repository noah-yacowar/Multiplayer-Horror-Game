using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float speed = 0.5f;

    // Update is called once per frame
    private void Update()
    {
        if(!IsOwner) return;

        // Get input from the keyboard
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        RequestUpdatePositionServerRpc(horizontalInput, verticalInput);
    }

    [ServerRpc]
    private void RequestUpdatePositionServerRpc(float horizontal, float vertical)
    {
        // Calculate movement vector on the server
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical) * speed * Time.deltaTime;

        // Move the player on the server
        transform.Translate(movement);
    }
}
