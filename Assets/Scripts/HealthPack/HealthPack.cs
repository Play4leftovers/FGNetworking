using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthPack : NetworkBehaviour
{
    [SerializeField] private int healthGain = 25;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        Health health = other.GetComponent<Health>();
        if (!health) return;
        if (health.currentHealth.Value > 99) return;
        
        health.TakeHealth(healthGain);
        if (health.currentHealth.Value > 100) health.currentHealth.Value = 100;
        
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
    }
}