using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test_MutiObjPool : NetworkMutiObjectPool
{
    public override void RespawnEvent(int index, GameObject Obj)
    {
        Obj.transform.SetParent(gameObject.transform);

        if (isServer)
        {
            int amount = ObjPools[index].ActivePool;

            //Obj.transform.position = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 5f;
            Obj.transform.position = new Vector3((amount + amount * 0.5f), 0, index);
        }

        base.RespawnEvent(index, Obj);
    }

    [Mirror.Command(requiresAuthority = false)]
    public void TestSpawn(int index)
    {
        Command_Spawn(index);
    }
    [Mirror.Command(requiresAuthority = false)]
    public void TestDespawn(int index)
    {
        int last = ObjPools[index].PoolList.Count;

        if (ObjPools[index].PoolList.Count > 0)
            Command_DeSpawn(index, ObjPools[index].PoolList[last - 1].GetComponent<NetworkIdentity>().netId);
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

        if (GUILayout.Button("Index 0 DeSpawn"))
        {
            Onwer.TestDespawn(0);
        }
        if (GUILayout.Button("Index 1 DeSpawn"))
        {
            Onwer.TestDespawn(1);
        }
    }
}
#endif