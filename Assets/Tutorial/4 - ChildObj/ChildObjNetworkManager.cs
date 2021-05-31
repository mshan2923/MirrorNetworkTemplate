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
    }//그냥 랜덤좌표 스폰

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkIdentity[] Ids = new NetworkIdentity[(conn.clientOwnedObjects).Count];
        (conn.clientOwnedObjects).CopyTo(Ids, 0);

        var Player = conn.identity.gameObject;

        for (int i = 0; i < Ids.Length; i++)
        {
            if (Ids[i] != conn.identity)
            {
                Ids[i].RemoveClientAuthority();//==============================소유권 제거
                Ids[i].AssignClientAuthority(NetworkServer.localConnection);// 소유권 이동
            }//Connect's Main Object(Player) is't Ids[i] -> 플래이어는 소유권제거 OR 이동불가
        }

        //base.OnServerDisconnect(conn);//Must Remove Id[i]'s OwnedObject Even Moved Authority Object
        //연결 끊는 대상의 소유물은 전부제거 + 소유권 이동되어도 제거됨 , 코루틴으로 다음 프레임 기다리면 제거 안될 수도

        conn.Disconnect();
        Player.gameObject.transform.DetachChildren();
        NetworkServer.Destroy(Player.gameObject);//되긴한데 자손까지 제거됨 , 분리시키면 자손은 제거X
    }//클라이언트가 나갈때 플레이어오브젝트 빼고 전부 서버의 소유로 바꾼후 , 플레이어오브젝트의 모든 자손 분리시키고 연결끊고 제거


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

                //하위개체 네트워크 미지원
                //NetworkTransformChild Component is Not Work

                PlayerChild.GetComponent<NetworkAttach>().Parent = client.identity.gameObject;
                //PlayerChild.GetComponent<NetworkAttachTransform>().Attach();

            }
        }
    }//그냥 수동스폰 , 인게임 버튼위젯으로 스폰

}
