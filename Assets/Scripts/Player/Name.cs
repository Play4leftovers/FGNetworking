using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class Name : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    
    //On networking spawning the player, it checks if this script runs on the server-side.
    //If it does, it grabs the player game objects and from there the network ID
    //Finally it retrieves the name sent in as part of the joining process.
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        var tempNetworkObject = gameObject.GetComponent<NetworkObject>();
        ulong ID = tempNetworkObject.OwnerClientId;

        playerName.Value = SavedClientInformationManager.GetUserData(ID).userName;
    }
    
    //This is further handled in NameUI.cs
}