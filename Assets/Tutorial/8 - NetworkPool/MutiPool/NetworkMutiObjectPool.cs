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
    public int PoolAmount = 0;//0이하는 무제한

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

        //Pool에서 가져오거나 회수될때 오브젝트 활성화/비활성화 없음
    }

}
public class NetworkMutiObjectPool : NetworkBehaviour
{
    //[Header("Don't Forget Add NetworkManager.SpawnablePrefabs")]//=========문제(갑자기 고쳐짐) : 비활성화 유닛 도중 들어오면 보임(동기화 함수로) , NetID가 없는경우(클라는 아닞스폰 안해서?)
    //================= 대신 새로운 문제 : 도중에 들어온 클라는 첨에 보인거 그이상으로 스폰X
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
        //에셋 Id 으로 해당 ObjPools[index] 찾아서 가져오기
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
        //서버와 클라 이벤트 발생
        Obj.SetActive(true);//Not Sync Active Object + SetParent

        if (isServer)
        {
            //SyncClientTransform(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);//위치 동기화 
            //아직은 클라에선 스폰이 안된 상태라
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
            ClientSetActive(false, index, Obj.GetComponent<NetworkIdentity>().netId);//클라들에게 비활성화 알람
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
            ClientSetActive(false, index, NetId);//클라들에게 비활성화 알람
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
            Debug.LogWarning("NetID Is't Vaild");//전부 뜸
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
            if (NetworkServer.localConnection != NetworkClient.connection)//---사실상 의미없음
            {
                StartCoroutine(WaitForSpawn(Active, index, netID));
                WaitingForSpawn++;
            }
            else
            {
                Debug.LogAssertion("??? Server?");
            }*/

            Test(netID);//되는듯?? , 각종 설정(active, 초기 위치설정 안되나?)

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

        Debug.LogWarning("waiting : " + WaitingForSpawn);//=====도중에 들어온 클라는 첨에 보인거 그이상으로 스폰X
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
            NetworkIdentity.print("WTF?");// 아마 여기로
        }
    }//클라가 못찾은 경우 스폰이벤트 요청
}
