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
    public int PoolAmount = 0;//0���ϴ� ������

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

        //Pool���� �������ų� ȸ���ɶ� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ ����
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
        //������ Ŭ�� �̺�Ʈ �߻�
        Obj.SetActive(true);//Not Sync Active Object + SetParent

        if (isServer)
        {
            SyncClientTransform(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);//��ġ ����ȭ 
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
            ClientSetActive(true, index, obj.GetComponent<NetworkIdentity>().netId);//Ŭ��鿡�� �������̺�Ʈ �Ѹ���
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
            ClientSetActive(false, index, Obj.GetComponent<NetworkIdentity>().netId);//Ŭ��鿡�� ��Ȱ��ȭ �˶�
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
