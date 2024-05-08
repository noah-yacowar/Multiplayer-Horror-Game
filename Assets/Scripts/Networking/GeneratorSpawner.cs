using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GeneratorSpawner : NetworkBehaviour
{
    public static GeneratorSpawner Instance { get; private set; }

    [SerializeField] private GameObject playingAreaObject; // Assign this in the Inspector
    [SerializeField] private GameObject generatorObject;
    [SerializeField] private int numGens = 5;
    public List<float> genVals = new List<float>();
    public event Action GeneratorsFullySpawned;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayerTransformHolder.Instance.PlayerTransformsFound += GenerateGenerators;
        GeneratorsFullySpawned += RequestAssignUIToGens;
    }

    public void GenerateGenerators()
    { 
        if (IsServer)
        {
            if (SpawnGenerators())
            {
                InformClientsGeneratorsAreSpawnedClientRpc();
            }
            else
            {
                Debug.Log("Issue with spawning generators");
            }        
        }  
    }

    private void RequestAssignUIToGens()
    {
        RequestAssignUIToGensServerRpc(NetworkManager.Singleton.LocalClient.ClientId);
    }

    private void UpdateUIGenProgress(int genID, float newProgress)
    {
        genVals[genID] = newProgress;
    }

    private bool SpawnGenerators()
    {
        if (playingAreaObject != null && generatorObject != null)
        {
            MeshRenderer groundMeshRenderer = playingAreaObject.GetComponent<MeshRenderer>();
            Renderer generatorRenderer = generatorObject.transform.Find("LightGenerator").GetComponent<Renderer>();

            if (groundMeshRenderer != null)
            {
                Vector3 planeSize = groundMeshRenderer.bounds.size;
                Vector3 planePosition = playingAreaObject.transform.position;

                // Define a maximum raycast distance
                float maxRaycastDistance = 200f;

                for (int genNum = 0; genNum < numGens; genNum++)
                {
                    Vector3 randomPosition = new Vector3(
                        UnityEngine.Random.Range(-planeSize.x / 2, planeSize.x / 2),
                        0,
                        UnityEngine.Random.Range(-planeSize.z / 2, planeSize.z / 2)
                    );

                    // Adjust the random position by the plane's world position
                    randomPosition += planePosition;

                    // Use a raycast from above the plane to find the exact y position on the surface
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(randomPosition.x, planePosition.y + maxRaycastDistance - 1, randomPosition.z), Vector3.down, out hit, maxRaycastDistance))
                    {
                        // Set the y position to where the raycast hit the plane
                        randomPosition.y = hit.point.y + generatorRenderer.bounds.size.y / 2;
                    }
                    else
                    {
                        // If the raycast didn't hit anything, log an error or use a default y position
                        Debug.LogError("Raycast didn't hit the plane surface. Using default y position.");
                        randomPosition.y = planePosition.y;
                    }

                    // Instantiate the generator at the randomPosition
                    float randomYRotation = UnityEngine.Random.Range(0f, 360f); // Generate a random rotation angle between 0 and 360 degrees
                    Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f); // Create a Quaternion based on the random Y rotation
                    GameObject spawnedGenerator = Instantiate(generatorObject, randomPosition, randomRotation);
                    spawnedGenerator.GetComponent<NetworkObject>().Spawn();
                    //spawnedGenerator.transform.parent = transform; NOT FUNCTIONAL
                }

                return true;
            }
            else
            {
                Debug.LogError("The planeGameObject does not have a MeshRenderer component.");
                return false;
            }
        }
        else
        {
            Debug.LogError("Plane GameObject or Generator Object is not assigned.");
            return false;
        }
    }

    [ClientRpc]
    private void InformClientsGeneratorsAreSpawnedClientRpc()
    {
        GeneratorsFullySpawned?.Invoke();
    }

    [ClientRpc]
    private void AssignUIToGensClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClient.ClientId == clientId)
        {
            int genNum = 0;

            GeneratorController[] generators = FindObjectsOfType<GeneratorController>();
            foreach (GeneratorController genController in generators)
            {
                genController.genID = genNum;
                genController.UpdatedGenProgress += UpdateUIGenProgress;
                genVals.Add(0);
                genNum++;
            }
        }  
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAssignUIToGensServerRpc(ulong clientId)
    {
        AssignUIToGensClientRpc(clientId);
    }
}
