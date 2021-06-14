using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkAttach : NetworkBehaviour
{
    public bool Active = true;
    //[Header("Sync Object / Same Spawn Object(No Movement Sync After Spawn)")]
    //public bool SyncMode = false;// �������� �ߵǼ� ����ó�� �θ����� �̺�Ʈ�� �ص� �ɵ�

    [SyncVar(hook = nameof(OnChangeParent))]
    public GameObject Parent;//Sync Var���� �ٲٴ� �ʰ� ������ Ŭ�󿡰� ������ ���� �ڵ� ����

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
        //���� �ؼ� �θ𿡰� �� ��������
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    /*
    #region Temp
    [Client]
    public void Attach()
    {
        if (SyncMode)
        {
            if (GetComponent<NetworkIdentity>() == null)
            {
                Debug.LogWarning("ReQuire NetworkIdentityComponent - Stop Attach");
            }
            else
            {

            }
        }
        else
        {
            if (Parent == null)
            {
                Debug.LogWarning("Parent is Null - Stop Attach");
            }else
            {
                SetChildenCommand(Parent.GetComponent<NetworkIdentity>().netId);
            }
        }
    }

    [Command(requiresAuthority = false)]//requiresAuthority - �⺻�� True => �����ϰ� �ִ°͸� ��ɳ���
    void SetChildenCommand(uint ParentNetID)
    {
        rpcSetChilden(ParentNetID);
    }
    [ClientRpc()]//includeOwner - ���� �𸣰���
    void rpcSetChilden(uint ParentNetID)
    {
        //�θ�� NetID ���� , �װɷ� �θ� ã�� ����

        var ParentId = NetworkIdentity.spawned[ParentNetID];
        gameObject.transform.SetParent(ParentId.gameObject.transform);
    }
    #endregion
    *///�������� �Լ� ȣ���ؼ� ������Ʈ

    void OnChangeParent(GameObject oldObj, GameObject newObj)
    {
        Parent = newObj;
        if (Parent.GetComponent<NetworkIdentity>() != null)
        {
            var ParentID = Parent.GetComponent<NetworkIdentity>();
            gameObject.transform.SetParent(ParentID.gameObject.transform);

            Debug.Log("Change Parent");
        }
    }
}
