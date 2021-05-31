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

    public override void Start()
    {
        base.Start();
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        conn.identity.gameObject.transform.position += new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f)) * 2;
    }//�׳� ������ǥ ����

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkIdentity[] Ids = new NetworkIdentity[(conn.clientOwnedObjects).Count];
        (conn.clientOwnedObjects).CopyTo(Ids, 0);

        var Player = conn.identity.gameObject;

        for (int i = 0; i < Ids.Length; i++)
        {
            if (Ids[i] != conn.identity)
            {
                Ids[i].RemoveClientAuthority();//==============================������ ����
                Ids[i].AssignClientAuthority(NetworkServer.localConnection);// ������ �̵�
            }//Connect's Main Object(Player) is't Ids[i] -> �÷��̾�� ���������� OR �̵��Ұ�
        }

        //base.OnServerDisconnect(conn);//Must Remove Id[i]'s OwnedObject Even Moved Authority Object
        //���� ���� ����� �������� �������� + ������ �̵��Ǿ ���ŵ� , �ڷ�ƾ���� ���� ������ ��ٸ��� ���� �ȵ� ����

        conn.Disconnect();
        Player.gameObject.transform.DetachChildren();
        NetworkServer.Destroy(Player.gameObject);//�Ǳ��ѵ� �ڼձ��� ���ŵ� , �и���Ű�� �ڼ��� ����X
    }//Ŭ���̾�Ʈ�� ������ �÷��̾������Ʈ ���� ���� ������ ������ �ٲ��� , �÷��̾������Ʈ�� ��� �ڼ� �и���Ű�� ������� ����


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
                var PlayerChild = GameObject.Instantiate(spawnPrefabs[0]);
                PlayerChild.transform.position = client.identity.gameObject.transform.position + Vector3.forward * 1;
                NetworkServer.Spawn(PlayerChild, client);

                //������ü ��Ʈ��ũ ������
                //NetworkTransformChild Component is Not Work

                PlayerChild.GetComponent<NetworkAttach>().Parent = client.identity.gameObject;
                //PlayerChild.GetComponent<NetworkAttachTransform>().Attach();

            }
        }
    }//�׳� �������� , �ΰ��� ��ư�������� ����

}
