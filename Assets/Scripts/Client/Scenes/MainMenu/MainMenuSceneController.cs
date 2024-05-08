using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainMenuSceneController : NetworkBehaviour
{
    public const string LOBBY_SCENE_NAME = "Lobby";
    public PlayerNameData playerNameData;

    private void Awake()
    {
        MainMenuUI.Instance.EnteredName += LoadLobbyScene;
    }

    public override void OnDestroy()
    {
        MainMenuUI.Instance.EnteredName -= LoadLobbyScene;
    }

    private void LoadLobbyScene(string playerName)
    {
        playerNameData.SetPlayerName(playerName);
        SceneManager.LoadScene(LOBBY_SCENE_NAME);
    }
}