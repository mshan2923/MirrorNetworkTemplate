using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject SpawnObject;

    public bool AutoSpawn = true;
    public bool ClientObject_Spawn = false;

    Dictionary<uint, uint> ClientSpawnObjects;//Server Only - 서버에서 PlayerID , ObjectID 를 리스트로 저장해서 관리

    public bool RemoveObjectWhenDisconnect = true;

    //클라 연결끊킬때 소유물제거 되는건 NetManager에서 , 스폰된 오브젝트의 OnStopClient 연결해서 써도 될듯
    #region 구조체의 멤버가 uint일때 사용시 에러
    /*
    struct ClientID
    {
        public uint playerId;
        public uint objectID;
        public bool netObject;
        public ClientID(uint PlayerID, uint ObjectID, bool NetObject = true)
        {
            this.playerId = PlayerID;
            this.objectID = ObjectID;
            this.netObject = NetObject;
        }
    }//이거쓰면 에러생김
    */
    #endregion

    #region Start & Stop Callbacks

    public override void OnStartServer()
    {
        //if (AutoSpawn)
        //    Spawn(null);//-> 접속상태 확인 함수

        //여기서 하면 아직 접속 안된 상태라서, Connection : 0
        // + 서버만 실행
    }//아직 클라와 연결안됨

    public override void OnStopServer() { }

    public override void OnStartClient()
    {
        //if (AutoSpawn)
        //    Spawn(null);//==> Error
        
    }//클라가 시작은 했지만 연결안됨

    public override void OnStartLocalPlayer() { }

    public override void OnStartAuthority() { }

    public override void OnStopAuthority() { }

    #endregion

    public override void OnStopClient() 
    {

    }

    private void Start()
    {
        ClientSpawnObjects = new Dictionary<uint, uint>();

        if (AutoSpawn)
            Spawn(null);
        //OnStartServer() 는 Connection 정보X , Start는 되네?
    }

    public void Spawn(Transform trans)
    {
        
        if (isServer)
        {
            if (NetworkServer.localConnection == null)
            {
                Debug.Log("Null id : " + NetworkServer.localConnection + "| Connect : " + NetworkServer.connections.Count);
            }
            else
            {
                SpawnObjectEvent(trans, new uint());

                DebugMessage("SpawnObjectEvent_id : " + NetworkServer.localConnection + "| Connect : " + NetworkServer.connections.Count);
            }
        }
        if (isClient && NetworkClient.ready && ClientObject_Spawn)
        {
            if (NetworkClient.connection == null)
            {
                DebugMessage("CallToServer_id : " + NetworkClient.connection);
            }
            else
            {
                CallToServer(trans, ((NetworkClient.localPlayer).netId));
                //(NetworkClient.localPlayer).netId

                DebugMessage("CallToServer_id : " + NetworkClient.connection);
            }
        }
        //Client에선 NetworkServer 작동X
    }

    [Command(requiresAuthority = false)]
    public void CallToServer(Transform trans, uint PlayerID)
    {
        {
            if (ClientSpawnObjects.ContainsKey(PlayerID))
            {
                //이미 스폰됨
                DebugMessage("Already Spawn");
            }else
            {
                SpawnObjectEvent(trans, PlayerID);
            }
        }

        DebugMessage("Call Spawn Player : " + PlayerID.ToString());
    }

    [Server]
    void SpawnObjectEvent(Transform trans, uint PlayerID)
    {

        NetworkIdentity id = null;
        if (PlayerID != new uint())
        {
            if (NetworkIdentity.spawned.ContainsKey(PlayerID))
                id = NetworkIdentity.spawned[PlayerID];
        }

        var obj = GameObject.Instantiate(SpawnObject, trans);

        if (PlayerID == new uint() && id == null)
            NetworkServer.Spawn(obj, NetworkServer.localConnection);//서버껄로
        else
            NetworkServer.Spawn(obj, id.connectionToClient);

        if (id != null)
        {
            if (id.isClient)
            {
                ClientSpawnObjects.Add(PlayerID, obj.GetComponent<NetworkIdentity>().netId);
            }
        }//클라 생성리스트 (ClientSpawnObjects) 세팅

        //NetworkServer.Spawn 하기전에 SpawnedID 설정하니깐 스폰문제생김 (클라 스폰 x)
    }

    public string ClientSpawnObjectsToString()
    {
        uint[] keys = new uint[ClientSpawnObjects.Count];
        string result = null;

        ClientSpawnObjects.Keys.CopyTo(keys, 0);

        for (int i = 0; i < ClientSpawnObjects.Count; i++)
        {
            result = result + "\n" + keys[i].ToString() + " : " + ClientSpawnObjects[keys[i]].ToString();
        }

        return result;
    }
    void UpdateSpawn(uint oldId, uint newID)
    {
        if (! NetworkIdentity.spawned.ContainsKey(newID))
        {
            NetworkIdentity id = null;
            if (NetworkIdentity.spawned.TryGetValue(newID, out id))
            {
                //id.connectionToServer//===NetworkConnection
            }
            //NetworkServer.localConnection

            //CallToServer(null);
        }
    }//나중에 접속한 클라에게 이걸로 동기화 - 지금은 사용X


    #region Debuging //====================

    void DebugMessage(string data)
    {
        if (isServer)
        {
            Debug.Log(data);
            SendToAll(data);
        }
        if (isClient && NetworkClient.ready)
        {
            SendToServer(data);
        }
    }
    [Command(requiresAuthority = false)]
    void SendToServer(string data)
    {
        Debug.Log(data + "| Connect : " + NetworkServer.connections.Count + " | Server");
        SendToAll(data + "| Connect : " + NetworkServer.connections.Count + " | Server");
    }
    [ClientRpc(includeOwner = true)]
    void SendToAll(string data)
    {
        Debug.Log(data + " | Client");
    }
    #endregion
}


#if UNITY_EDITOR
[CustomEditor(typeof(NetworkSpawner))]
public class NetworkSpawnerEditor : Editor
{
    NetworkSpawner onwer;
    private void OnEnable()
    {
        onwer = target as NetworkSpawner;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Spawn"))
        {
            onwer.Spawn(null);
        }

        if (GUILayout.Button("ClientSpawnObjects"))
        {
            Debug.Log(onwer.ClientSpawnObjectsToString());
        }
    }
}
#endif