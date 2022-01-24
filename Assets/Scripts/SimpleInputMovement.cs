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
    float MoveSpeed = 5f;//동기화 안할경우 변조가능 , 설마 에디터에서 변경하면 hook 이벤트가 안뜸

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
        //유효값 체크
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
        if (hasAuthority)//hasAuthority 이 Enable일때는 소유권 지정 아직임
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
