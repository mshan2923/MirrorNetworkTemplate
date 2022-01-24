using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Mirror;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class ChildObjNetworkManager : NetworkManager
{
    public float SpawnRadius = 2f;

    //=============================== ���� �߻�!!  Ŭ�� ��ġ �����ȵ� , �ڽ� ������Ʈ�� �ȵ�

    public override void Start()
    {
        base.Start();
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        //conn.identity.gameObject.transform.position += new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f)) * SpawnRadius;
        Vector3 Rpos = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * SpawnRadius;

        conn.identity.GetComponent<NetworkTransform>().RpcTeleportAndRotate(Rpos, Random.rotation);

        //�������� ��ġ�� �����ؾ��� , �и� ������ �ڵ� ����ȭ�Ǿ��µ�?
        //���޾��ϸ�  Ŭ�� ������ ���� ���

    }//�׳� ������ǥ ����

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkIdentity[] Ids = new NetworkIdentity[(conn.clientOwnedObjects).Count];
        conn.clientOwnedObjects.CopyTo(Ids, 0);

        GameObject Player = conn.identity.gameObject;

        /*
        for (int i = 0; i < Ids.Length; i++)
        {
            if (Ids[i] != conn.identity)
            {
                Ids[i].RemoveClientAuthority();//������ ����
                Ids[i].AssignClientAuthority(NetworkServer.localConnection);// ������ �̵�
            }//Connect's Main Object(Player) is't Ids[i] -> �÷��̾�� ���������� OR �̵��Ұ�
        }*/

        //base.OnServerDisconnect(conn);//Must Remove Id[i]'s OwnedObject Even Moved Authority Object
        //�������� ���� �ϸ� �������ѵ� //���� ���� ����� �������� �������� + ������ �̵��Ǿ ���ŵ� , �ڷ�ƾ���� ���� ������ ��ٸ��� ���� �ȵ� ����

        /*
        if (Player.GetComponentInChildren<IAttach>() != null)
            Player.GetComponentInChildren<IAttach>().Detach();
        else
            print("Not Detached");
        */

        //======================================�и� ���ϰ� Ŭ�� ������ ���� (������ Ŭ���� ���� ������Ʈ�� ã������) - �������̵� ����
        //���� ���� ��ư�� ���� �ؾ��ϳ�

        conn.Disconnect();

        NetworkServer.Destroy(Player);//    ���� :  �Ǳ��ѵ� �ڼձ��� ���ŵ� , �и���Ű�� �ڼ��� ����X

        //base.OnServerDisconnect(conn);



        //�ڷ�ƾ���� ������ ������ �̵� ����
    }

    [Server]
    public void SpawnChild()
    {
        int keyAmount = NetworkServer.connections.Keys.Count;
        Debug.Log("connections :" + keyAmount);

        var key = NetworkServer.connections.Keys;
        int[] keyArray = new int[keyAmount];
        key.CopyTo(keyArray, 0);

        for (int i = 0; i < keyAmount; i++)
        {          

            var client = NetworkServer.connections[keyArray[i]];
            if (client != null)
            {

                var PlayerObj = client.identity.gameObject;
                var PlayerChild = GameObject.Instantiate(spawnPrefabs[0]);
                NetworkServer.Spawn(PlayerChild, client);

                PlayerChild.GetComponent<NetworkAttach>().Attach(PlayerObj, (Vector3.forward * 1), Quaternion.identity);
            }
        }
    }//�׳� �������� , �ΰ��� ��ư�������� ����
    public void DetachAllChildren()
    {
        foreach (var data in NetworkServer.connections)
        {
            //NetworkServer.connections[data.Key].identity.gameObject.GetComponentInChildren<IAttach>().Detach();
            var Lattach = NetworkServer.connections[data.Key].identity.gameObject.GetComponentInChildren<IAttach>();
            if (Lattach != null)
            {
                Lattach.Detach();
            }
            NetworkManager.print(NetworkServer.connections[data.Key].identity.netId + " - Detach" + " | Count : " + NetworkServer.connections.Count);
        }
    }
    public void DetachNDisconnect()
    {
        List<NetworkConnectionToClient> clients = new();

        foreach (var data in NetworkServer.connections)
        {
            for (int i = 0; i < NetworkServer.connections[data.Key].identity.gameObject.transform.childCount; i++)
            {
                var ChildObj = NetworkServer.connections[data.Key].identity.gameObject.transform.GetChild(i).gameObject;

                ChildObj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                //ChildObj.GetComponent<NetworkIdentity>().AssignClientAuthority(NetworkServer.localConnection);
            }

            var Lattach = NetworkServer.connections[data.Key].identity.gameObject.GetComponentInChildren<IAttach>();
            if (Lattach != null)
            {
                if (!data.Value.identity.isLocalPlayer)
                {
                    NetworkServer.connections[data.Key].identity.gameObject.GetComponentInChildren<NetworkAttach>().OnCompleteDetach
                        += new NetworkAttach.CompleteDetach(OnCompleteDetach);
                }
            }
        }

        NetworkConnectionToClient[] Nctc = new NetworkConnectionToClient[NetworkServer.connections.Values.Count];
        NetworkServer.connections.Values.CopyTo(Nctc, 0);

        for (int j = 0; j < Nctc.Length; j++)
        {
            var Lattach = Nctc[j].identity.gameObject.GetComponentInChildren<NetworkAttach>();
            if (Lattach)
            {
                Lattach.Detach();
            }else
            {
                //Nctc[j].Disconnect();   
            }
        }//foreach �ȿ� ������ ���� ���ܼ� - ����Ʈ ����
    }

    void OnCompleteDetach(NetworkAttach attach, uint ParentID)//=====================DetachNDisconnect �� foreach �߿� ��� Ŭ�� ������ܼ� ����
    {
        NetworkManager.print("OnCompleteDetach / " + attach.netIdentity + " / " + attach.netIdentity.connectionToClient + "\n" + ParentID);
        //attach.netIdentity.connectionToClient == Null
        //������ NetAttach�� �θ� NetID�� ������ �� ������ (NetAttach�� Line 127)

        //attach.netIdentity.connectionToClient.Disconnect();//Not Work
        if (ParentID != uint.MinValue)
            NetworkServer.spawned[ParentID].connectionToClient.Disconnect();
        else
            NetworkManager.print("Cencle to Disconnect");
    }
}
