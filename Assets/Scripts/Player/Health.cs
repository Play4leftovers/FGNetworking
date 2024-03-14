using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Health : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> lives = new NetworkVariable<int>();

    public NetworkVariable<bool> dead = new NetworkVariable<bool>();
    private PlayerController _playerController;

    //On spawning it sets up all values as if the player needs to be alive
    public override void OnNetworkSpawn()
    {
        _playerController = GetComponent<PlayerController>();
        if(!IsServer) return;
        dead.Value = false;
        currentHealth.Value = 100;
        lives.Value = 3;
    }

    //Several objects can directly damage the player. On taking damage it makes sure that it does deal damage and doesn't heal the player.
    //If the player has zero or less health remaining it calls on the Death function
    public void TakeDamage(int damage)
    {
        damage = damage<0? damage:-damage;
        currentHealth.Value += damage;
        if (currentHealth.Value <= 0)
        {
            Death();
        }
    }

    //Some objects can heal the player. Like TakeDamage it makes sure that it heals and doesn't deal damage.
    public void TakeHealth(int health)
    {
        health = health > 0 ? health : -health;
        currentHealth.Value += health;
    }

    //When the player dies it checks first if it is happening on the server as all values are communicated and death should only happen once.
    //Then it checks that the remaining lives are above 1. If it is above one it reduces the lives by one and resets the current health. It declares the player dead and sets about respawning them.
    //After the invoke it teleports the player away as to keep them out of the game until the respawn is complete.
    //If they lost their last life they are instead disconnected.
    private void Death()
    {
        if (!IsServer) return;
        if (lives.Value > 1)
        {
            lives.Value--;
            currentHealth.Value = 100;
            
            dead.Value = true;
            Invoke(nameof(Respawn), 2);
            DeathTeleportRpc(new Vector3(99,99,99));
        }
        else
        {
            NetworkManager.DisconnectClient(GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    //As the NetworkTransform is overwritten in NetworkTransformClientAuth to ensure that the player moves itself rather than having to ask the server
    //The server instead sends back to the player to move them out of the way.
    [Rpc(SendTo.Owner)]
    private void DeathTeleportRpc(Vector3 coordinates)
    {
        _playerController.TeleportPlayer(coordinates);
    }

    //When the player respawns they are teleported back to a random position on the map and their dead state is set to false.
    private void Respawn()
    {
        int xPosition = Random.Range(-4, 4);
        int yPosition = Random.Range(-2, 2);
        DeathTeleportRpc(new Vector3(xPosition,yPosition,0));

        dead.Value = false;
    }
}
