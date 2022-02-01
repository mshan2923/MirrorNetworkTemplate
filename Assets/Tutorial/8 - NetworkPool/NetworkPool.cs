using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class NetworkPool : NetworkBehaviour
{
    [System.Serializable]
    public class PoolSlot
    {
        public string Title;
        public int StartSize = 0;
        public int MaxSize = 1;
        public GameObject prefab;
        public GameObject parent;

        [SerializeField] Queue<GameObject> Lpool;//NetworkServer.(Un)Spawn 할때 NetID도 포함해서 보냄
        public Queue<GameObject> LocalPool
        {
            get => Lpool;
        }
        [SerializeField] int currentCount;
        public int ActiveCount
        {
            get => currentCount;
        }

        public void InitializePool(GameObject Parnet)
        {
            Lpool = new();
            for (int i = 0; i < StartSize; i++)
            {
                GameObject next = CreateNew();

                Lpool.Enqueue(next);
            }

            NetworkClient.UnregisterPrefab(prefab);
            NetworkClient.RegisterPrefab(prefab, SpawnHandler, UnspawnHandler);
            parent = Parnet;
        }
        GameObject CreateNew()
        {
            if (!CanSpawn()) { return null; }

            GameObject next = Instantiate(prefab);

            next.name = $"{prefab.name}_pooled_{currentCount}";
            next.transform.SetParent(parent.transform);
            next.SetActive(false);

            return next;
        }
        [Command]
        bool CanSpawn()
        {
            return currentCount < MaxSize || MaxSize < 0;
        }
        GameObject SpawnHandler(SpawnMessage msg)
        {
            NetworkDebug.CMDLog("spawn " + msg.netId);

            return GetFromPool(msg.position, msg.rotation);
        }
        void UnspawnHandler(GameObject spawned)
        {
            //NetworkDebug.CMDLog("Unspawn");

            PutBackInPool(spawned);
        }
        [Server]
        public GameObject Get()
        {
            return GetFromPool(Vector3.zero, Quaternion.identity);
        }
        GameObject GetFromPool(Vector3 Pos, Quaternion Rot)
        {
            GameObject next = Lpool.Count > 0 ? Lpool.Dequeue() : CreateNew();//로컬의 상황에 맞게 
            if (next == null) { return null; }

            if (NetworkClient.isHostClient)
            {
                NetworkServer.Spawn(next);//이걸로 클라에게 신호

                next.transform.SetPositionAndRotation(Pos, Rot);
            }

            next.transform.SetParent(parent.transform);
            next.SetActive(true);
            currentCount++;
            return next;

        }
        [Server]
        public void Return(GameObject spawned)
        {
            PutBackInPool(spawned);
        }
        void PutBackInPool(GameObject Spawnd)
        {
            //NetworkDebug.RPCLog("PutBackInPool");
            if (Lpool.Contains(Spawnd))
            {
                return;
            }

            if (NetworkClient.isHostClient)
                NetworkServer.UnSpawn(Spawnd);//이걸로 클라에게 신호

            Spawnd.transform.SetAsLastSibling();// For Debuging
            Spawnd.SetActive(false);

            Lpool.Enqueue(Spawnd);
            currentCount--;
        }
    }
    

    static NetworkPool _instance;
    public static NetworkPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<NetworkPool>();
            }
            return _instance;
        }
    }

    public List<PoolSlot> Pool = new();


    private void Start()
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            Pool[i].InitializePool(gameObject);

            if (NetworkClient.isHostClient)
            {
                //if (! NetworkManager.singleton.spawnPrefabs.Contains(Pool[i].prefab))
                {
                    //NetworkManager.singleton.spawnPrefabs.Add(Pool[i].prefab);
                }
            }else
            {              
                //CMDSyncPool(NetworkClient.localPlayer.netId, i);
                //스폰 안된 이유가 NetworkManager.spawnprefab 에 등록 안되서
            }

            //작동 안되면 Pool오브젝트가 붙인체로 스폰 안됨
        }
        //서버와 클라가 있어야 함
    }
    private void OnDestroy()
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            NetworkClient.UnregisterPrefab(Pool[i].prefab);
        }
    }
    public override void OnStartClient()
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            if (!NetworkManager.singleton.spawnPrefabs.Contains(Pool[i].prefab))
            {
                NetworkManager.singleton.spawnPrefabs.Add(Pool[i].prefab);//클라도 등록필요
            }
        }

        base.OnStartClient();
    }

    [Command(requiresAuthority = false)]
    void CMDSyncPool(uint NetID, int Index)
    {
        //print("Need Spawn : " + Pool[Index].ActiveCount + "  NetID : " + NetID);

        ForceSyncPool(NetworkServer.spawned[NetID].connectionToClient, Index, Pool[Index].ActiveCount);

        
    }
    [TargetRpc()]
    void ForceSyncPool(NetworkConnection target, int PoolIndex, int Amount)
    {
        print("Receive Need Spawn " + Amount);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NetworkPool))]
public class NetworkPoolEditor : Editor
{
    NetworkPool onwer;

    private void OnEnable()
    {
        onwer = target as NetworkPool;   
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add Pool"))
        {
            //onwer.Pool[0].Get(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 5, Quaternion.identity);
            var obj = onwer.Pool[0].Get();
            obj.GetComponent<NetworkTransform>().RpcTeleportAndRotate(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 5, Quaternion.identity);
        }
        if (GUILayout.Button("Retrun Pool"))
        {
            onwer.Pool[0].Return(onwer.gameObject.transform.GetChild(0).gameObject);
        }
    }
}
#endif