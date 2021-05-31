using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkAttach : NetworkBehaviour
{
    public bool Active = true;
    //[Header("Sync Object / Same Spawn Object(No Movement Sync After Spawn)")]
    //public bool SyncMode = false;// 생각보다 잘되서 지금처럼 부모지정 이벤트만 해도 될듯

    [SyncVar(hook = nameof(OnChangeParent))]
    public GameObject Parent;//Sync Var으로 바꾸니 늦게 참여한 클라에게 서버의 정보 자동 전달

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
        //시작 해서 부모에게 값 가져오기
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

    [Command(requiresAuthority = false)]//requiresAuthority - 기본값 True => 소유하고 있는것만 명령내림
    void SetChildenCommand(uint ParentNetID)
    {
        rpcSetChilden(ParentNetID);
    }
    [ClientRpc()]//includeOwner - 차이 모르겠음
    void rpcSetChilden(uint ParentNetID)
    {
        //부모는 NetID 보유 , 그걸로 부모 찾아 연결

        var ParentId = NetworkIdentity.spawned[ParentNetID];
        gameObject.transform.SetParent(ParentId.gameObject.transform);
    }
    #endregion
    *///수동으로 함수 호출해서 업데이트

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
