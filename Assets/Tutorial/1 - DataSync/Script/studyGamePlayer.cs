using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class studyGamePlayer : NetworkBehaviour
{
    Vector3 movement = new Vector3();

    //[SyncVar]                             //=====> 변수값 동기화 + 업데이트된 시점이 필요 없는경우
    [SyncVar(hook = nameof(SetColor))]   //=====>  변경시 해당함수 실행
    public Color32 _color = Color.white;

    #region Send Message , ( Client > Server , Server >>Clients )
    //[ClientCallback]//클라이언트만 작동 + 클라이언트 접근시 에러X
    [Client]//클라이언트만 작동
    private void Update()
    {
        if (!hasAuthority) { return; }//소유하고 있는지

        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                movement = Vector3.forward;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                movement = Vector3.back;
            }
            else
            {
                movement = Vector3.zero;
            }
        }//MoveDirection

        if (movement != Vector3.zero)
        {
            //transform.Translate(movement);
            //NetworkTransform으로 동기화 가능하지만  , Transform Lerp 적용되서

            CmdMove(movement);//서버에게 명령전달
        }
    }

    [Command]//클라이언트에서 호출 => 서버에서실행하는 메서드//requiresAuthority - 기본값 True => 소유하고 있는것만 명령내림
    void CmdMove(Vector3 vec)
    {
        //Validate logic here//유효확인 로직

        RpcMove(vec);//유효한지 확인하고 명령 수락 + 모든 클라이언트 동기화
    }
    [ClientRpc]// 모든 클라이언트 동기화 //includeOwner - 차이 모르겠음, 설명 없음
    void RpcMove(Vector3 vec)
    {
        transform.Translate(vec);
    }
    #endregion

    #region Variable Sync - 변수 동기화
    void SetColor(Color32 oldColor, Color32 newColor)
    {
        gameObject.GetComponent<Renderer>().material.color = newColor;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        StartCoroutine(RandomzieColor());
    }

    IEnumerator RandomzieColor()
    {
        while(true)
        {
            yield return new WaitForSeconds(2f);

            _color = Random.ColorHSV(0, 1, 1, 1, 0, 1, 1, 1);
            //변경시 서버에 변경된값을 업데이트
            //델리게이트 처럼 연결된 SetColor가 다른 클라이언트에서 새로운 값을 가지고 실행
        }
    }
    #endregion
}
