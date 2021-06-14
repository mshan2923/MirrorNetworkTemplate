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

    Dictionary<uint, uint> ClientSpawnObjects;//Server Only - �������� PlayerID , ObjectID �� ����Ʈ�� �����ؼ� ����

    public bool RemoveObjectWhenDisconnect = true;

    //Ŭ�� �����ų�� ���������� �Ǵ°� NetManager���� , ������ ������Ʈ�� OnStopClient �����ؼ� �ᵵ �ɵ�
    #region ����ü�� ����� uint�϶� ���� ����
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
    }//�̰ž��� ��������
    */
    #endregion

    #region Start & Stop Callbacks

    public override void OnStartServer()
    {
        //if (AutoSpawn)
        //    Spawn(null);//-> ���ӻ��� Ȯ�� �Լ�

        //���⼭ �ϸ� ���� ���� �ȵ� ���¶�, Connection : 0
        // + ������ ����
    }//���� Ŭ��� ����ȵ�

    public override void OnStopServer() { }

    public override void OnStartClient()
    {
        //if (AutoSpawn)
        //    Spawn(null);//==> Error
        
    }//Ŭ�� ������ ������ ����ȵ�

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
        //OnStartServer() �� Connection ����X , Start�� �ǳ�?
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
        //Client���� NetworkServer �۵�X
    }

    [Command(requiresAuthority = false)]
    public void CallToServer(Transform trans, uint PlayerID)
    {
        {
            if (ClientSpawnObjects.ContainsKey(PlayerID))
            {
                //�̹� ������
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
            NetworkServer.Spawn(obj, NetworkServer.localConnection);//��������
        else
            NetworkServer.Spawn(obj, id.connectionToClient);

        if (id != null)
        {
            if (id.isClient)
            {
                ClientSpawnObjects.Add(PlayerID, obj.GetComponent<NetworkIdentity>().netId);
            }
        }//Ŭ�� ��������Ʈ (ClientSpawnObjects) ����

        //NetworkServer.Spawn �ϱ����� SpawnedID �����ϴϱ� ������������ (Ŭ�� ���� x)
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
    }//���߿� ������ Ŭ�󿡰� �̰ɷ� ����ȭ - ������ ���X


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