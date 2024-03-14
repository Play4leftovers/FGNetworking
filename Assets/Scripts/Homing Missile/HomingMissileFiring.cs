using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class HomingMissileFiring : NetworkBehaviour
{
    private PlayerController _playerController;

    //I call it missileType since in the future one might want to have different kinds of missiles that act like a homing one, but may have different effects
    //Such as invisible missiles or ones with area of effect, or such like
    [SerializeField] private GameObject missileType;
    
    //Assigns the playerController variable that exists on the player
    //Assigns the missileEvent from PlayerController.cs to OnMissileEvent
    public override void OnNetworkSpawn()
    {
        _playerController = gameObject.GetComponent<PlayerController>();
        _playerController.onMissileEvent += OnMissileEvent;
    }

    //This function listens to if the player hits the Missile key
    //It then calls on SpawnServerMissileRpc and sending its own clientId along with it
    private void OnMissileEvent(bool isFiring)
    {
        if (isFiring)
        {
            SpawnServerMissileRpc(gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    //This calls on the server to instantiate a missile far off the map so as to not be interacted with the players for now
    //Then it calls on the MissilePreFire with the clientId of the one who fired it as it will be required to identify where the missile shall move and who it can target.
    //When that is done, the server properly spawn the missile on all clients.
    [Rpc(SendTo.Server)]
    private void SpawnServerMissileRpc(ulong playerID)
    {
        GameObject newMissile = Instantiate(missileType, transform.position, Quaternion.identity);
        NetworkObject no = newMissile.GetComponent<NetworkObject>();
        no.Spawn();
        newMissile.GetComponent<HomingMissile>().MissilePreFire(playerID);
        
    }
}