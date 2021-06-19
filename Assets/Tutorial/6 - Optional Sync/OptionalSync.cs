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
    OMessageDelegate ValidEvent;    //유효 델리게이트 - Server Only
    public delegate bool ConditionDelegate(int ConnID);
    ConditionDelegate ConditionEvent;    //조건 델리게이트 - Server Only
    public delegate void BoolDelegate(bool Updated);
    BoolDelegate SyncEvent;//         동기화 델리게이트 - Client Only


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
    }//제너릭 안되서 직렬화
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

        //Command 함수, 리턴 지원X , 제너릭지원X
        //ClientRPC - 서버가 모든 클라에게 명령(소유권 무시)
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
    public void SetServer(OptionalMessage Data)//제너릭 안됨
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
                        //===============모든 클라 조건해서 , 1개라도 조건이 맞는경우 모든 클라에게 보여줌
                        //=============================================== 반대로 모든클라가 조건이 맞지 않은경우 서버만 변경(하고 클라 동기화만 안하면? 가능한가?)
                        //ClientConn.Send(Data);//-> 클라에게 값전달 권한이 없어서 값 변경X => 전부 값을 전달 , 클라들 끼리 서로 다른값 하는건...

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

        SyncEvent.Invoke(SyncCodition);//SyncEvent.Invoke//->데이터 적용

        //조건맞는 클라들에게 데이터 전달 - 조건 델리게이트
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
                        //ConditionEvent.Invoke 해서
                        //하나라도 참인경우 -> 중단후 서버에 값적용, 클라들 동기화 | 전부 거짓 -> 서버만 값 적용
                    }

                    {
                        bool Lcondition = ConditionEvent.Invoke(CallerID);
                        if (Lcondition)//유효한지 확인 - 유효 델리게이트
                        {
                            SetData(Data);
                        }
                        ClientDataCondition(Lcondition);//===============모든 클라 조건해서 , 1개라도 조건이 맞는경우 모든 클라에게 보여줌
                                                        //=============================================== 반대로 모든클라가 조건이 맞지 않은경우 서버만 변경(하고 클라 동기화만 안하면? 가능한가?)
                    }//Disable
                }
            }*/
        }
        SetData(Data);
    }//SetData 와 중복됨, 클라는 (Set > DataToServer > SetData) 으로 접근

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
            SyncEvent.Invoke(Condition);//-------이걸 서버로 음...
        else
            Debug.LogWarning("Need SetUp");
    }
    [ClientRpc(includeOwner = true)]// 서버가 클라함수 명령 (소유 무시)
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
