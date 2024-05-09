using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobbySceneController : NetworkBehaviour
{
    public static LobbySceneController Instance { get; private set; }

    public const string FORESAKEN_FOREST_SCENE_NAME = "Foresaken Forest";

    private void Awake()
    {
        Instance = this;
    }

    public void LoadSceneByName(string sceneName)
    {
        // Switch the scene for all clients
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    // Clean up network-related resources before the object is destroyed
    override public void OnDestroy()
    {
        // Unregister the object from the network
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
        }

        base.OnDestroy();
    }
}