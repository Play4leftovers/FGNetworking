using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AmmoPack : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        FiringAction firingAction = other.GetComponent<FiringAction>();
        if (!firingAction) return;
        if (firingAction.ammo.Value == firingAction.startingAmmo) return;

        firingAction.ammo.Value = firingAction.startingAmmo;
        
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
    }
}