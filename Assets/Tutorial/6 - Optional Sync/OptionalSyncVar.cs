using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;


public class OptionalSyncVar : NetworkBehaviour//============제너릭 클래스, 상속용버전으로 나눌거임 , 일단 이건 상속버전으로
{
    [Header("Optional Sync Var")]
    public Component AccessSync;
    public string SyncVarName;
    public AutoVarAccess SyncData;//값변경 이벤트(Sync 대상이 값변경될때) , 델리게이트 연결

    [Header("Access Calculate Variable")]
    public Component AccessTarget;
    public string TargetVarName;

    [Header("Calculate Parameter")]
    public AutoVarAccess TargetToggleData;//타입제한 (bool)
    [Space(5)]
    public float Distance = 1f;//         Target.gameObject ~ gameobject간 거리
    [Space(5)]
    public float DotAngle = 60f;//        (Target.gameObject - gameobject) => 방향
    public bool ReverseDot = false;

#if UNITY_EDITOR
    [Expand.AttributeMask("Distance", "Dot", "Target Variable"), Space(10)]
#endif
    public int CalculateTypeMask = 0;

    public virtual void Start()
    {
        if (isServer)
        {
            SyncData.ChangeData = ChangeSyncData;//이거 실행되고 에디터에서 초기화
        }
    }
    [Server]
    public virtual object ChangeSyncData(object oldData, object newData)
    {
        //자신포함 조건맞는 클라이어트에게만 데이터 전달
        //전달 받은거 데이터 넘기고 AutoVarAccess.Get<>에서 확인

        Debug.Log("Receive event - Server | " + newData);
        RPCTemp(newData);

        SyncData.Data = newData;

        //유효값 검사
        return newData;
    }//Call Path { AutoVarAccess.SyncData < ChangeDataEvent < Set }
    // SyncData 쓸때 AutoVarAccess.Data로 자동 형전환

    [ClientRpc(includeOwner = true)]
    public virtual void RPCTemp(object Data)
    {
        Debug.LogWarning("Receive Client : " + Data.GetType() + " : " + Data.ToString() + " | " + (string)Data);
        SyncData.Data = Data;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OptionalSyncVar))]
public class SyncVarAttributeDrawer : Editor//PropertDrawer으로... > 프로퍼티.isDirty으로 변경확인
{
    OptionalSyncVar Onwer;

    Component CAccess = null;
    string AceesVarName = "";

    Component CTarget = null;
    string TargetVarName = "";

    private void OnEnable()
    {
        Onwer = target as OptionalSyncVar;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        {
            if (Onwer.SyncData != null)
            {
                if (Onwer.AccessSync == null || string.IsNullOrEmpty(Onwer.SyncVarName))
                    Onwer.SyncData = null;
            }

            if (Onwer.TargetToggleData != null)
            {
                if (Onwer.AccessTarget == null || string.IsNullOrEmpty(Onwer.TargetVarName))
                    Onwer.TargetToggleData = null;
            }
        }//유효한 값이 아닐때

        {
            if (Onwer.AccessSync != CAccess || Onwer.SyncVarName != AceesVarName)
            {
                Onwer.SyncData = new AutoVarAccess(Onwer.AccessSync, Onwer.SyncVarName, Onwer.ChangeSyncData);

                CAccess = Onwer.AccessSync;
                AceesVarName = Onwer.SyncVarName;
            }

            if (Onwer.AccessTarget != CTarget || Onwer.TargetVarName != TargetVarName)
            {
                Onwer.TargetToggleData = new AutoVarAccess(Onwer.AccessTarget, Onwer.TargetVarName, Onwer.ChangeSyncData, typeof(bool));

                CTarget = Onwer.AccessTarget;
                TargetVarName = Onwer.TargetVarName;
            }
        }//값이 변할때

        if (GUILayout.Button("Testing"))
        {
            //var temp = new GenericVar(typeof(string), UnityEngine.Random.value.ToString());
            //Onwer.SyncData.Set(temp);

            Onwer.SyncData.Set(UnityEngine.Random.value.ToString());
        }
    }
}
#endif
