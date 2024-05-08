using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private Transform playerCardParent; // Parent object to hold the player cards

    public Sprite killerStatusImg;
    public Sprite survivorStatusImg;

    public Color readyColour = Color.red;
    public Color unreadyColour = Color.white;

    private List<GameObject> playerCards = new List<GameObject>();
    private float yOffsetForPlayerCards = 150f;

    private void OnEnable()
    {
        // Subscribe the SpawnPlayerCards function to the event
        LobbyController.Instance.OnPlayerDataChanged += SpawnPlayerCards;
    }

    private void OnDisable()
    {
        // Unsubscribe the SpawnPlayerCards function from the event
        LobbyController.Instance.OnPlayerDataChanged -= SpawnPlayerCards;
    }

    // Function to spawn a player card for each player
    public void SpawnPlayerCards(List<Player> players)
    {
        // Clear existing player cards
        foreach (var card in playerCards)
        {
            Destroy(card);
        }
        playerCards.Clear();

        // Instantiate player cards for each player
        float yOffset = 0f; // initial vertical offset

        foreach (var player in players)
        {
            GameObject playerCard = Instantiate(playerCardPrefab, playerCardParent);
            
            // Set the position of the player card
            playerCard.transform.localPosition = new Vector3(0f, yOffset, 0f);

            TextMeshProUGUI playerNameText = playerCard.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>();
            Image playerStatusImg = playerCard.transform.Find("ReadyStatusImg").GetComponent<Image>();

            if (playerNameText != null && playerStatusImg != null)
            {
                string playerName = player.Data[LobbyController.KEY_PLAYER_NAME].Value;
                string playerRole = player.Data[LobbyController.KEY_PLAYER_ROLE].Value;
                string isPlayerReady = player.Data[LobbyController.KEY_PLAYER_READY_STATUS].Value;

                playerNameText.text = playerName;

                if (playerRole == "SURVIVOR")
                {
                    playerStatusImg.sprite = survivorStatusImg;
                }
                else
                {
                    playerStatusImg.sprite = killerStatusImg;
                }

                if(isPlayerReady == "TRUE")
                {
                    playerStatusImg.color = readyColour;
                }
                else
                {
                    playerStatusImg.color = unreadyColour;
                }
            }

            playerCards.Add(playerCard);

            // Increment the vertical offset for the next card
            yOffset -= yOffsetForPlayerCards;
        }
    }
}

