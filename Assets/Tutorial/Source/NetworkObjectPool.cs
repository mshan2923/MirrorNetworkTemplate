using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkObjectPool : NetworkBehaviour
{
    Pool<GameObject> pool;//Func == (제너릭 델리게이트 + 입력 파라미터 없음)//
    //클라에게 NetID으로 비활성화,활성화 
    //List<uint> PoolObjNetID;//어쩌피 서버에서 Pool을 관리하니깐... 필요 없지 않을까?

    [Header("Don't Forget Add NetworkManager.SpawnablePrefabs")]
    public GameObject PoolObject;
    public int ActivePool = 0;
    public int PoolAmount = 0;//0이하는 무제한



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
        //PoolObjNetID.Add(obj.GetComponent<NetworkIdentity>().netId);//PoolObjNetID 에 정보 입력
        EventGenrate(obj);
        NetworkServer.Spawn(obj);
        return obj;

        //Pool에서 가져오거나 회수될때 오브젝트 활성화/비활성화 없음
    }

    public virtual void EventGenrate(GameObject Obj)
    {
        Obj.SetActive(true);//오브젝트 활성화 ---- 이건 동기화 안됨

        if (isServer)
        {
            UpdateObjectPosition(Obj.GetComponent<NetworkIdentity>().netId, Obj.transform.position, Obj.transform.rotation);
        }
        //서버부분 과 클라부분으로 나뉨
        //구별안해도 동기화되어 같아지긴 하지만 확실히 하기위해
    }//상속해서 Override으로 구현 - 생성된 Object 설정(주로 Transform)

    [ClientRpc(includeOwner = true)]
    void UpdateObjectPosition(uint netID, Vector3 Pos, Quaternion Rot)
    {
        if (NetworkIdentity.spawned.ContainsKey(netID))
        {
            var obj = NetworkIdentity.spawned[netID].gameObject;
            obj.transform.position = Pos;
            obj.transform.rotation = Rot;
        }
    }//위치 클라 동기화

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
                ActiveClientObject(obj.GetComponent<NetworkIdentity>().netId);//클라들에게 EventGenrate실행  + Obj의  NetID 전달
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
            DeactiveClientObject(obj.GetComponent<NetworkIdentity>().netId);//클라들에게 비활성화 알림
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
