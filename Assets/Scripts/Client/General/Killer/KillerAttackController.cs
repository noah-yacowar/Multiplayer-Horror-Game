using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KillerAttackController : NetworkBehaviour
{
    private KillerAnimationStateController killerAnimatorController;

    [SerializeField] private GameObject attackAreaCollider;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    void Start()
    {
        if(IsServer)
        {
            killerAnimatorController = GetComponent<KillerAnimationStateController>();

            attackAreaCollider = transform.Find("AttackArea/Collider").gameObject;
            attackAreaCollider.SetActive(false);

            attackAreaCollider.GetComponent<KillerAttack>().PlayerWasHit += PlayerHit;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(attackKey))
            {
                transform.GetComponent<KillerAnimationStateController>().PlayerIsAttacking();
            }
        }

        if (IsServer)
        {
            if(killerAnimatorController.IsAnimatorInAttackingState())
            {
                attackAreaCollider.SetActive(true);
            }
            else if(attackAreaCollider.activeSelf)
            {
                attackAreaCollider.SetActive(false);
            }
        }        
    }
        
    private void PlayerHit(ulong clientId)
    {
        SurvivorHealth survivorHealth = PlayerTransformHolder.Instance.GetPlayerTransformById(clientId).GetComponent<SurvivorHealth>();
        survivorHealth.PlayerWasHit();
    }
}
