using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkObjectPool : NetworkBehaviour
{
    Pool<GameObject> pool;//Func == (���ʸ� ��������Ʈ + �Է� �Ķ���� ����)//
    //Ŭ�󿡰� NetID���� ��Ȱ��ȭ,Ȱ��ȭ 
    //List<uint> PoolObjNetID;//��¼�� �������� Pool�� �����ϴϱ�... �ʿ� ���� ������?

    [Header("Don't Forget Add NetworkManager.SpawnablePrefabs")]
    public GameObject PoolObject;
    public int ActivePool = 0;
    public int PoolAmount = 0;//0���ϴ� ������



    private void OnEnable()
    {

    }
    void Start()
    {
        if (isServer)
        {
            pool = new Pool<GameObject>(Generator);
            //PoolObjNetID = new List<uint>();
        }
    }
    [Server]
    private GameObject Generator()
    {
        Debug.Log("Generator Event");

        GameObject obj = GameObject.Instantiate(PoolObject, Vector3.zero, Quaternion.identity);
        //PoolObjNetID.Add(obj.GetComponent<NetworkIdentity>().netId);//PoolObjNetID �� ���� �Է�
        EventGenrate(obj);
        NetworkServer.Spawn(obj);
        return obj;

        //Pool���� �������ų� ȸ���ɶ� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ ����
    }

    public virtual void EventGenrate(GameObject Obj)
    {
        Obj.SetActive(true);//������Ʈ Ȱ��ȭ ---- �̰� ����ȭ �ȵ�

        if (isServer)
        {
            UpdateObjectPosition(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);
        }
        //�����κ� �� Ŭ��κ����� ����
        //�������ص� ����ȭ�Ǿ� �������� ������ Ȯ���� �ϱ�����
    }//����ؼ� Override���� ���� - ������ Object ����(�ַ� Transform)

    [ClientRpc(includeOwner = true)]
    void UpdateObjectPosition(uint netID, Vector3 Pos, Quaternion Rot)
    {
        if (NetworkIdentity.spawned.ContainsKey(netID))
        {
            var obj = NetworkIdentity.spawned[netID].gameObject;
            obj.transform.position = Pos;
            obj.transform.rotation = Rot;
        }
    }//��ġ Ŭ�� ����ȭ

    [Server]
    public GameObject Spawn()
    {
        GameObject obj = null;
        if (pool == null)
        {
            pool = new Pool<GameObject>(Generator);
        }

        if (ActivePool < PoolAmount || PoolAmount < 0)
        {
            Debug.Log("Spawn Start");
            obj = pool.Take();

            EventGenrate(obj);
            if (obj.GetComponent<NetworkIdentity>() != null)
            {
                ActiveClientObject(obj.GetComponent<NetworkIdentity>().netId);//Ŭ��鿡�� EventGenrate����  + Obj��  NetID ����
            }
            ActivePool++;
        }

        return obj;
    }
    [ClientRpc(includeOwner = true)]
    void ActiveClientObject(uint netID)
    {
        var id = NetworkIdentity.spawned[netID];

        EventGenrate(id.gameObject);
    }

    //======================

    [Server]
    public void Despawn(GameObject obj)
    {
        if (ActivePool > 0)
        {
            pool.Return(obj);
            DeactiveClientObject(obj.GetComponent<NetworkIdentity>().netId);//Ŭ��鿡�� ��Ȱ��ȭ �˸�
            ActivePool--;
        }
    }
    [ClientRpc(includeOwner = true)]
    void DeactiveClientObject(uint netID)
    {
        var id = NetworkIdentity.spawned[netID];

        id.gameObject.SetActive(false);
    }
}
