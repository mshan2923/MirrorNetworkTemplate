using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;


public class OptionalSyncVar : NetworkBehaviour
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

    [AttributeMask("Distance", "Dot", "Target Variable"), Space(10)]
    public int CalculateTypeMask = 0;

    private void Start()
    {/*
        if (isServer)
        {
            SyncData.changeData = new AutoVarAccess.ChangeDataDelegate(ChangeSyncData);
            SyncData.ChangeData = ChangeSyncData;

            Debug.Log("Set Delegate : " + (SyncData.ChangeData != null));
        }*/
    }

    public void ChangeSyncData(object oldData, object newData)
    {
        //(AutoVarAccess.ChangeDataEvent 에서 유효검사 )
        //자신포함 조건맞는 클라이어트에게만 데이터 전달
        //전달 받은거 데이터 넘기고 AutoVarAccess.Get<>에서 확인

        Debug.Log("Receive event - Server | " + newData);
        if (NetworkServer.connections.Count > 0)
            RPCTemp(newData);

        SyncData.Data = newData;
    }//Call Path { AutoVarAccess.SyncData < ChangeDataEvent < Set }
    // SyncData 쓸때 AutoVarAccess.Data로 자동 형전환

    [ClientRpc(includeOwner = true)]
    void RPCTemp(object Data)
    {
        Debug.Log("Receive Client : " + Data.GetType() + " : " + Data.ToString());
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OptionalSyncVar))]
public class SyncVarAttributeDrawer : Editor
{
    OptionalSyncVar Onwer;

    Component CAccess = null;
    string AceesVarName = "";

    Component CTarget = null;
    string TargetVarName = "";

    private void OnEnable()
    {
        Onwer = target as OptionalSyncVar;
        /*
        if (Onwer.AccessSync != null && !string.IsNullOrEmpty(Onwer.SyncVarName))
            Onwer.SyncData = new AutoVarAccess(Onwer.AccessSync, Onwer.SyncVarName);

        if (Onwer.AccessTarget != null && !string.IsNullOrEmpty(Onwer.TargetVarName))
            Onwer.TargetToggleData = new AutoVarAccess(Onwer.AccessTarget, Onwer.TargetVarName, typeof(bool));
        */
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
