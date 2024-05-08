using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIOptions : MonoBehaviour
{
    public static LobbyUIOptions Instance {  get; private set; }

    [SerializeField] private Button killerButton;
    [SerializeField] private Button survivorButton;
    [SerializeField] private Button customGameButton;
    [SerializeField] private Button LeaveButton;

    private const string readyUpText = "Press E To Ready Up!";
    private const string unreadyUpText = "Press E To Unready!";

    [SerializeField] private TextMeshProUGUI readyUpTextObject;
    [SerializeField] private KeyCode readyUpKey = KeyCode.E;

    private void Awake()
    {
        Instance = this;

        killerButton.onClick.AddListener(KillerButtonClick);
        survivorButton.onClick.AddListener(SurvivorButtonClick);
        LeaveButton.onClick.AddListener(LeaveLobbyButtonClick);

        LobbyController.Instance.OnLobbyJoined += ShowReadyUpMessage;
        LobbyController.Instance.OnLobbyLeft += HideReadyUpMessage;
        LobbyController.Instance.OnPlayerReady += ChangeReadyUpMessage;

        LobbyController.Instance.OnLobbyLeft += LeaveLobbyButtonClick;
    }

    private void Update()
    {
        TogglePlayerReadyStatus();
    }

    private void OnDestroy()
    {
        LobbyController.Instance.OnLobbyJoined -= ShowReadyUpMessage;
        LobbyController.Instance.OnLobbyLeft -= HideReadyUpMessage;
        LobbyController.Instance.OnPlayerReady -= ChangeReadyUpMessage;
    }

    private void KillerButtonClick()
    {
        ShowInLobbyUI();
        LobbyController.Instance.CreateLobby();
        StartCoroutine(TheatreVideoController.Instance.ChangeScreenToLobbyKiller());
    }

    private void SurvivorButtonClick()
    {
        ShowInLobbyUI();
        LobbyController.Instance.QuickJoinLobby();
        StartCoroutine(TheatreVideoController.Instance.ChangeScreenToLobbySurvivor());
    }

    private void LeaveLobbyButtonClick()
    {
        ShowOutOfLobbyUI();
        LobbyController.Instance.LeaveLobby();
        StartCoroutine(TheatreVideoController.Instance.ChangeScreenToLobbyOptions());
    }

    private void TogglePlayerReadyStatus()
    {
        if(Input.GetKeyDown(readyUpKey)) 
        {
            LobbyController.Instance.UpdatePlayerReadyStatus();
        }
    }

    private void ChangeReadyUpMessage(bool isReady)
    {
        if(!isReady) 
        {
            readyUpTextObject.text = readyUpText;
        }
        else
        {
            readyUpTextObject.text = unreadyUpText;
        }
    }

    private void ShowReadyUpMessage()
    {
        readyUpTextObject.gameObject.SetActive(true);
    }

    private void HideReadyUpMessage()
    {
        readyUpTextObject.gameObject.SetActive(false);
    }

    private void ShowInLobbyUI()
    {
        killerButton.gameObject.SetActive(false);
        survivorButton.gameObject.SetActive(false);
        customGameButton.gameObject.SetActive(false);
        LeaveButton.gameObject.SetActive(true);
    }

    private void ShowOutOfLobbyUI()
    {
        killerButton.gameObject.SetActive(true);
        survivorButton.gameObject.SetActive(true);
        customGameButton.gameObject.SetActive(true);
        LeaveButton.gameObject.SetActive(false);
    }
}
