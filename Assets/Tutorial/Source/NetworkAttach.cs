using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public interface IAttach
{
    [Server]
    public void Attach(GameObject Parent, Vector3 Pos, Quaternion Rot);//=====�������̽��� ������ �Ǵ��� Ȯ��
    [Server]
    public void Detach();
}
public class NetworkAttach : NetworkBehaviour, IAttach
{

    //���϶� ��ġ���� ������Ʈ �Ϸ�
    //������ ���λ��� ������Ʈ �ʿ� , �θ��� netID�� �����ϰ� 
    //���� ������ ��ġ���� , Ŭ�� ������ �и� �߰� ,  , �и�(IAttach �ִ°͸� �и�)
    //============ �θ� �ٲٴ°�? - �Ƹ� �ɵ�

    [System.Serializable]
    public struct AttachData : NetworkMessage
    {
        public uint ParentID;//0 is Null
        public Vector3 Pos;
        public Quaternion Rot;
        public Vector3 Scale;

        public AttachData(uint parentID, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            ParentID = parentID;
            Pos = pos;
            Rot = rot;
            Scale = scale;
        }
    }

    /// IAttach ���� �̺�Ʈ ���� > SyncVar hook���� ChangeData ���� > RPC���� �θ�� Transform ����

    [SerializeField]
    uint ParentID;

    [SerializeField, SyncVar(hook = nameof(ChangeData))]
    AttachData Data;

    Vector3 ParentPosition;

    public delegate void CompleteDetach(NetworkAttach attach , uint ParnetID);
    public CompleteDetach OnCompleteDetach;

    /// <summary>
    /// Parent need NetworkTransformChild
    /// </summary>
    [Server]
    public void Attach(GameObject Parent, Vector3 LocalPos, Quaternion Rot)
    {
        ParentID = Parent.GetComponent<NetworkIdentity>().netId;
        Data = new AttachData(ParentID, LocalPos, Rot, Vector3.one);
    }
    [Server]
    public void Detach()
    {
        NetworkManager.print("Is Enable " + Data.ParentID + " : " + NetworkServer.spawned.ContainsKey(Data.ParentID));// result : 2, 4 is enable
        NetworkManager.print("Is Enable " + gameObject.GetComponent<NetworkIdentity>().netId +
            " : " + NetworkServer.spawned.ContainsKey(gameObject.GetComponent<NetworkIdentity>().netId));

        ParentPosition = NetworkServer.spawned[Data.ParentID].gameObject.transform.position;
        Data = new AttachData(0, Data.Pos, Data.Rot, Data.Scale);
    }


    [ClientRpc(includeOwner = true)]
    void RpcSetParent(uint parentID)
    {
        if (NetworkClient.spawned.ContainsKey(parentID))
            gameObject.transform.SetParent(NetworkClient.spawned[parentID].gameObject.transform);
        else
        {
            gameObject.transform.SetParent(null);
            NetworkManager.print("Not Found Parent");
        }
        //gameObject.transform.SetParent(Parent.transform);

        if (OnCompleteDetach != null && NetworkClient.isHostClient)
        {
            OnCompleteDetach.Invoke(this, ParentID);// �������� �и��� �ɶ� ��������Ʈ Invoke �ؼ� ���� ����
        }
    }

    void ChangeData(AttachData oldData, AttachData newData)
    {
        if (isServer)
        {
            NetworkManager.print(oldData.ParentID.ToString() + " >> " + newData.ParentID.ToString());

            if (NetworkServer.spawned.ContainsKey(newData.ParentID))//newData.ParentID > uint.MinValue
            {
                var ParentObj = NetworkServer.spawned[newData.ParentID].gameObject;

                RpcSetParent(newData.ParentID);
                gameObject.GetComponent<NetworkTransform>().RpcTeleportAndRotate((ParentObj.transform.position + newData.Pos), newData.Rot);//�θ���ġ + newData.Pos

                if (NetworkServer.spawned.ContainsKey(oldData.ParentID))//oldData.ParentID > uint.MinValue
                {

                }//Change Parent
                else
                {

                }//Attach
            }
            else
            {
                Vector3 ParentPos = NetworkServer.spawned[oldData.ParentID].gameObject.transform.position;
                //Quaternion ParentRot = NetworkServer.spawned[oldData.ParentID].gameObject.transform.rotation;

                RpcSetParent(0);
                gameObject.GetComponent<NetworkTransform>().RpcTeleportAndRotate((ParentPos + oldData.Pos), oldData.Rot);

                if (NetworkServer.spawned.ContainsKey(oldData.ParentID))
                {

                }//Detach
                else
                {

                }//RemovedParent
            }

            if (OnCompleteDetach != null)
            {
                //OnCompleteDetach.Invoke(this, oldData.ParentID);
            }
        }else
        {
            if (NetworkClient.spawned.ContainsKey(newData.ParentID))
            {
                gameObject.transform.SetParent(NetworkClient.spawned[newData.ParentID].gameObject.transform);

                //gameObject.GetComponent<NetworkTransform>().CmdTeleportAndRotate(newData.Pos, newData.Rot);
                //��ġ ����ȭ - �������� ��ȣ�� ������ > �������� RPCTeleport

                OnNeedSyncTransform();
            }
            else
            {
                NetworkManager.print("Not Found Parent");

                gameObject.transform.SetParent(null);

                OnNeedSyncTransform();
            }
        }
    }

    [Command(requiresAuthority = false)]
    void OnNeedSyncTransform()
    {
        if (NetworkServer.spawned.ContainsKey(Data.ParentID))
        {
            var ParentObj = NetworkServer.spawned[Data.ParentID].gameObject;

            gameObject.GetComponent<NetworkTransform>().RpcTeleportAndRotate((ParentObj.transform.position + Data.Pos), Data.Rot);//�θ���ġ + Data.Pos
        }else
        {
            //ParentPosition
            gameObject.GetComponent<NetworkTransform>().RpcTeleportAndRotate((ParentPosition + Data.Pos), Data.Rot);
        }
    }
}
