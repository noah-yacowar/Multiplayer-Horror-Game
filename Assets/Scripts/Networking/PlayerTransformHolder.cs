using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using Unity.Services.Lobbies.Models;

public class PlayerTransformHolder : NetworkBehaviour
{
    public static PlayerTransformHolder Instance { get; private set; }

    const string SURVIVOR_TAG = "Survivor";
    const string KILLER_TAG = "Killer";

    private Dictionary<ulong, Transform> playerTransformsMap = new Dictionary<ulong, Transform>();

    public event Action PlayerTransformsFound;

    private void Awake()
    {
        Instance = this;
        PlayerNetworkSpawner.Instance.AllPlayersSpawned += FindAllPlayerTransforms;
    }

    public Transform GetPlayerTransformById(ulong id)
    {
        return playerTransformsMap[id];
    }

    public Transform GetKillerTransform()
    {
        foreach(Transform playerTransform in GetAllPlayerTransforms()) 
        {
            if(playerTransform.tag == KILLER_TAG)
            {
                return playerTransform;
            }
        }

        return null;
    }

    public bool IsPlayerSurvivorById(ulong id)
    {
        if (GetPlayerTransformById(id).tag == SURVIVOR_TAG)
        {
            return true;
        }

        return false;
    }

    public bool IsPlayerKillerById(ulong clientId)
    {
        Transform clientTransform = GetPlayerTransformById(clientId);

        if (clientTransform.tag == KILLER_TAG)
        {
            return true;
        }

        return false;
    }

    public List<Transform> GetAllPlayerTransforms()
    {
        return playerTransformsMap.Values.ToList();
    }

    private void FindAllPlayerTransforms()
    {
        string[] targetTags = { SURVIVOR_TAG, KILLER_TAG };

        // Find all GameObjects with the specified tag
        GameObject[] players = new GameObject[0]; // Initialize an empty array to store all found objects

        // Loop through each tag and find GameObjects with that tag
        foreach (string tag in targetTags)
        {
            // Find GameObjects with the current tag and append them to the existing array
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            players = CombineArrays(players, objectsWithTag);
        }

        // Loop through each tagged GameObject and access its transform
        foreach (GameObject taggedOplayerbject in players)
        {
            Transform playerTransform = taggedOplayerbject.transform;

            playerTransformsMap[playerTransform.GetComponent<NetworkObject>().OwnerClientId] = playerTransform;
        }

        PlayerTransformsFound?.Invoke();
    }

    // Helper method to combine two arrays
    private GameObject[] CombineArrays(GameObject[] array1, GameObject[] array2)
    {
        GameObject[] combinedArray = new GameObject[array1.Length + array2.Length];
        array1.CopyTo(combinedArray, 0);
        array2.CopyTo(combinedArray, array1.Length);
        return combinedArray;
    }

    //Does not physically despawn player just translated prefab below visible ground
    public void RemovePlayer(ulong playerId)
    {
        RemovePlayerClientRpc(playerId);
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong playerId)
    {
        Debug.Log("Removing Player");
        Transform playerToRemove = GetPlayerTransformById(playerId);
        playerToRemove.GetComponent<HandlePlayerDeactivation>().DisablePlayerPrefab();

        StartCoroutine(HidePlayerFromView(playerToRemove));
    }

    private IEnumerator HidePlayerFromView(Transform playerTransform)
    {
        yield return new WaitForSeconds(2);
        
        Vector3 currentPosition = playerTransform.position;
        playerTransform.GetComponent<Rigidbody>().position = currentPosition + Vector3.down * 500;
    }
}
