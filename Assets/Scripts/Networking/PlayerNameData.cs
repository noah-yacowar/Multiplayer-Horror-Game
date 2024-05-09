using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNameData", menuName = "Player/Player Name Data")]
public class PlayerNameData : ScriptableObject
{
    public string playerName;

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
