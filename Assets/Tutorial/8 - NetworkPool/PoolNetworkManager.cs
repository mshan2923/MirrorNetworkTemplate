using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PoolNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (NetworkServer.active && spawnPrefabs.Count > 0)
        {
            var temp = GameObject.Instantiate(spawnPrefabs[0]);
            NetworkServer.Spawn(temp);
        }
    }
}
