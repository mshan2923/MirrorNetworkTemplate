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

    //=============================== 문제 발생!!  클라 위치 업뎃안됨 , 자식 오브젝트도 안됨

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

        //서버에서 위치를 전달해야함 , 분명 전에는 자동 동기화되었는데?
        //전달안하면  클라가 서버의 값을 덮어씀

    }//그냥 랜덤좌표 스폰

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
                Ids[i].RemoveClientAuthority();//소유권 제거
                Ids[i].AssignClientAuthority(NetworkServer.localConnection);// 소유권 이동
            }//Connect's Main Object(Player) is't Ids[i] -> 플래이어는 소유권제거 OR 이동불가
        }*/

        //base.OnServerDisconnect(conn);//Must Remove Id[i]'s OwnedObject Even Moved Authority Object
        //수동으로 끊고 하면 괜찮긴한데 //연결 끊는 대상의 소유물은 전부제거 + 소유권 이동되어도 제거됨 , 코루틴으로 다음 프레임 기다리면 제거 안될 수도

        /*
        if (Player.GetComponentInChildren<IAttach>() != null)
            Player.GetComponentInChildren<IAttach>().Detach();
        else
            print("Not Detached");
        */

        //======================================분리 안하고 클라가 나갈때 에러 (서버가 클라의 붙인 오브젝트를 찾지못함) - 소유권이동 문제
        //연결 끊기 버튼을 따로 해야하나

        conn.Disconnect();

        NetworkServer.Destroy(Player);//    현제 :  되긴한데 자손까지 제거됨 , 분리시키면 자손은 제거X

        //base.OnServerDisconnect(conn);



        //코루틴으로 연결끊어도 소유권 이동 에러
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
    }//그냥 수동스폰 , 인게임 버튼위젯으로 스폰
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
        }//foreach 안에 있으면 오류 생겨서 - 리스트 변경
    }

    void OnCompleteDetach(NetworkAttach attach, uint ParentID)//=====================DetachNDisconnect 의 foreach 중에 모든 클라 연결끊겨서 에러
    {
        NetworkManager.print("OnCompleteDetach / " + attach.netIdentity + " / " + attach.netIdentity.connectionToClient + "\n" + ParentID);
        //attach.netIdentity.connectionToClient == Null
        //변경전 NetAttach의 부모 NetID를 가져올 수 있으니 (NetAttach의 Line 127)

        //attach.netIdentity.connectionToClient.Disconnect();//Not Work
        if (ParentID != uint.MinValue)
            NetworkServer.spawned[ParentID].connectionToClient.Disconnect();
        else
            NetworkManager.print("Cencle to Disconnect");
    }
}
