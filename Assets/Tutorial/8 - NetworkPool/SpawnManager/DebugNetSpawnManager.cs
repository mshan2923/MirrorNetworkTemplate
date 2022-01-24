using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class DebugNetSpawnManager : NetworkSpawnManager
{
    #region Debug_Client
    [Command()]
    public void CallSpawn(int index)
    {
        Spawn(index);
    }
    [Command()]
    public void CallDespawn(int index, GameObject obj)
    {
        Despawn(index, obj);
    }
    [Command()]
    public void CallAutoDespawn(int index)
    {
        if (Spawns[0].ActiveList.Count > 0)
        {
            Despawn(index, Spawns[0].ActiveList[0]);
        }
    }//리턴값을 가질 수 없음
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(DebugNetSpawnManager))]
public class DebugSpawnManagerEditor : Editor
{
    DebugNetSpawnManager Onwer;
    public override void OnInspectorGUI()
    {
        Onwer = target as DebugNetSpawnManager;

        base.OnInspectorGUI();

        if (GUILayout.Button("Set SpawnObject"))
        {
            var netManager = FindObjectOfType<NetworkManager>();

            netManager.spawnPrefabs.Add(Onwer.gameObject);

            for (int i = 0; i < Onwer.Spawns.Count; i++)
            {
                //NetworkManager.singleton.spawnPrefabs.Add(Onwer.Spawns[i].SpawnObject);//인게임에선

                if (!netManager.spawnPrefabs.Exists(t => t == Onwer.Spawns[i].SpawnObject))
                {
                    netManager.spawnPrefabs.Add(Onwer.Spawns[i].SpawnObject);
                }
            }
        }

        if (GUILayout.Button("Spawn 0"))
        {
            if (Onwer.isServer)
            {
                Onwer.Spawn(0);
                NetworkIdentity.print("Try Spawn From Server ");
            }
            else if (Onwer.isClient)
            {
                //CallSpawn(0);
                Onwer.CallSpawn(0);
                NetworkIdentity.print("Try Spawn From Client - Authority : " + Onwer.isLocalPlayer);
            }
        }

        if (GUILayout.Button("Despawn 0"))
        {
            if (Onwer.isServer)
            {
                if (Onwer.Spawns[0].ActiveList.Count > 0)
                    Onwer.Despawn(0, Onwer.Spawns[0].ActiveList[0]);
            }
            else if (Onwer.isClient)
            {
                Onwer.CallAutoDespawn(0);
                //Onwer.Despawn(0, Onwer.Spawns[0].ActiveList[0]);
            }
        }

        EditorGUILayout.HelpBox("Must Add NetManager.SpawnPrefabs", MessageType.Info);
    }

    //여기서 Command 함수 쓸경우//클라에서 서버에게 넘겼는데 왜? 실행이 -> 서버가 활성화 안됨 =>> UNITY_EDITOR 때문에 빌드시 제거됨
    //Command 함수 리턴형이 무조건 void 이여야함
}
#endif