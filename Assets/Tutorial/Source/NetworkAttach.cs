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
