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
