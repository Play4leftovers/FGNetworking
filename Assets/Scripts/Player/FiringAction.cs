using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FiringAction : NetworkBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject clientSingleBulletPrefab;
    [SerializeField] GameObject serverSingleBulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;

    //I added on three variables to keep track of ammo. The first is a simple rule on how much each ship will have
    //Second keeps track on the local ammo for use in UI
    //Third is the server keeping track on the proper amount of ammo
    public int startingAmmo = 10;
    int _localAmmo;
    public NetworkVariable<int> ammo = new NetworkVariable<int>();
    
    public override void OnNetworkSpawn()
    {
        _localAmmo = startingAmmo;
        ammo.OnValueChanged += LocalAmmoCount;
        playerController.onFireEvent += Fire;
        
        //It keeps a lookout on that if the network ammo changes, the local ammo should change with it to keep the same value at all time.
        //I use an event listener for this
        if (!IsServer) return;
        ammo.Value = startingAmmo;
    }

    private void Fire(bool isShooting)
    {
        if (isShooting)
        {
            ShootLocalBullet();
        }
    }

    [ServerRpc]
    private void ShootBulletServerRpc()
    {
        //After the local bullet has been fired it actually makes sure the player really has enough ammo to do so on the server
        //If they do, they subtract one ammo from the network variable
        if (ammo.Value <= 0) return;
        ammo.Value--;
        GameObject bullet = Instantiate(serverSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
        ShootBulletClientRpc();
    }

    [ClientRpc]
    private void ShootBulletClientRpc()
    {
        if (IsOwner) return;
        GameObject bullet = Instantiate(clientSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
    }

    private void ShootLocalBullet()
    {
        //At first it only checks locally to make sure that when you fire it spawns a dummy bullet, so to keep networking to a minimum, it only has to make sure that the local ammo is above zero
        //I prefer using a guard clause for most things like this
        if(_localAmmo <= 0) return;
        GameObject bullet = Instantiate(clientSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
        
        ShootBulletServerRpc();
    }

    //When the network variable changes, the local ammo is set to be equal to the network ammo.
    private void LocalAmmoCount(int prevValue, int newValue)
    {
        _localAmmo = newValue;
    }
}
