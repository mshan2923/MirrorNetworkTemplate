using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Test_Delegate//���� �۾��ϰ� object���� ��ȯ
{
    GameObject Caller;
    string Data;

    public delegate void VauleDeleget(string data);
    public VauleDeleget vauleDeleget;

    public delegate bool VaildDelegate(GameObject caller, string data);//Ŭ�� ����ɶ� �������� ��ȿ�� �Ǻ�
    public VaildDelegate vaildDelegate;

    public delegate bool VisibleDelegate(GameObject caller);//Ŭ�� Ư�����ǿ� �´� ������ �´���
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
        // ������� Ÿ�� ���ٰ��� �ϴϱ�
    }

    [Server]
    void SendDataToClient(GameObject ClientCaller, string ClientData)
    {
        int[] Keys = new int[NetworkServer.connections.Keys.Count];
        NetworkServer.connections.Keys.CopyTo(Keys, 0);
        Debug.Log("Connections : " + Keys.Length + " == " + NetworkServer.connections.Count + " | Active : " + NetworkServer.active);//---------����ǵ� 0 ...?// 0 == 0 �̶�µ�??
                                                                                                                                     // 0 == 0 | Active : false  ,  2 == 2 | Active : true
        if (NetworkServer.connections.Count > 0)
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                //VisibleDelegate//Ŭ�� Ư�����ǿ� �´� ������ �´��� ��������Ʈ�� �Ǵ�
                //                 ������ ����ȭ  
                bool visible = visibleDelegate(ClientCaller);

                if (visible)
                {
                    //Ŭ�󿡰� ������ 
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
        vauleDeleget(msg.data);// ���� ����� ����
        Data = msg.data;

    }//������ ���� RegisterHandler����ؼ� Send ���� ���Լ���

    [ClientRpc(includeOwner = true)]
    void ToClient(string Ldata)
    {
        Debug.Log("Recieve To Server");
        vauleDeleget(Ldata);
        Data = Ldata;
    }//����ɶ� NetworkServer.connections ������ 0�϶��� �־
}
