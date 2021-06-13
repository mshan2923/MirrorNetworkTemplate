using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Test_Delegate//조금 작업하고 object으로 전환
{
    GameObject Caller;
    string Data;

    public delegate void VauleDeleget(string data);
    public VauleDeleget vauleDeleget;

    public delegate bool VaildDelegate(GameObject caller, string data);//클라가 변경될때 서버에서 유효값 판별
    public VaildDelegate vaildDelegate;

    public delegate bool VisibleDelegate(GameObject caller);//클라가 특정조건에 맞는 조건이 맞는지
    public VisibleDelegate visibleDelegate;

    struct Test_Dele_ToClient : NetworkMessage
    {
        public string data;

        public Test_Dele_ToClient(string Data)
        {
            data = Data;
        }
    }

    public Test_Delegate(GameObject caller, VauleDeleget vaule, VaildDelegate vaild, VisibleDelegate visible)
    {
        Caller = caller;
        vauleDeleget = vaule;
        vaildDelegate = vaild;
        visibleDelegate = visible;

        NetworkClient.RegisterHandler<Test_Dele_ToClient>(RecievfromServer, false);
        //NetworkServer.RegisterHandler<Test_Dele_ToClient>(RecievfromServer);
    }

    [Client]
    public void Set(string data)
    {
        //Data = data;
        SendToServer(Caller, data);
    }

    [Command(requiresAuthority = false)]
    public void SendToServer(GameObject ClientCaller, string ClientData)
    {
        Debug.Log("Server - Sender : " + ClientCaller + " | " + ClientData);
        bool result = vaildDelegate(ClientCaller, ClientData);

        if (result)
        {
            SendDataToClient(ClientCaller, ClientData);
        }
        // 여기까지 타입 접근가능 하니깐
    }

    [Server]
    void SendDataToClient(GameObject ClientCaller, string ClientData)
    {
        int[] Keys = new int[NetworkServer.connections.Keys.Count];
        NetworkServer.connections.Keys.CopyTo(Keys, 0);
        Debug.Log("Connections : " + Keys.Length + " == " + NetworkServer.connections.Count + " | Active : " + NetworkServer.active);//---------연결되도 0 ...?// 0 == 0 이라는데??
                                                                                                                                     // 0 == 0 | Active : false  ,  2 == 2 | Active : true
        if (NetworkServer.connections.Count > 0)
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                //VisibleDelegate//클라가 특정조건에 맞는 조건이 맞는지 델리게이트로 판단
                //                 맞으면 동기화  
                bool visible = visibleDelegate(ClientCaller);

                if (visible)
                {
                    //클라에게 값전달 
                    var SendData = new Test_Dele_ToClient(ClientData);
                    NetworkServer.connections[Keys[i]].Send(SendData);//Send Event To Clients > RecievfromServer;
                    Debug.Log("Try Send To Client");
                }
            }
        }
        else
        {
            Debug.Log("Try Send To Client - Error , Force Send All Client");
            ToClient(ClientData);
        }
    }
    void RecievfromServer(Test_Dele_ToClient msg)
    {
        Debug.Log("Recieve Client : " + msg.data);
        vauleDeleget(msg.data);// 이제 결과값 리턴
        Data = msg.data;

    }//생성자 에서 RegisterHandler등록해서 Send 사용시 이함수로

    [ClientRpc(includeOwner = true)]
    void ToClient(string Ldata)
    {
        Debug.Log("Recieve To Server");
        vauleDeleget(Ldata);
        Data = Ldata;
    }//연결될떼 NetworkServer.connections 갯수가 0일때가 있어서
}
