using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : NetworkBehaviour
{
    public static LobbyController Instance { get; private set; }

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float heartbeatTimerInterval = 15;

    private float lobbyUpdateTimer;
    private float lobbyUpdateInterval = 2;

    private float lobbyActivePlayerCheckTimer;
    private float lobbyActivePlayerCheckInterval = 6;

    private float lobbyReadyUpCheckTimer;
    private float lobbyReadyUpCheckInterval = 4;

    private string playerName = "";
    private bool readyStatus = false;
    private bool isPlayerSurvivor = true;
    public PlayerNameData playerNameData;

    public const string KEY_START_GAME = "StartGame";
    public const string KEY_GAME_MODE = "GameMode";

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_CLIENT_ID = "ClientId";
    public const string KEY_PLAYER_ROLE = "PlayerRole";
    public const string KEY_PLAYER_LOADED_STATUS = "PlayerLoadedStatus";

    public const string KEY_ACTIVE_PLAYER_CHECK = "ActivePlayerCheck";
    public const string KEY_PLAYER_READY_STATUS = "ReadyStatus";
    public const string KEY_NEXT_MAP_NAME = "NextMapName";

    private Dictionary<string, bool> isNewPlayerData = new Dictionary<string, bool>();

    private int activePlayerCode = 0;
    private int iterationsSinceLastActiveCodeUpdate = 0;
    private const int MAX_PASSED_ACTIVE_CODE_ITERATIONS = 5;

    private float delayClientIdUpdateCall = 6f;

    bool gameIsStarting = false;
    bool clientIdAssigned = false;
    bool playersLoaded = false;

    private System.Random rng;

    public event Action<List<Player>> OnPlayerDataChanged;

    public event Action OnLobbyJoined;
    public event Action OnLobbyLeft;
    public event Action<bool> OnPlayerReady;

    private void Awake()
    {
        Instance = this;

        heartbeatTimer = heartbeatTimerInterval;
        lobbyUpdateTimer = lobbyUpdateInterval;
        lobbyActivePlayerCheckTimer = lobbyActivePlayerCheckInterval;
        lobbyReadyUpCheckTimer = lobbyReadyUpCheckInterval;

        rng = new System.Random();
    }

    private void Start()
    {
        playerName = playerNameData.GetPlayerName();
    }

    private void Update()
    {
        HandleLobbyPollForUpdates();
        HandleLobbyHeartbeat();
        HandleLobbyInactivity();
        HandleLobbyStarting();
        HandleLobbyClientIdAssigning();
        HandlePlayersLoaded();
    }

    private async Task UpdatePersistingPlayerData()
    {
        foreach (Player player in joinedLobby.Players)
        {
            PersistingPlayerData.Instance.AssignNewPlayerData(
                                                                ulong.Parse(player.Data[KEY_CLIENT_ID].Value),
                                                                player.Data[KEY_PLAYER_NAME].Value.ToString()
                                                             );                                        
        }
        
    }

    public async Task StartGame()
    {
        if (IsPlayerLobbyHost())
        {
            try
            {
                Debug.Log("Starting Game");

                string relayCode = await RelayHandling.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                StartCoroutine(DelayUpdateClientId(delayClientIdUpdateCall));

                joinedLobby = lobby;
                isNewPlayerData.Clear();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                heartbeatTimer = heartbeatTimerInterval;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyStarting()
    {
        if (gameIsStarting) return;

        if (joinedLobby != null && IsPlayerLobbyHost())
        {
            lobbyReadyUpCheckTimer -= Time.deltaTime;
            if (lobbyReadyUpCheckTimer < 0)
            {
                lobbyReadyUpCheckTimer = lobbyReadyUpCheckInterval;

                foreach (Player player in joinedLobby.Players)
                {
                    if (player.Data[KEY_PLAYER_READY_STATUS].Value == "FALSE")
                    {
                        return;
                    }
                }

                await StartGame();

                gameIsStarting = true;
            }
        }
    }

    private async void HandleLobbyClientIdAssigning()
    {
        if (clientIdAssigned) return;

        if (joinedLobby != null) 
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Data[KEY_CLIENT_ID].Value == "-1")
                {
                    return;
                }
            }

            await UpdatePersistingPlayerData();
            UpdatePlayerLoadedStatus("TRUE");
            clientIdAssigned = true;
        }
    }

    private async void HandlePlayersLoaded()
    {
        if (playersLoaded) return;

        if (joinedLobby != null && IsPlayerLobbyHost())
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Data[KEY_PLAYER_LOADED_STATUS].Value == "FALSE")
                {
                    return;
                }
            }

            OnLobbyLeft?.Invoke();
            playersLoaded = true;
            LobbySceneController.Instance.LoadSceneByName(joinedLobby.Data[KEY_NEXT_MAP_NAME].Value);
            joinedLobby = null;
        }
    }

    private void HandleLobbyInactivity()
    {
        if (joinedLobby != null && IsPlayerLobbyHost())
        {
            lobbyActivePlayerCheckTimer -= Time.deltaTime;

            if (lobbyActivePlayerCheckTimer < 0)
            {
                Debug.Log("UPDATING LOBBY ACTIVE CODE.");
                lobbyActivePlayerCheckTimer = lobbyActivePlayerCheckInterval;

                KickDisconnectedPlayers();

                UpdateLobbyActiveCheckCode(rng.Next(1, 1001));
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0)
            {
                lobbyUpdateTimer = lobbyUpdateInterval;

                iterationsSinceLastActiveCodeUpdate++;

                if (iterationsSinceLastActiveCodeUpdate == MAX_PASSED_ACTIVE_CODE_ITERATIONS)
                {
                    joinedLobby = null;
                    OnLobbyLeft?.Invoke();

                    // Pass an empty list
                    List<Player> emptyList = new List<Player>();
                    OnPlayerDataChanged?.Invoke(emptyList);
                    return;
                }

                if (!IsPlayerInLobby())
                {
                    Debug.Log("Kicked from lobby!");

                    joinedLobby = null;
                    OnLobbyLeft?.Invoke();
                    return;
                }

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (activePlayerCode != int.Parse(joinedLobby.Data[KEY_ACTIVE_PLAYER_CHECK].Value))
                {
                    Debug.Log("UPDATING PLAYER ACTIVE CODE.");
                    activePlayerCode = int.Parse(joinedLobby.Data[KEY_ACTIVE_PLAYER_CHECK].Value);
                    UpdatePlayerActiveCheckCode(activePlayerCode);
                    iterationsSinceLastActiveCodeUpdate = 0;
                }

                OnPlayerDataChanged?.Invoke(joinedLobby.Players);

                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsPlayerLobbyHost())
                    {
                        await RelayHandling.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        StartCoroutine(DelayUpdateClientId(delayClientIdUpdateCall));
                    }
                }
            }
        }
    }

    private async void UpdateLobbyActiveCheckCode(int newCode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_ACTIVE_PLAYER_CHECK, new DataObject(DataObject.VisibilityOptions.Member, newCode.ToString()) }
                }
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator DelayUpdateClientId(float delay)
    {
        yield return new WaitForSeconds(delay);

        UpdatePlayerClientId(NetworkManager.Singleton.LocalClientId);
    }

    private async void UpdatePlayerClientId(ulong clientId)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_CLIENT_ID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, clientId.ToString()) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerLoadedStatus(string isReady)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_LOADED_STATUS, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    private async void UpdatePlayerActiveCheckCode(int newCode)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_ACTIVE_PLAYER_CHECK, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newCode.ToString()) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerReadyStatus()
    {
        readyStatus = !readyStatus;
        string newStatus;

        if (readyStatus)
        {
            newStatus = "TRUE";
            OnPlayerReady?.Invoke(true);
        }
        else
        {
            newStatus = "FALSE";
            OnPlayerReady?.Invoke(false);
        }

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_READY_STATUS, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newStatus) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void KickDisconnectedPlayers()
    {
        if (!IsPlayerLobbyHost()) return;

        for (int p = 0; p < joinedLobby.Players.Count; p++)
        {
            string playerActiveCodeValue = joinedLobby.Players[p].Data[KEY_ACTIVE_PLAYER_CHECK].Value;
            string lobbyActiveCodeValue = joinedLobby.Data[KEY_ACTIVE_PLAYER_CHECK].Value;

            string playerId = joinedLobby.Players[p].Id;

            if (playerActiveCodeValue != lobbyActiveCodeValue && (playerActiveCodeValue != "0" || isNewPlayerData.ContainsKey(playerId)))
            {
                isNewPlayerData.Remove(playerId);
                KickPlayer(p);
            }
            else if(playerActiveCodeValue == "0")
            {
                Debug.Log("Setting new player to old.");
                isNewPlayerData[playerId] = true; 
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            isPlayerSurvivor = false;
            string gameMap = LobbySceneController.FORESAKEN_FOREST_SCENE_NAME;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, "Casual", DataObject.IndexOptions.S1)},
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { KEY_ACTIVE_PLAYER_CHECK, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { KEY_NEXT_MAP_NAME, new DataObject(DataObject.VisibilityOptions.Member, gameMap) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            OnLobbyJoined?.Invoke();


            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            isPlayerSurvivor = true;

            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            joinedLobby = lobby;
            OnLobbyJoined?.Invoke();
            Debug.Log("Joined lobby with code '" + lobbyCode + "'");
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void QuickJoinLobby()
    {
        try
        {
            isPlayerSurvivor = true;

            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            OnLobbyJoined?.Invoke();
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        string playerRole;

        if (isPlayerSurvivor)
        {
            playerRole = "SURVIVOR";
        }
        else
        {
            playerRole = "KILLER";
        }

        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { KEY_CLIENT_ID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "-1") },
                { KEY_PLAYER_ROLE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerRole) },
                { KEY_ACTIVE_PLAYER_CHECK, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") },
                { KEY_PLAYER_READY_STATUS, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "FALSE") },
                { KEY_PLAYER_LOADED_STATUS, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "FALSE") },
            }
        };
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void LeaveLobby()
    {
        if(joinedLobby == null)
        {
            return;
        }

        try
        { 
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            
            // Pass an empty list
            List<Player> emptyList = new List<Player>();
            OnPlayerDataChanged?.Invoke(emptyList);

            joinedLobby = null;
            hostLobby = null;
            OnLobbyLeft?.Invoke();
        }
        catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer(int player)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[player].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost(int newHost) 
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[newHost].Id
            }); 
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private bool IsPlayerLobbyHost()
    {
        return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        for(int i = 0; i < joinedLobby.Players.Count; i++)
        {
            if (joinedLobby.Players[i].Id == AuthenticationService.Instance.PlayerId)
            {
                return true;
            }
        }

        return false;
    }

    private void DeleteLobby()
    {
        try
        {
            LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach(Player player in lobby.Players)
        {
            Debug.Log("Player \'" + player.Data[KEY_PLAYER_NAME].Value +'\'');
        }
    }
}
