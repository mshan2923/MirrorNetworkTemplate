using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OptionalSync : NetworkBehaviour
{
    OptionalMessage data;
    //System.Type dataType;
    public string DebugVaule = "";
    public bool CoditionIgnore = true;

    public delegate bool OMessageDelegate(OptionalMessage Data);
    OMessageDelegate ValidEvent;    //��ȿ ��������Ʈ - Server Only
    public delegate bool ConditionDelegate(int ConnID);
    ConditionDelegate ConditionEvent;    //���� ��������Ʈ - Server Only
    public delegate void BoolDelegate(bool Updated);
    BoolDelegate SyncEvent;//         ����ȭ ��������Ʈ - Client Only


    public struct OptionalMessage : NetworkMessage
    {
        public string data;
        public void Set<T>(T Data)
        {
            data = JsonUtility.ToJson(new Wrap<T>(Data));
        }
        public T Get<T>()
        {
            if (string.IsNullOrEmpty(data))
                return default;
            else
                return JsonUtility.FromJson<Wrap<T>>(data).Get;
        }
    }//���ʸ� �ȵǼ� ����ȭ
    public OptionalMessage Convert<T>(T Data)
    {
        var SendData = new OptionalMessage();
        SendData.Set(Data);

        return SendData;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public void SetUp(OMessageDelegate Valid, ConditionDelegate Condition, BoolDelegate Sync)
    {
        ValidEvent = Valid;
        ConditionEvent = Condition;
        SyncEvent = Sync;
    }
    void Start()
    {
        if (isServerOnly)
            NetworkClient.RegisterHandler<OptionalMessage>(RecieveEvent, false);


        if (isClientOnly)
        {
            NetworkClient.ReplaceHandler<OptionalMessage>(RecieveEvent, false);

            StartCoroutine(WaitForClientReady());
        }

        //NetworkClient.connection.connectionId
        //NetworkServer.connections //Key : connectionId 

        //Command �Լ�, ���� ����X , ���ʸ�����X
        //ClientRPC - ������ ��� Ŭ�󿡰� ���(������ ����)
    }

    [Command(requiresAuthority = false)]
    public void RequestData(int ClientConnectionID)
    {
        NetworkServer.connections[ClientConnectionID].Send(data);
    }
    IEnumerator WaitForClientReady()
    {
        bool Bloop = true;

        while (Bloop)
        {
            yield return new WaitForEndOfFrame();

            Bloop = ! NetworkClient.connection.isReady;
        }
        if (NetworkClient.connection.isReady)
        {
            RequestData(NetworkClient.connection.connectionId);
            Debug.Log("Ready");
        }
    }

    public T Get<T>()
    {
        return data.Get<T>();
    }
    [Client]
    public void Set(OptionalMessage Data)
    {
        if (isClient)
        {
            DataToServer(Data, NetworkClient.connection.connectionId);
        }
    }
    [Command(requiresAuthority = false)]
    public void SetServer(OptionalMessage Data)//���ʸ� �ȵ�
    {
        SetData(Data);
    }

    [Server]
    void SetData(OptionalMessage Data)
    {
        int[] ConnID = new int[NetworkServer.connections.Count];
        NetworkServer.connections.Keys.CopyTo(ConnID, 0);
        bool SyncCodition = false;

        if (ValidEvent.Invoke(Data))
        {
            data = Data;

            if (CoditionIgnore)
            {
                SyncCodition = true;
            }
            else
            {
                for (int i = 0; i < ConnID.Length; i++)
                {
                    var ClientConn = NetworkServer.connections[ConnID[i]];

                    if (ValidEvent == null)
                    {
                        Debug.LogWarning("Need SetUp");
                    }
                    else
                    {
                        //===============��� Ŭ�� �����ؼ� , 1���� ������ �´°�� ��� Ŭ�󿡰� ������
                        //=============================================== �ݴ�� ���Ŭ�� ������ ���� ������� ������ ����(�ϰ� Ŭ�� ����ȭ�� ���ϸ�? �����Ѱ�?)
                        //ClientConn.Send(Data);//-> Ŭ�󿡰� ������ ������ ��� �� ����X => ���� ���� ���� , Ŭ��� ���� ���� �ٸ��� �ϴ°�...

                        if (ConditionEvent.Invoke(ConnID[i]))
                        {
                            SyncCodition = true;
                            break;
                        }
                    }
                }
            }
        }


        if (SyncCodition)
        {
            SetClientData(Data, true);
        }else
        {
            SetClientData(new OptionalMessage(), false);
        }

        SyncEvent.Invoke(SyncCodition);//SyncEvent.Invoke//->������ ����

        //���Ǹ´� Ŭ��鿡�� ������ ���� - ���� ��������Ʈ
    }

    [Command(requiresAuthority = false)]
    void DataToServer(OptionalMessage Data, int CallerID)
    {
        {/*
            //ConditionEvent.Invoke();
            if (ConditionEvent == null)
            {
                Debug.LogWarning("Need SetUp");
            }
            else
            {
                if (CoditionIgnore)
                {
                    SetData(Data);
                    ClientDataCondition(true);
                }
                else
                {
                    int[] ConnID = new int[NetworkServer.connections.Count];
                    NetworkServer.connections.Keys.CopyTo(ConnID, 0);

                    for (int i = 0; i < ConnID.Length; i++)
                    {
                        //ConditionEvent.Invoke �ؼ�
                        //�ϳ��� ���ΰ�� -> �ߴ��� ������ ������, Ŭ��� ����ȭ | ���� ���� -> ������ �� ����
                    }

                    {
                        bool Lcondition = ConditionEvent.Invoke(CallerID);
                        if (Lcondition)//��ȿ���� Ȯ�� - ��ȿ ��������Ʈ
                        {
                            SetData(Data);
                        }
                        ClientDataCondition(Lcondition);//===============��� Ŭ�� �����ؼ� , 1���� ������ �´°�� ��� Ŭ�󿡰� ������
                                                        //=============================================== �ݴ�� ���Ŭ�� ������ ���� ������� ������ ����(�ϰ� Ŭ�� ����ȭ�� ���ϸ�? �����Ѱ�?)
                    }//Disable
                }
            }*/
        }
        SetData(Data);
    }//SetData �� �ߺ���, Ŭ��� (Set > DataToServer > SetData) ���� ����

    public void RecieveEvent(OptionalMessage message)
    {
        data = message;
        DebugVaule = message.data;

        Debug.Log("Recieve : " + message.data);
    }
    [ClientRpc]
    void ClientDataCondition(bool Condition)
    {
        if (SyncEvent != null)
            SyncEvent.Invoke(Condition);//-------�̰� ������ ��...
        else
            Debug.LogWarning("Need SetUp");
    }
    [ClientRpc(includeOwner = true)]// ������ Ŭ���Լ� ��� (���� ����)
    void SetClientData(OptionalMessage message, bool SyncCodition)
    {
        if (SyncCodition)
        {
            data = message;
            DebugVaule = message.data;
        }
        SyncEvent.Invoke(SyncCodition);
    }
}
