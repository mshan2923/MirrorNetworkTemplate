using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class NetworkPool : NetworkBehaviour
{
    #region Disable
    /*
    [System.Serializable]
    public class PoolManageSlot
    {
        public string TitleName;
        public int ActiveAmount = 0;
        public int SpawnAmount = 1;// 0 ���ϴ� ������ ����

        int _onwerID = 0;
        /// <summary>
        /// Connection ID
        /// </summary>
        public int OnwerID// �⺻���� ��������
        {
            get => _onwerID;
            set
            {
                _onwerID = value;

                ChangeOnwer();//���� �Լ�
            }
        }

        /// <summary>
        /// Need NetworkIdentity Component
        /// </summary>
        [SerializeField]
        GameObject SpawnObject;

        List<uint> ActiveObjects = new List<uint>();
        public List<uint> Activates
        {
            get => ActiveObjects;
        }
        Stack<uint> DeactiveObjects = new();

        void ChangeOnwer()
        {
            //ActiveObjects�� ������ �̵�
        }

        [Server]
        public uint Get()
        {
            if (SpawnObject != null)
            {
                if (SpawnAmount <= 0 || ActiveObjects.Count < SpawnAmount)
                {
                    if (DeactiveObjects.Count > 0)
                    {
                        var objID = NetworkServer.spawned[DeactiveObjects.Pop()];
                        SyncPoolObjectEnable(objID.netId, true);
                        //objID.visible = Visibility.Default;
                        //objID.enabled = true;

                        ActiveAmount++;
                        ActiveObjects.Add(objID.netId);

                        return objID.netId;
                    }
                    else
                    {
                        var obj = GameObject.Instantiate(SpawnObject);

                        if (OnwerID > 0)
                        {
                            NetworkServer.Spawn(obj, NetworkServer.connections[OnwerID]);
                        }
                        else
                        {
                            NetworkServer.Spawn(obj, NetworkServer.localConnection);
                        }


                        NetworkIdentity spawnedID = obj.GetComponent<NetworkIdentity>();
                        if (spawnedID == null)
                        {
                            NetworkServer.UnSpawn(obj);
                            return 0;
                        }
                        else
                        {
                            ActiveObjects.Add(spawnedID.netId);

                            NetworkDebug.RPCLog(spawnedID.netId + " + Create Pool");

                            ActiveAmount++;

                            return spawnedID.netId;
                        }
                    }
                }
            }
            return 0;
        }
        [Server]
        public bool Return(uint ID)
        {
            if (ActiveObjects.Contains(ID))
            {
                //var obj = NetworkServer.spawned[ID].gameObject;
                SyncPoolObjectEnable(ID, false);
                //NetworkServer.spawned[ID].visible = Visibility.ForceHidden;
                //NetworkServer.spawned[ID].enabled = false;

                ActiveAmount--;
                ActiveObjects.Remove(ID);
                DeactiveObjects.Push(ID);

                return true;
            }

            NetworkManager.print("Fail to Return");

            return false;
        }

        [ClientRpc(includeOwner = true)]
        void SyncPoolObjectEnable(uint ID, bool enable)
        {
            var obj = NetworkClient.spawned[ID].gameObject;

            Debug.Log(ID + " + " + enable + " - SyncPoolObject Enable");
            NetworkDebug.CMDLog(ID + " + " + enable);

            NetworkClient.spawned[ID].visible = enable ? Visibility.ForceShown : Visibility.ForceHidden;
            NetworkClient.spawned[ID].enabled = enable;

            obj.SetActive(enable);
        }//============================���� ... RPC�� �Ǵ°� �¾�??
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

    //=====���߿� ���� ������Ʈ Ȱ��ȭ��? , ConnectionID �� 0���� �����ΰ�? 
    //=====NetworkManager.spawnable Prefab �ڵ���� , (��)Ȱ��ȭ ���� ���� �ȵ�

    public List<PoolManageSlot> Pool = new List<PoolManageSlot>();

    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    */
    #endregion
    [System.Serializable]
    public class PoolSlot
    {
        public string Title;
        public int StartSize = 0;
        public int MaxSize = 1;
        public GameObject prefab;
        public GameObject parent;

        [SerializeField] Queue<GameObject> Lpool;//NetworkServer.(Un)Spawn �Ҷ� NetID�� �����ؼ� ����
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
            //NetworkDebug.CMDLog("spawn");

            return GetFromPool(msg.position, msg.rotation);
        }
        void UnspawnHandler(GameObject spawned)
        {
            //NetworkDebug.CMDLog("Unspawn");

            PutBackInPool(spawned);
        }
        public GameObject GetFromPool(Vector3 Pos, Quaternion Rot)
        {
            GameObject next = Lpool.Count > 0 ? Lpool.Dequeue() : CreateNew();//������ ��Ȳ�� �°� 
            if (next == null) { return null; }

            if (NetworkClient.isHostClient)
            {
                NetworkServer.Spawn(next);//�̰ɷ� Ŭ�󿡰� ��ȣ

                next.transform.position = Pos;
                next.transform.rotation = Rot;
            }

            next.transform.SetParent(parent.transform);
            next.SetActive(true);
            currentCount++;
            return next;

        }
        public void PutBackInPool(GameObject Spawnd)
        {
            //NetworkDebug.RPCLog("PutBackInPool");

            if (NetworkClient.isHostClient)
                NetworkServer.UnSpawn(Spawnd);//�̰ɷ� Ŭ�󿡰� ��ȣ

            Spawnd.transform.SetAsLastSibling();//============================== For Debuging
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
            //�۵� �ȵǸ� Pool������Ʈ�� ����ü�� ���� �ȵ�
        }
        //������ Ŭ�� �־�� ��
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    private void OnDestroy()
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            NetworkClient.UnregisterPrefab(Pool[i].prefab);
        }
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

        #region Disable
        /*
        if (GUILayout.Button("Add Pool"))
        {
            uint objId = onwer.Pool[0].Get();

            NetworkServer.spawned[objId].gameObject.transform.position = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 5;
        }
        if (GUILayout.Button("Return Pool"))
        {
            if (onwer.Pool[0].Activates.Count > 0)
            {
                onwer.Pool[0].Return(onwer.Pool[0].Activates[0]);
            }
        }
        */
        #endregion 

        if (GUILayout.Button("Add Pool"))
        {
            onwer.Pool[0].GetFromPool(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 5, Quaternion.identity);
        }
        if (GUILayout.Button("Retrun Pool"))
        {
            onwer.Pool[0].PutBackInPool(onwer.gameObject.transform.GetChild(0).gameObject);
        }
    }
}
#endif