using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Test_NetManager : NetworkManager
{
    public string ServerDebug;
    NetworkConnectCheck ConnectCheck;

    public override void OnStartServer()
    {
        base.OnStartServer();

        //var temp = GameObject.Instantiate(spawnPrefabs[0]);
        //NetworkServer.Spawn(temp);


        ServerDebug += "Start Server \n";
        //여기서 스폰하면 클라가 접근해서 오류

        if (NetworkServer.active)
        {
            var temp = GameObject.Instantiate(spawnPrefabs[0]);
            NetworkServer.Spawn(temp);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        ServerDebug += ("conn : " + conn.connectionId + "\n");
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        //var temp = GameObject.Instantiate(spawnPrefabs[0]);
        //NetworkServer.Spawn(temp);// No "conn" , Use "conn.identity.connectionToClient"

        //temp.GetComponent<NetworkIdentity>().AssignClientAuthority(conn.identity.connectionToClient);

        /*
        bool temp = false;
        foreach(var connection in NetworkServer.connections)
        {
            StartCoroutine(AssignClient(connection.Value));

            temp |= (conn == connection.Value);
        }

        if (!temp)
            StartCoroutine(AssignClient( conn));//코루틴 + Server는 안되는거 같음
        */
    }

    //[ServerCallback]
    IEnumerator AssignClient( NetworkConnection conn)
    {
        if (conn.identity != null)
            Debug.Log(conn.identity + " || ConnctionToServer : " + conn.identity.connectionToServer);
        else
            Debug.Log(conn != null? ("identity is Null , Conn : " + conn.identity) : "??");

        yield return new WaitForSeconds(1f);

        if (NetworkServer.active)
        {
            var temp = GameObject.Instantiate(spawnPrefabs[0]);
            NetworkServer.Spawn(temp);

            if (conn.identity.connectionToClient != null)
                temp.GetComponent<NetworkIdentity>().AssignClientAuthority(conn.identity.connectionToClient);
            ConnectCheck = temp.GetComponent<NetworkConnectCheck>();
            ConnectCheck.DebugMessage("Spawn ConnectChecker");
            
        }
        else
        {
            Debug.Log("conn : " + conn + "  server is Not Ready");
            StartCoroutine(AssignClient(conn));
        }

        if (ConnectCheck != null)
        {
            ConnectCheck.DebugMessage("Spawn ConnectChecker");
        }
        else
        {
            Debug.Log("Not Spawn ConnectChecker");
        }

        //connectionToServer 값은 있는데 서버활성화는 안됨....
        //NetworkServer.active => false , ConnctionToServer => true
    }
    [Server]
    void SpawnToServer(NetworkConnection conn)
    {
        if (conn.identity != null)
            Debug.Log(conn.identity + " || ConnctionToServer : " + conn.identity.connectionToServer);
        else
            Debug.Log(conn != null ? ("identity is Null , Conn : " + conn.identity) : "??");

        var temp = GameObject.Instantiate(spawnPrefabs[0]);
        temp.GetComponent<NetworkIdentity>().AssignClientAuthority(NetworkServer.localConnection);

        NetworkServer.Spawn(temp, conn.identity.connectionToClient);
    }


    public void Create()
    {
        StartHost();
    }
    public void Join()
    {
        
        if (ConnectCheck != null)
        {
            ConnectCheck.RecieveToManager();
        }else
        {
            //NetworkClient.connection.clientOwnedObjects.Count

            foreach (var i in NetworkServer.spawned)
            {
                if (i.Value.gameObject.GetComponent<NetworkConnectCheck>() != null)
                    i.Value.gameObject.GetComponent<NetworkConnectCheck>().RecieveToManager();
            }

            {/*
                NetworkIdentity[] onwerobjs = new NetworkIdentity[NetworkClient.connection.clientOwnedObjects.Count];
                NetworkClient.connection.clientOwnedObjects.CopyTo(onwerobjs);

                if (onwerobjs.Length > 0)
                {
                    for (int i = 0; i < onwerobjs.Length; i++)
                    {
                        var checker = onwerobjs[i].gameObject.GetComponent<NetworkConnectCheck>();
                        if (checker != null)
                        {
                            ConnectCheck = checker;
                            checker.RecieveToManager();
                            Debug.Log(" Checker ");
                        }
                    }
                }*/
            }//Not Work - Not Setting Client Onwer Objects
        }

        StartClient();
    }

    //[Command]
    public void ToServer()
    {

    }
}
