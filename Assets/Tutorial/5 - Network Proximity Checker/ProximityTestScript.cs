using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct ColorMessage : NetworkMessage
{
    public uint Target;
    public Color color;
}

public class ProximityTestScript : NetworkBehaviour
{
    //�а� ���������ϰ� , ���� �������� ����  => Ȱ��ȭ �̺�Ʈ �ޱ� 
    //�ϴ� �غ��� ����ȭ ���� ������ ����ġ��
    //����1 : ������ ������ �ۿ� �ִ°� ����ȭ�� , �����ȿ� ������ ������ ����

    //�÷��̾ �ڵ����� ������ ���� ��� �����ְ�

    //public DistanceInterestManagement interestManagement;

    public GameObject TestObj;
    public float ObjMaxDistance = 5;
    public int ObjAmount = 50;

    List<GameObject> Objs = new List<GameObject>();

    public float ChangeDelay = 2f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        //NetworkClient.RegisterSpawnHandler
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < ObjAmount; i++)
        {
            GameObject temp = GameObject.Instantiate(TestObj);
            temp.transform.position = new Vector3(Random.Range(-1f, 1f), gameObject.transform.position.y, Random.Range(-1f, 1f)) * ObjMaxDistance * 2;
            NetworkServer.Spawn(temp);

            Objs.Add(temp);
        }

        StartCoroutine(ChangeColorLoop());
    }

    IEnumerator ChangeColorLoop()
    {
        yield return new WaitForSeconds(ChangeDelay);

        for (int i = 0; i < ObjAmount; i++)
        {
            Color color = Random.ColorHSV(0, 1, 1, 1, 0, 1, 1, 1);
            //Objs[i].GetComponent<Renderer>().material.color = color;

            var objId = Objs[i].GetComponent<NetworkIdentity>();

            if (objId != null && isServer)
            {
                //OnChangeColorClient(objId.netId, color);//���� ����ȭ�̴ϱ� �����˻�⿡ ����X  |  SyncVar�� �Ѵٸ� ����ȭ �ɷ���?
                Objs[i].GetComponent<TestObjColor>().color = color;
            }
        }

        StartCoroutine(ChangeColorLoop());
    }

    [Command(requiresAuthority = false)]
    void CmdOnChangeColor(uint NetID, Color color)
    {
        OnChangeColorClient(NetID, color);
    }
    [ClientRpc(includeOwner = true)]
    void OnChangeColorClient(uint NetID, Color color)
    {
        if (NetworkIdentity.spawned.ContainsKey(NetID))
        {
            var ObjId = NetworkIdentity.spawned[NetID];
            ObjId.gameObject.GetComponent<Renderer>().material.color = color;
        }else
        {
            Debug.LogWarning("?? : " + NetID);
        }
    }
}
