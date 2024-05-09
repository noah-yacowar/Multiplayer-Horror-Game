using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersistingPlayerData : MonoBehaviour
{
    public static PersistingPlayerData Instance { get; private set; }

    private Dictionary<ulong, string> playerNamesMap = new Dictionary<ulong, string>();
    public int playerCount = 0;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<string> GetAllPlayerNames()
    {
        return playerNamesMap.Values.ToList();
    }

    public List<ulong> GetAllPlayerClientIDs()
    {
        return playerNamesMap.Keys.ToList();
    }

    public string GetPlayerNameByClientId(ulong clientId)
    {
        return playerNamesMap[clientId];
    }

    public int GetPlayerCount()
    {
        return playerCount;
    }

    public void AssignNewPlayerData(ulong clientId, string playerName)
    {
        playerNamesMap.Add(clientId, playerName);
        playerCount++;
    }

    public void ClearAllPlayers()
    {
        playerNamesMap.Clear();
        playerCount = 0;
    }
}
