using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjPoolSlot
{
    public Pool<GameObject> pool;
    int index = -1;
    NetworkMutiObjectPool.Generated generate;

    public GameObject PoolObject;
    
    public int ActivePool = 0;
    public int PoolAmount = 0;//0이하는 무제한

    public void Setup(int Index, bool isServer, NetworkMutiObjectPool.Generated Generate)
    {
        index = Index;
        generate = Generate;

        if (isServer)
        {
            ServerSetUp();
        }

        Debug.Log("Setup");
    }
    [Server]
    void ServerSetUp()
    {
        pool = new Pool<GameObject>(Generator);
    }
    [Server]
    private GameObject Generator()
    {
        GameObject obj = GameObject.Instantiate(PoolObject, Vector3.zero, Quaternion.identity);

        generate.Invoke(index, obj);

        NetworkServer.Spawn(obj);
        return obj;

        //Pool에서 가져오거나 회수될때 오브젝트 활성화/비활성화 없음
    }

}
public class NetworkMutiObjectPool : NetworkBehaviour
{
    //Auto Added NetworkManager.SpawnablePrefabs
    NetworkManager networkManager;
    public List<ObjPoolSlot> ObjPools = new List<ObjPoolSlot>();
    public delegate void Generated(int index, GameObject Obj);
    Generated generated;

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < ObjPools.Count; i++)
        {
            ObjPools[i].Setup(i, isServer, generated);

            if (!networkManager.spawnPrefabs.Exists(t => t == ObjPools[i].PoolObject))
                networkManager.spawnPrefabs.Add(ObjPools[i].PoolObject);
        }

    }
    public void OnEnable()
    {
        networkManager = GameObject.FindObjectOfType<NetworkManager>();
        generated = new Generated(RespawnEvent);
    }


    public virtual void RespawnEvent(int index, GameObject Obj)
    {
        //서버와 클라 이벤트 발생
        Obj.SetActive(true);//Not Sync Active Object + SetParent

        if (isServer)
        {
            SyncClientTransform(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);//위치 동기화 
        }
    }

    [Server]
    public GameObject Command_Spawn(int index)
    {
        GameObject obj = null;

        if (ObjPools[index].ActivePool < ObjPools[index].PoolAmount || ObjPools[index].PoolAmount < 0)
        {
            obj = ObjPools[index].pool.Take();
            RespawnEvent(index, obj);
            ClientSetActive(true, index, obj.GetComponent<NetworkIdentity>().netId);//클라들에게 리스폰이벤트 뿌리기
            ObjPools[index].ActivePool++;
        }

        return obj;
    }
    [Server]
    public void Command_DeSpawn(int index, GameObject Obj)
    {
        if (ObjPools[index].ActivePool > 0)
        {
            ObjPools[index].pool.Return(Obj);
            ClientSetActive(false, index, Obj.GetComponent<NetworkIdentity>().netId);//클라들에게 비활성화 알람
            ObjPools[index].ActivePool--;
        }
    }


    [ClientRpc(includeOwner = true)]
    public void SyncClientTransform(uint netID, Vector3 Pos, Quaternion Rot)
    {
        if (NetworkIdentity.spawned.ContainsKey(netID))
        {
            var obj = NetworkIdentity.spawned[netID].gameObject;
            obj.transform.position = Pos;
            obj.transform.rotation = Rot;
        }
    }
    [ClientRpc(includeOwner = true)]
    public void ClientSetActive(bool Active, int index, uint netID)
    {
        var id = NetworkIdentity.spawned[netID];

        if (Active)
        {
            RespawnEvent(index, id.gameObject);
        }
        else
        {
            id.gameObject.SetActive(false);
        }
    }
}
