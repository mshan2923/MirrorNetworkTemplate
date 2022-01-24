using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class SimpleInputMovement : NetworkBehaviour, MainInputSystem.IPlayerActions
{
    MainInputSystem inputActions;
    Vector2 inputVector;

    [SerializeField, SyncVar(hook = nameof(ChangedSpeed))]
    float MoveSpeed = 5f;//����ȭ ���Ұ�� �������� , ���� �����Ϳ��� �����ϸ� hook �̺�Ʈ�� �ȶ�

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new MainInputSystem();
        }
    }
    private void OnDisable()
    {
        if (hasAuthority)
            inputActions.Player.Disable();
    }

    void ChangedSpeed(float oldSpeed, float newSpeed)
    {
        Debug.LogWarning("!");
        DebugMessage("Change Speed / " + oldSpeed + " >> " + newSpeed);
    }
    [ClientRpc(includeOwner = true)]
    void DebugMessage(string text)
    {
        Debug.LogWarning(text);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }
    [Command]
    void CmdMove(Vector2 normalized)
    {
        //��ȿ�� üũ
        Vector3 Lpos = new Vector3(normalized.normalized.x, 0, normalized.normalized.y) * MoveSpeed;

        RpcMove(Lpos);
    }
    [ClientRpc(includeOwner = true)]
    void RpcMove(Vector3 pos)
    {
        gameObject.transform.position += pos * Time.deltaTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (hasAuthority)//hasAuthority �� Enable�϶��� ������ ���� ������
        {
            inputActions.Player.SetCallbacks(instance: this);
            inputActions.Player.Enable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority)
            CmdMove(inputVector);
    }
}
