using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;

public class NetworkSpawnManager : NetworkBehaviour
{
    [System.Serializable]
    public class SpawnSlot
    {
        public string Title;
        public GameObject SpawnObject;
        Queue<GameObject> Pool = new Queue<GameObject>();
        public List<GameObject> ActiveList = new List<GameObject>();//(Object.NetID) - Server Only
        public int ActiveAmount = 0;
        public int MaxSpawnAmount = 1;//0�̸� �̸� ������

        public bool AutoResize = false;//���϶� �ִ�ġ �޼��� ���ο� Ǯ ����
        public bool AutoSpawn = false;//�����Ҷ� 1�� ����
        public bool ClientSpawn = true;

        [Server]
        public bool Spawn(out GameObject Obj)
        {
            if (ActiveAmount < MaxSpawnAmount) 
            {
                GameObject Lobj = null;
                if (Pool.Count > 0)
                {
                    Lobj = Pool.Dequeue();
                    if (Lobj == null)
                    {
                        NetworkIdentity.print("UnVaild Pool Object");
                        Lobj = GameObject.Instantiate(SpawnObject, Vector3.zero, Quaternion.identity);
                    }
                }else
                {
                    Lobj = GameObject.Instantiate(SpawnObject, Vector3.zero, Quaternion.identity);
                }

                Obj = Lobj;
                // Object.netID - ���� ���� �������� �ʾ� ������ 0

                ActiveList.Add(Lobj);
                Obj.SetActive(true);
                ActiveAmount++;
                if (ClientSpawn)
                {
                    NetworkServer.Spawn(Obj, Obj.GetComponent<NetworkIdentity>().assetId, NetworkServer.localConnection);
                }

                return true;
            }else
            {
                if (AutoResize)
                {
                    Obj = GameObject.Instantiate(SpawnObject, Vector3.zero, Quaternion.identity);
                    ActiveList.Add(Obj);
                    Obj.SetActive(true);

                    MaxSpawnAmount++;
                    ActiveAmount = MaxSpawnAmount;

                    if (ClientSpawn)
                    {
                        NetworkServer.Spawn(Obj, Obj.GetComponent<NetworkIdentity>().assetId, NetworkServer.localConnection);
                    }

                    return true;
                }

                Obj = null;
                return false;
            }
        }
        [Server]
        public void Despawn(GameObject Obj) 
        {
            Pool.Enqueue(Obj);

            ActiveList.Remove(Obj);
            Obj.SetActive(false);
            ActiveAmount--;
            //NetworkServer.Destroy(Obj);//=====================������ ���ŵǼ�...
            NetworkServer.UnSpawn(Obj);//....�׳� �̰ɷ��ϸ� ��Ȱ��ȭ ��...  ��������
        }
    }

    public List<SpawnSlot> Spawns = new List<SpawnSlot>();
    public bool DebugSpawnPos = false;//Ȱ��ȭ���� ���� ��ġ���� �̹Ƿ� ���ļ� ���� ����

    public delegate void SpawnDelegate(int Index, GameObject Obj);
    public SpawnDelegate SpawnEvent;

    private void OnEnable()
    {
        for (int i = 0; i < Spawns.Count; i++)
        {
            NetworkManager.singleton.spawnPrefabs.Add(Spawns[i].SpawnObject);//�ڵ� ���
        }
    }

    void Start()
    {
        //OnStartServer, OnStartClient �� ���� ����ȵ� ����

        for (int i = 0; i < Spawns.Count; i++)
        {
            if (isServer)
            {
                if (Spawns[i].AutoSpawn)
                {
                    Spawn(i);
                }
            }

            {
                /*
                var LassetID = Spawns[i].SpawnObject.GetComponent<NetworkIdentity>().assetId;//System.Guid.NewGuid();
                    NetworkClient.RegisterPrefab(Spawns[i].SpawnObject, LassetID ,Spawns[i].TestSpawnEvent, Spawns[i].TestDespawnEvent);
                //NetworkClient.RegisterSpawnHandler(LassetID, Spawns[i].TestSpawnEvent, Spawns[i].TestDespawnEvent);
                */
            }// Not Work (RegisterPrefab, RegisterSpawnHandler)
        }
    }

    [Server]
    public virtual void DefaultTransform(int index, GameObject Obj)
    {
        if (Spawns[index].ActiveAmount > 0 && Obj != null && DebugSpawnPos)//ù��°�� �ƴϰ� + ��ȿ + DebugSpawnPos
            Obj.transform.position = Vector3.right * (Spawns[index].ActiveAmount - 1);

        if (SpawnEvent != null)
        {
            SpawnEvent.Invoke(index, Obj);
        }
    }


    [Server]
    public void Spawn(int index) 
    {
        Spawns[index].Spawn(out GameObject obj);
        DefaultTransform(index, obj);
    }

    [Server]
    public void Despawn(int index, GameObject obj)
    {
        Spawns[index].Despawn(obj);
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(NetworkSpawnManager))]
public class NetworkSpawnManagerEditor : Editor
{
    NetworkSpawnManager Onwer;

    public override void OnInspectorGUI()
    {
        Onwer = target as NetworkSpawnManager;
        base.OnInspectorGUI();

        if (GUILayout.Button("Set SpawnObject"))
        {
            var netManager = FindObjectOfType<NetworkManager>();

            netManager.spawnPrefabs.Add(Onwer.gameObject);

            for (int i = 0; i < Onwer.Spawns.Count; i++)
            {
                //NetworkManager.singleton.spawnPrefabs.Add(Onwer.Spawns[i].SpawnObject);//�ΰ��ӿ���

                if (! netManager.spawnPrefabs.Exists(t=> t == Onwer.Spawns[i].SpawnObject))
                {
                    netManager.spawnPrefabs.Add(Onwer.Spawns[i].SpawnObject);
                }
            }
        }
        EditorGUILayout.HelpBox("Must Add NetManager.SpawnPrefabs", MessageType.Info);

    }
}
#endif