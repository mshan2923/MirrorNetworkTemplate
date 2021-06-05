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
    //넓게 랜덤스폰하고 , 색상 랜덤으로 변함  => 활성화 이벤트 받기 
    //일단 해보고 동기화 문제 있으면 뜯어고치기
    //문제1 : 스폰시 설정값 밖에 있는거 동기화됨 , 범위안에 들어오고 나가면 적용

    //플레이어가 자동으로 움직여 적용 결과 보여주게

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
                //OnChangeColorClient(objId.netId, color);//수동 동기화이니깐 근접검사기에 반응X  |  SyncVar로 한다면 동기화 될려나?
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
