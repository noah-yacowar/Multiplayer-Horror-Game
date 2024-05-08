using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuOptions : NetworkBehaviour
{
    public KeyCode menuActivationKey = KeyCode.Escape;
    public GameObject menu;

    [SerializeField] private Button leaveButton;

    private void Start()
    {
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
    }

    void Update()
    {
        if(Input.GetKeyDown(menuActivationKey)) 
        {
            if(menu.activeSelf)
            {
                menu.SetActive(false);
            }
            else
            {
                menu.SetActive(true);
            }
        }
    }

    private void OnLeaveButtonPressed()
    {
        EndGameHandler.Instance.PlayerRequestingLeaveServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
