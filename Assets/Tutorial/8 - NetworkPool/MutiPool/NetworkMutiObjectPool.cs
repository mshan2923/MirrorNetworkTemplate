using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjPoolSlot
{
    public Pool<GameObject> pool;
    public List<GameObject> PoolList = new List<GameObject>();
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

    }
    [Server]
    void ServerSetUp()
    {
        pool = new Pool<GameObject>(Generator, 0);
    }
    [Server]
    private GameObject Generator()
    {
        GameObject obj = GameObject.Instantiate(PoolObject, Vector3.zero, Quaternion.identity);

        generate.Invoke(index, obj);

        NetworkServer.Spawn(obj, NetworkServer.localConnection);
        return obj;

        //Pool���� �������ų� ȸ���ɶ� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ ����
    }

}
public class NetworkMutiObjectPool : NetworkBehaviour
{
    //[Header("Don't Forget Add NetworkManager.SpawnablePrefabs")]//=========����(���ڱ� ������) : ��Ȱ��ȭ ���� ���� ������ ����(����ȭ �Լ���) , NetID�� ���°��(Ŭ��� �ƈ����� ���ؼ�?)
    //================= ��� ���ο� ���� : ���߿� ���� Ŭ��� ÷�� ���ΰ� ���̻����� ����X
    NetworkManager networkManager;
    public List<ObjPoolSlot> ObjPools = new List<ObjPoolSlot>();
    public delegate void Generated(int index, GameObject Obj);
    Generated generated;

    public int WaitingForSpawn = 0;

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < ObjPools.Count; i++)
        {
            ObjPools[i].Setup(i, isServer, generated);

            if (!networkManager.spawnPrefabs.Exists(t => t == ObjPools[i].PoolObject))
                networkManager.spawnPrefabs.Add(ObjPools[i].PoolObject);

            //NetworkClient.RegisterPrefab(ObjPools[i].PoolObject, SpawnPool, DespawnPool);
            NetworkClient.UnregisterSpawnHandler(ObjPools[i].PoolObject.GetComponent<NetworkIdentity>().assetId);
            //NetworkClient.UnregisterPrefab(ObjPools[i].PoolObject);

            NetworkClient.RegisterSpawnHandler(ObjPools[i].PoolObject.GetComponent<NetworkIdentity>().assetId, SpawnPool, DespawnPool);
        }

    }
    public void OnEnable()
    {
        networkManager = GameObject.FindObjectOfType<NetworkManager>();
        generated = new Generated(RespawnEvent);

        for (int i = 0; i < ObjPools.Count; i++)
        {
            if (!networkManager.spawnPrefabs.Exists(t => t == ObjPools[i].PoolObject))
                networkManager.spawnPrefabs.Add(ObjPools[i].PoolObject);
        }

    }

    public GameObject SpawnPool(SpawnMessage message)
    {
        //���� Id ���� �ش� ObjPools[index] ã�Ƽ� ��������
        //message.assetId
        int Lindex = ObjPools.FindIndex(v => v.PoolObject.GetComponent<NetworkIdentity>().assetId == message.assetId);

        Debug.Log("Testing Custon Spawn Fuctions - " + Lindex);
        NetworkIdentity.print("Testing Custon Spawn Fuctions - " + Lindex);
        return null;
    }
    public void DespawnPool(GameObject Obj)
    {
        int Lindex = ObjPools.FindIndex(v => v.PoolObject.GetComponent<NetworkIdentity>().assetId == Obj.GetComponent<NetworkIdentity>().assetId);
        if (Lindex >= 0)
        {
            ObjPools[Lindex].pool.Return(Obj);
            ObjPools[Lindex].PoolList.Remove(Obj);
            ObjPools[Lindex].ActivePool--;

            ClientSetActive(false, Lindex, Obj.GetComponent<NetworkIdentity>().netId);

            NetworkIdentity.print("Testing Custon DeSpawn Fuctions - " + Lindex);
        }
    }

    public virtual void RespawnEvent(int index, GameObject Obj)
    {
        //������ Ŭ�� �̺�Ʈ �߻�
        Obj.SetActive(true);//Not Sync Active Object + SetParent

        if (isServer)
        {
            //SyncClientTransform(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);//��ġ ����ȭ 
            //������ Ŭ�󿡼� ������ �ȵ� ���¶�
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

            ObjPools[index].PoolList.Add(obj);
            
            NetworkServer.Spawn(obj, ObjPools[index].PoolObject.GetComponent<NetworkIdentity>().assetId, NetworkServer.localConnection);
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

            ObjPools[index].PoolList.Remove(Obj);

            NetworkServer.Destroy(Obj);
        }
    }
    [Server]
    public void Command_DeSpawn(int index, uint NetId)
    {
        if (!NetworkServer.spawned.ContainsKey(NetId))
            Debug.LogWarning("NetID is't Vaild");

        GameObject Obj = NetworkServer.spawned[NetId].gameObject;

        if (ObjPools[index].ActivePool > 0)
        {
            ObjPools[index].pool.Return(Obj);
            ClientSetActive(false, index, NetId);//Ŭ��鿡�� ��Ȱ��ȭ �˶�
            ObjPools[index].ActivePool--;

            ObjPools[index].PoolList.Remove(Obj);
        }
    }


    [ClientRpc(includeOwner = true)]
    public void SyncClientTransform(uint netID, Vector3 Pos, Quaternion Rot)
    {
        if (NetworkServer.spawned.ContainsKey(netID))
        {
            var obj = NetworkServer.spawned[netID].gameObject;
            obj.transform.position = Pos;
            obj.transform.rotation = Rot;
        }else
        {
            Debug.LogWarning("NetID Is't Vaild");//���� ��
        }
    }
    [ClientRpc(includeOwner = true)]
    public void ClientSetActive(bool Active, int index, uint netID)
    {
        if (NetworkServer.spawned.ContainsKey(netID))
        {
            var id = NetworkServer.spawned[netID];

            if (Active)
            {
                RespawnEvent(index, id.gameObject);
            }
            else
            {
                id.gameObject.SetActive(false);
            }
        }else
        {
            /*
            if (NetworkServer.localConnection != NetworkClient.connection)//---��ǻ� �ǹ̾���
            {
                StartCoroutine(WaitForSpawn(Active, index, netID));
                WaitingForSpawn++;
            }
            else
            {
                Debug.LogAssertion("??? Server?");
            }*/

            Test(netID);//�Ǵµ�?? , ���� ����(active, �ʱ� ��ġ���� �ȵǳ�?)

            if (NetworkServer.spawned.ContainsKey(netID))
            {
                var id = NetworkServer.spawned[netID];

                if (Active)
                {
                    RespawnEvent(index, id.gameObject);
                }
                else
                {
                    id.gameObject.SetActive(false);
                }

                NetworkIdentity.print("Exist netID");//--xxx
            }
            else
            {
                NetworkIdentity.print("Not Exist netID");
            }
        }
    }

    public IEnumerator WaitForSpawn(bool Active, int index, uint netID)
    {
        yield return new WaitForEndOfFrame();

        Debug.LogWarning("waiting : " + WaitingForSpawn);//=====���߿� ���� Ŭ��� ÷�� ���ΰ� ���̻����� ����X
        if (NetworkServer.spawned.ContainsKey(netID))
        {
            WaitingForSpawn--;
            ClientSetActive(Active, index, netID);
        }
        else
        {
            StartCoroutine(WaitForSpawn(Active, index, netID));
        }
    }

    [Command(requiresAuthority = false)]
    public void Test(uint netID)
    {
        if (NetworkServer.spawned.ContainsKey(netID))
        {
            NetworkServer.Spawn(NetworkServer.spawned[netID].gameObject, NetworkServer.localConnection);

            NetworkIdentity.print("Recall Spawn");//---xxxxx
        }
        else
        {
            NetworkIdentity.print("WTF?");// �Ƹ� �����
        }
    }//Ŭ�� ��ã�� ��� �����̺�Ʈ ��û
}
