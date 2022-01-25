using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargetRpcNetManager : NetworkManager
{
    public int Amount = 10;
    public float Distance = 5f;
    public override void OnStartServer()
    {
        base.OnStartServer();

        for(int i = 0; i < Amount; i++)
        {
            Vector3 SpawnPos = new Vector3(Random.value, 0, Random.value) * Distance;
            var Obj = GameObject.Instantiate(spawnPrefabs[0], SpawnPos, Quaternion.identity);
            NetworkServer.Spawn(Obj, NetworkServer.localConnection);


        }
    }
}
