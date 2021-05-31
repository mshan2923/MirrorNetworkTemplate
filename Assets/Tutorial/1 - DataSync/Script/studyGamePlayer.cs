using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class studyGamePlayer : NetworkBehaviour
{
    Vector3 movement = new Vector3();

    //[SyncVar]                             //=====> ������ ����ȭ + ������Ʈ�� ������ �ʿ� ���°��
    [SyncVar(hook = nameof(SetColor))]   //=====>  ����� �ش��Լ� ����
    public Color32 _color = Color.white;

    #region Send Message , ( Client > Server , Server >>Clients )
    //[ClientCallback]//Ŭ���̾�Ʈ�� �۵� + Ŭ���̾�Ʈ ���ٽ� ����X
    [Client]//Ŭ���̾�Ʈ�� �۵�
    private void Update()
    {
        if (!hasAuthority) { return; }//�����ϰ� �ִ���

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
            //NetworkTransform���� ����ȭ ����������  , Transform Lerp ����Ǽ�

            CmdMove(movement);//�������� �������
        }
    }

    [Command]//Ŭ���̾�Ʈ���� ȣ�� => �������������ϴ� �޼���//requiresAuthority - �⺻�� True => �����ϰ� �ִ°͸� ��ɳ���
    void CmdMove(Vector3 vec)
    {
        //Validate logic here//��ȿȮ�� ����

        RpcMove(vec);//��ȿ���� Ȯ���ϰ� ��� ���� + ��� Ŭ���̾�Ʈ ����ȭ
    }
    [ClientRpc]// ��� Ŭ���̾�Ʈ ����ȭ //includeOwner - ���� �𸣰���, ���� ����
    void RpcMove(Vector3 vec)
    {
        transform.Translate(vec);
    }
    #endregion

    #region Variable Sync - ���� ����ȭ
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
            //����� ������ ����Ȱ��� ������Ʈ
            //��������Ʈ ó�� ����� SetColor�� �ٸ� Ŭ���̾�Ʈ���� ���ο� ���� ������ ����
        }
    }
    #endregion
}
