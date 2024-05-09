using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject playerStatusPrefab;
    [SerializeField] private Transform playerStatusParent; // Parent object to hold the player cards

    public Sprite deadStatusImg;
    public Sprite aliveStatusImg;
    public Sprite disconnectedStatusImg;
    public Sprite escapedStatusImg;

    private bool allPlayerStatsSpawned = false;

    public Color injuredColour = Color.red;

    private List<GameObject> playerStatusItems = new List<GameObject>();
    private float yOffsetForPlayerStats = 90f;

    private const float RETRIEVE_PLAYER_STATS_MAX_TIMER = 5f;
    private float retrievePlayerStatsTimer = 0f;

    private void Start()
    {
        retrievePlayerStatsTimer = RETRIEVE_PLAYER_STATS_MAX_TIMER;
        PlayerTransformHolder.Instance.PlayerTransformsFound += (() => { allPlayerStatsSpawned = true; });
    }

    private void Update()
    {
        if(allPlayerStatsSpawned)
        {
            retrievePlayerStatsTimer += Time.deltaTime;

            if (retrievePlayerStatsTimer >= RETRIEVE_PLAYER_STATS_MAX_TIMER) 
            {
                Debug.Log("Updating Player Stats UI");
                retrievePlayerStatsTimer = 0;
                SpawnPlayerStats();
            }
        }
    }

    // Function to spawn a player card for each player
    public void SpawnPlayerStats()
    {
        Debug.Log("Spawning Players UI Stat Items");

        // Clear existing player cards
        foreach (var playerStat in playerStatusItems)
        {
            Destroy(playerStat);
        }
        playerStatusItems.Clear();

        // Instantiate player cards for each player
        float yOffset = 0f; // initial vertical offset

        foreach (ulong clientId in PersistingPlayerData.Instance.GetAllPlayerClientIDs())
        {
            if (PlayerTransformHolder.Instance.IsPlayerKillerById(clientId))
            {
                continue;
            }

            GameObject playerStatusItem = Instantiate(playerStatusPrefab, playerStatusParent);

            // Set the position of the player card
            playerStatusItem.transform.localPosition = new Vector3(0f, yOffset, 0f);

            TextMeshProUGUI playerNameText = playerStatusItem.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>();
            Image playerStatusImg = playerStatusItem.transform.Find("ConnectionStatusImg").GetComponent<Image>();

            if (playerNameText != null && playerStatusImg != null)
            {
                string playerName = PersistingPlayerData.Instance.GetPlayerNameByClientId(clientId);
                playerNameText.text = playerName;

                if (PlayerStateHolder.Instance.IsPlayerAlive(clientId))
                {
                    playerStatusImg.sprite = aliveStatusImg;
                }
                else if(PlayerStateHolder.Instance.IsPlayerDead(clientId))
                {
                    playerStatusImg.sprite = deadStatusImg;
                }
                else if (PlayerStateHolder.Instance.IsPlayerEscaped(clientId))
                {
                    playerStatusImg.sprite = escapedStatusImg;
                }
                else
                {
                    playerStatusImg.sprite = disconnectedStatusImg;
                }

            }

            playerStatusItems.Add(playerStatusItem);

            // Increment the vertical offset for the next card
            yOffset -= yOffsetForPlayerStats;
        }
    }
}
