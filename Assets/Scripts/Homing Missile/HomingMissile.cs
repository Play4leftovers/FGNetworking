using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class HomingMissile : NetworkBehaviour
{
    [SerializeField] private float missileSpeed = 10;
    [SerializeField] private int missileDamage = 40;
    
    private NetworkClient closestEnemy;
    private NetworkClient launchingPlayer;
    private float closestEnemyDistance = 0;
    private bool missileFired = false;
    private Rigidbody2D _rb;

    //All required during Network Spawn is to assign a reference to the rigidbody component 
    //It also invokes FireMissile with a one-second delay
    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        Invoke(nameof(FireMissile), 1);
    }

    //After it has been instantiated on the server, the HomingMissileFiring.cs calls on this one.
    //First it gets a list of all the players on the server and the player who launched it specifically.
    //Afterwards it runs through each of the players to find who is the nearest one to the player, setting the ClosestEnemy to equal to that one
    public void MissilePreFire(ulong playerID)
    {
        var playerList = NetworkManager.ConnectedClients;
        
        launchingPlayer = playerList[playerID];
        
        Vector2 launchingPlayerPosition = launchingPlayer.PlayerObject.gameObject.transform.position;
        

        foreach (var player in playerList)
        {
            if(player.Key == playerID || player.Value.PlayerObject.GetComponent<Health>().dead.Value) continue;
            
            Vector2 enemyPosition = player.Value.PlayerObject.gameObject.transform.position;
            float distance = Vector2.Distance(launchingPlayerPosition, enemyPosition);
            if ((closestEnemyDistance == 0) || (distance < closestEnemyDistance))
            {
                closestEnemyDistance = distance;
                closestEnemy = player.Value;
            }
        }
    }

    //To make sure that the missile is not doing anything until it is fired, this function is called one second after it has been spawned
    //Afterwards it sets the missileFired to true
    private void FireMissile()
    {
        missileFired = true;
    }

    //On each update loop it checks if the missileFired has become true before continuing. It also checks if the closestEnemy is null, in case that the player launching the missile is the only player on the server.
    //After that it gets the position of the target and its own position, setting its rotation to be towards the player
    //With that, it resets the velocity to be towards the rotated direction.
    private void Update()
    {
        if (!IsServer) return;
        if (!missileFired) return;
        if (closestEnemy == null) return;
        
        var position = closestEnemy.PlayerObject.GetComponent<NetworkTransform>().transform.position;
        var netTrans = GetComponent<NetworkTransform>().transform.position;
        Vector3 dir = (position - netTrans).normalized;
        
        _rb.velocity =  dir * missileSpeed;
    }

    //On hitting a trigger box it checks first if the game object doesn't have the player controller and also if that player is the player who fired the missile.
    //If neither is true, it deals damage to the player and despawns the missile.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer) return;
        if (!missileFired) return;
        if (!col.GetComponent<PlayerController>()) return;
        if (!col.GetComponent<NetworkObject>()) return;
        if (col.GetComponent<NetworkObject>()?.OwnerClientId == launchingPlayer.ClientId) return;
        
        
        NetworkObject networkObject = GetComponent<NetworkObject>();
        Health health = col.transform.GetComponent<Health>();
        Debug.Log("About to deal damage");
        health.TakeDamage(missileDamage);
        Debug.Log("About to despawn");
        networkObject.Despawn();
    }
}