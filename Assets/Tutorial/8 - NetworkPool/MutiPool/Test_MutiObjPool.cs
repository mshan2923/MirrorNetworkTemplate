using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test_MutiObjPool : NetworkMutiObjectPool
{
    public override void RespawnEvent(int index, GameObject Obj)
    {
        if (isServer)
        {

        }

        base.RespawnEvent(index, Obj);
    }

    [Mirror.Command(requiresAuthority = false)]
    public void TestSpawn(int index)
    {
        Debug.Log("Call Spawn");
        Command_Spawn(index);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Test_MutiObjPool))]
public class TestMutiObjPoolEditor : Editor
{
    Test_MutiObjPool Onwer;
    public override void OnInspectorGUI()
    {
        Onwer = (Test_MutiObjPool)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Index 0 Spawn"))
        {
            Onwer.TestSpawn(0);
        }
        if (GUILayout.Button("Index 1 Spawn"))
        {
            Onwer.TestSpawn(1);
        }
    }
}
#endif