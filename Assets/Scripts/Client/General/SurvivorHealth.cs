using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SurvivorHealth : MonoBehaviour
{
    public int health;
    public const int MAX_HEALTH = 3;

    private void Awake()
    {
        health = MAX_HEALTH;
    }

    public void PlayerWasHit()
    {
        DecrementPlayerHealthClientRpc();
    }

    [ClientRpc]
    private void DecrementPlayerHealthClientRpc()
    {
        Debug.Log("Player Was Hit!");
        health--;
    }
}
